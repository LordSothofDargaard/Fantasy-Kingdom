using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class PlanetGenerator : Node3D
{
    [Export]
    public NodePath MeshInstancePath { get; set; } = "PlanetMesh";

    [Export]
    public NodePath CollisionBodyPath { get; set; } = "PlanetCollision";

    [Export]
    public bool RebuildCollision { get; set; } = false;

    public PlanetSettings CurrentSettings { get; private set; } = new();

    private MeshInstance3D _meshInstance = null;

    public override void _Ready()
    {
        _meshInstance = ResolveMeshInstance();
    }

    public void Generate(PlanetSettings settings)
    {
        CurrentSettings = settings ?? new PlanetSettings();
        _meshInstance ??= ResolveMeshInstance();

        IcosphereData icosphere = BuildIcosphere(Mathf.Max(0, CurrentSettings.GetSubdivisionCount()));
        LandmassPreset landmassPreset = ResolveLandmassPreset(CurrentSettings);

        NoiseField terrainNoise = BuildNoise(CurrentSettings.TerrainSeed, CurrentSettings.NoiseScale, 5, 2.0f, 0.5f);
        NoiseField continentNoise = BuildNoise(CurrentSettings.ContinentSeed, landmassPreset.ContinentNoiseScale, 3, 2.0f, 0.55f);
        NoiseField continentWarpX = BuildNoise(CurrentSettings.ContinentSeed + 1001, landmassPreset.ContinentWarpScale, 2, 2.0f, 0.5f);
        NoiseField continentWarpY = BuildNoise(CurrentSettings.ContinentSeed + 2003, landmassPreset.ContinentWarpScale, 2, 2.0f, 0.5f);
        NoiseField continentWarpZ = BuildNoise(CurrentSettings.ContinentSeed + 3001, landmassPreset.ContinentWarpScale, 2, 2.0f, 0.5f);
        NoiseField mountainNoise = BuildNoise(CurrentSettings.MountainSeed, CurrentSettings.MountainNoiseScale, 5, 2.0f, 0.5f);

        float[] terrainValues = SampleGroupedTerrain(
            icosphere.Vertices,
            terrainNoise,
            continentNoise,
            continentWarpX,
            continentWarpY,
            continentWarpZ,
            landmassPreset);
        NormalizeInPlace(terrainValues);

        BiomeThresholds thresholds = CalculateBiomeThresholds(CurrentSettings);
        float radius = CurrentSettings.GetPlanetRadius();

        Vector3[] displacedVertices = new Vector3[icosphere.Vertices.Count];
        Color[] colors = new Color[icosphere.Vertices.Count];

        for (int i = 0; i < icosphere.Vertices.Count; i++)
        {
            Vector3 direction = icosphere.Vertices[i].Normalized();
            float terrainValue = terrainValues[i];
            float mountainValue = mountainNoise.GetNoise(direction);
            PlanetBiome biome = ResolveBiome(terrainValue, thresholds);
            float heightOffset = ResolveHeightOffset(biome, terrainValue, mountainValue);

            displacedVertices[i] = direction * (radius + heightOffset);
            colors[i] = ResolveColor(biome, terrainValue, mountainValue);
        }

        ArrayMesh mesh = BuildMesh(displacedVertices, icosphere.Indices, colors);
        _meshInstance.Mesh = mesh;
        _meshInstance.MaterialOverride = CreatePlanetMaterial();

        if (RebuildCollision)
        {
            RebuildStaticCollision(mesh);
        }
    }

    private MeshInstance3D ResolveMeshInstance()
    {
        if (HasNode(MeshInstancePath))
        {
            return GetNode<MeshInstance3D>(MeshInstancePath);
        }

        MeshInstance3D meshInstance = new()
        {
            Name = "PlanetMesh"
        };
        AddChild(meshInstance);

        if (Engine.IsEditorHint() && GetTree().EditedSceneRoot != null)
        {
            meshInstance.Owner = GetTree().EditedSceneRoot;
        }

        return meshInstance;
    }

    private NoiseField BuildNoise(int seed, float scale, int octaves, float lacunarity, float gain)
    {
        FastNoiseLite noise = new()
        {
            Seed = seed,
            NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth,
            Frequency = Mathf.Max(0.0001f, scale),
            FractalType = FastNoiseLite.FractalTypeEnum.Fbm,
            FractalOctaves = octaves,
            FractalLacunarity = lacunarity,
            FractalGain = gain
        };

        return new NoiseField(noise, CurrentSettings.GetPlanetRadius());
    }

    private float[] SampleGroupedTerrain(
        IReadOnlyList<Vector3> vertices,
        NoiseField terrainNoise,
        NoiseField continentNoise,
        NoiseField continentWarpX,
        NoiseField continentWarpY,
        NoiseField continentWarpZ,
        LandmassPreset landmassPreset)
    {
        float[] values = new float[vertices.Count];

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 direction = vertices[i].Normalized();
            Vector3 warpedDirection = WarpDirection(direction, continentWarpX, continentWarpY, continentWarpZ, landmassPreset.ContinentWarpStrength);

            float terrainDetail = terrainNoise.GetNoise(direction);
            float continentMask = continentNoise.GetNoise(warpedDirection);
            continentMask = Mathf.Pow(Mathf.Clamp(continentMask, 0.0f, 1.0f), landmassPreset.ContinentSharpness);

            float groupedTerrain = Mathf.Lerp(terrainDetail, continentMask, landmassPreset.ContinentInfluence);
            groupedTerrain += (terrainDetail - 0.5f) * landmassPreset.DetailRetention;
            groupedTerrain -= landmassPreset.SeaBias;

            values[i] = Mathf.Clamp(groupedTerrain, 0.0f, 1.0f);
        }

        return values;
    }

    private static Vector3 WarpDirection(
        Vector3 direction,
        NoiseField continentWarpX,
        NoiseField continentWarpY,
        NoiseField continentWarpZ,
        float warpStrength)
    {
        Vector3 warpOffset = new(
            continentWarpX.GetSignedNoise(direction),
            continentWarpY.GetSignedNoise(direction),
            continentWarpZ.GetSignedNoise(direction));

        return (direction + (warpOffset * warpStrength)).Normalized();
    }

    private static void NormalizeInPlace(float[] values)
    {
        if (values.Length == 0)
        {
            return;
        }

        float min = values[0];
        float max = values[0];

        for (int i = 1; i < values.Length; i++)
        {
            min = Mathf.Min(min, values[i]);
            max = Mathf.Max(max, values[i]);
        }

        float range = max - min;
        if (Mathf.IsZeroApprox(range))
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = 0.5f;
            }

            return;
        }

        for (int i = 0; i < values.Length; i++)
        {
            values[i] = (values[i] - min) / range;
        }
    }

    private static BiomeThresholds CalculateBiomeThresholds(PlanetSettings settings)
    {
        float total = Mathf.Max(
            0.001f,
            settings.WaterPercent +
            settings.WetlandsPercent +
            settings.PlainsPercent +
            settings.ForestPercent +
            settings.DesertPercent +
            settings.MountainPercent);

        float waterRatio = settings.WaterPercent / total;
        float wetlandsRatio = settings.WetlandsPercent / total;
        float plainsRatio = settings.PlainsPercent / total;
        float forestRatio = settings.ForestPercent / total;
        float desertRatio = settings.DesertPercent / total;

        return new BiomeThresholds(
            water: Mathf.Clamp(waterRatio, 0.0f, 1.0f),
            wetlands: Mathf.Clamp(waterRatio + wetlandsRatio, 0.0f, 1.0f),
            plains: Mathf.Clamp(waterRatio + wetlandsRatio + plainsRatio, 0.0f, 1.0f),
            forest: Mathf.Clamp(waterRatio + wetlandsRatio + plainsRatio + forestRatio, 0.0f, 1.0f),
            desert: Mathf.Clamp(waterRatio + wetlandsRatio + plainsRatio + forestRatio + desertRatio, 0.0f, 1.0f));
    }

    private static PlanetBiome ResolveBiome(float terrainValue, BiomeThresholds thresholds)
    {
        if (terrainValue <= thresholds.Water)
        {
            return PlanetBiome.Water;
        }

        if (terrainValue <= thresholds.Wetlands)
        {
            return PlanetBiome.Wetlands;
        }

        if (terrainValue <= thresholds.Plains)
        {
            return PlanetBiome.Plains;
        }

        if (terrainValue <= thresholds.Forest)
        {
            return PlanetBiome.Forest;
        }

        if (terrainValue <= thresholds.Desert)
        {
            return PlanetBiome.Desert;
        }

        return PlanetBiome.Mountains;
    }

    private float ResolveHeightOffset(PlanetBiome biome, float terrainValue, float mountainValue)
    {
        return biome switch
        {
            PlanetBiome.Water => terrainValue * CurrentSettings.WaterHeight,
            PlanetBiome.Wetlands => Mathf.Lerp(CurrentSettings.WaterHeight, CurrentSettings.WetlandsHeight, terrainValue),
            PlanetBiome.Plains => Mathf.Lerp(CurrentSettings.WetlandsHeight, CurrentSettings.PlainsHeight, terrainValue),
            PlanetBiome.Forest => Mathf.Lerp(CurrentSettings.PlainsHeight, CurrentSettings.ForestHeight, terrainValue),
            PlanetBiome.Desert => Mathf.Lerp(CurrentSettings.WetlandsHeight, CurrentSettings.DesertHeight, terrainValue),
            PlanetBiome.Mountains => Mathf.Lerp(CurrentSettings.ForestHeight, CurrentSettings.MountainHeight, mountainValue),
            _ => 0.0f
        };
    }

    private Color ResolveColor(PlanetBiome biome, float terrainValue, float mountainValue)
    {
        return biome switch
        {
            PlanetBiome.Water => CurrentSettings.WaterColor.Darkened((1.0f - terrainValue) * 0.2f),
            PlanetBiome.Wetlands => CurrentSettings.WetlandsColor.Darkened((1.0f - terrainValue) * 0.1f),
            PlanetBiome.Plains => CurrentSettings.PlainsColor.Lightened((terrainValue - 0.45f) * 0.12f),
            PlanetBiome.Forest => CurrentSettings.ForestColor.Darkened((1.0f - terrainValue) * 0.14f),
            PlanetBiome.Desert => CurrentSettings.DesertColor.Lightened(terrainValue * 0.14f),
            PlanetBiome.Mountains => CurrentSettings.MountainColor.Lightened(mountainValue * 0.2f),
            _ => Colors.White
        };
    }

    private ArrayMesh BuildMesh(IReadOnlyList<Vector3> vertices, IReadOnlyList<int> indices, IReadOnlyList<Color> colors)
    {
        SurfaceTool surfaceTool = new();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

        for (int i = 0; i < indices.Count; i++)
        {
            int vertexIndex = indices[i];
            Vector3 vertex = vertices[vertexIndex];
            Vector3 normal = vertex.Normalized();

            surfaceTool.SetColor(colors[vertexIndex]);
            surfaceTool.SetNormal(normal);
            surfaceTool.AddVertex(vertex);
        }

        surfaceTool.Index();
        surfaceTool.GenerateNormals();
        surfaceTool.GenerateTangents();

        return surfaceTool.Commit();
    }

    private static StandardMaterial3D CreatePlanetMaterial()
    {
        return new StandardMaterial3D
        {
            VertexColorUseAsAlbedo = true,
            Roughness = 1.0f,
            Metallic = 0.0f,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled
        };
    }

    private void RebuildStaticCollision(ArrayMesh mesh)
    {
        if (HasNode(CollisionBodyPath))
        {
            GetNode<StaticBody3D>(CollisionBodyPath).QueueFree();
        }

        StaticBody3D staticBody = new()
        {
            Name = "PlanetCollision"
        };

        CollisionShape3D collisionShape = new();
        ConcavePolygonShape3D shape = new()
        {
            Data = mesh.GetFaces()
        };

        collisionShape.Shape = shape;
        staticBody.AddChild(collisionShape);
        AddChild(staticBody);

        if (Engine.IsEditorHint() && GetTree().EditedSceneRoot != null)
        {
            staticBody.Owner = GetTree().EditedSceneRoot;
            collisionShape.Owner = GetTree().EditedSceneRoot;
        }
    }

    private static IcosphereData BuildIcosphere(int subdivisions)
    {
        List<Vector3> vertices = new();
        List<int> indices = new();

        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;
        AddBaseVertex(vertices, new Vector3(-1, t, 0));
        AddBaseVertex(vertices, new Vector3(1, t, 0));
        AddBaseVertex(vertices, new Vector3(-1, -t, 0));
        AddBaseVertex(vertices, new Vector3(1, -t, 0));
        AddBaseVertex(vertices, new Vector3(0, -1, t));
        AddBaseVertex(vertices, new Vector3(0, 1, t));
        AddBaseVertex(vertices, new Vector3(0, -1, -t));
        AddBaseVertex(vertices, new Vector3(0, 1, -t));
        AddBaseVertex(vertices, new Vector3(t, 0, -1));
        AddBaseVertex(vertices, new Vector3(t, 0, 1));
        AddBaseVertex(vertices, new Vector3(-t, 0, -1));
        AddBaseVertex(vertices, new Vector3(-t, 0, 1));

        int[] baseIndices =
        {
            0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11,
            1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
            3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9,
            4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1
        };

        indices.AddRange(baseIndices);

        for (int i = 0; i < subdivisions; i++)
        {
            Dictionary<EdgeKey, int> midpointCache = new();
            List<int> subdivided = new(indices.Count * 4);

            for (int j = 0; j < indices.Count; j += 3)
            {
                int v1 = indices[j];
                int v2 = indices[j + 1];
                int v3 = indices[j + 2];

                int a = GetMidpointIndex(v1, v2, vertices, midpointCache);
                int b = GetMidpointIndex(v2, v3, vertices, midpointCache);
                int c = GetMidpointIndex(v3, v1, vertices, midpointCache);

                subdivided.Add(v1);
                subdivided.Add(a);
                subdivided.Add(c);

                subdivided.Add(v2);
                subdivided.Add(b);
                subdivided.Add(a);

                subdivided.Add(v3);
                subdivided.Add(c);
                subdivided.Add(b);

                subdivided.Add(a);
                subdivided.Add(b);
                subdivided.Add(c);
            }

            indices = subdivided;
        }

        return new IcosphereData(vertices, indices);
    }

    private static void AddBaseVertex(List<Vector3> vertices, Vector3 vertex)
    {
        vertices.Add(vertex.Normalized());
    }

    private static int GetMidpointIndex(int first, int second, List<Vector3> vertices, Dictionary<EdgeKey, int> midpointCache)
    {
        EdgeKey key = new(first, second);
        if (midpointCache.TryGetValue(key, out int index))
        {
            return index;
        }

        Vector3 midpoint = ((vertices[first] + vertices[second]) * 0.5f).Normalized();
        index = vertices.Count;
        vertices.Add(midpoint);
        midpointCache[key] = index;
        return index;
    }

    private static LandmassPreset ResolveLandmassPreset(PlanetSettings settings)
    {
        return settings.LandmassMode switch
        {
            PlanetLandmassMode.LargeContinents => new LandmassPreset(
                continentNoiseScale: Mathf.Max(0.001f, settings.ContinentNoiseScale * 0.55f),
                continentWarpScale: Mathf.Max(0.001f, settings.ContinentWarpScale * 0.75f),
                continentInfluence: Mathf.Clamp(settings.ContinentInfluence + 0.18f, 0.0f, 1.0f),
                continentSharpness: Mathf.Max(0.1f, settings.ContinentSharpness + 0.35f),
                continentWarpStrength: Mathf.Clamp(settings.ContinentWarpStrength * 0.75f, 0.0f, 1.0f),
                detailRetention: 0.08f,
                seaBias: 0.04f),
            PlanetLandmassMode.Archipelago => new LandmassPreset(
                continentNoiseScale: Mathf.Max(0.001f, settings.ContinentNoiseScale * 1.85f),
                continentWarpScale: Mathf.Max(0.001f, settings.ContinentWarpScale * 1.5f),
                continentInfluence: Mathf.Clamp(settings.ContinentInfluence - 0.24f, 0.0f, 1.0f),
                continentSharpness: Mathf.Max(0.1f, settings.ContinentSharpness - 0.3f),
                continentWarpStrength: Mathf.Clamp(settings.ContinentWarpStrength + 0.18f, 0.0f, 1.0f),
                detailRetention: 0.2f,
                seaBias: 0.14f),
            _ => new LandmassPreset(
                continentNoiseScale: Mathf.Max(0.001f, settings.ContinentNoiseScale),
                continentWarpScale: Mathf.Max(0.001f, settings.ContinentWarpScale),
                continentInfluence: Mathf.Clamp(settings.ContinentInfluence, 0.0f, 1.0f),
                continentSharpness: Mathf.Max(0.1f, settings.ContinentSharpness),
                continentWarpStrength: Mathf.Clamp(settings.ContinentWarpStrength, 0.0f, 1.0f),
                detailRetention: 0.12f,
                seaBias: 0.08f)
        };
    }

    private readonly struct IcosphereData
    {
        public IcosphereData(List<Vector3> vertices, List<int> indices)
        {
            Vertices = vertices;
            Indices = indices;
        }

        public List<Vector3> Vertices { get; }

        public List<int> Indices { get; }
    }

    private readonly struct EdgeKey : IEquatable<EdgeKey>
    {
        public EdgeKey(int first, int second)
        {
            if (first < second)
            {
                First = first;
                Second = second;
            }
            else
            {
                First = second;
                Second = first;
            }
        }

        public int First { get; }

        public int Second { get; }

        public bool Equals(EdgeKey other)
        {
            return First == other.First && Second == other.Second;
        }

        public override bool Equals(object obj)
        {
            return obj is EdgeKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(First, Second);
        }
    }

    private readonly struct NoiseField
    {
        private readonly FastNoiseLite _noise;
        private readonly float _radius;

        public NoiseField(FastNoiseLite noise, float radius)
        {
            _noise = noise;
            _radius = radius;
        }

        public float GetNoise(Vector3 direction)
        {
            Vector3 sample = direction * _radius;
            return (_noise.GetNoise3Dv(sample) + 1.0f) * 0.5f;
        }

        public float GetSignedNoise(Vector3 direction)
        {
            Vector3 sample = direction * _radius;
            return _noise.GetNoise3Dv(sample);
        }
    }

    private readonly struct BiomeThresholds
    {
        public BiomeThresholds(float water, float wetlands, float plains, float forest, float desert)
        {
            Water = water;
            Wetlands = wetlands;
            Plains = plains;
            Forest = forest;
            Desert = desert;
        }

        public float Water { get; }

        public float Wetlands { get; }

        public float Plains { get; }

        public float Forest { get; }

        public float Desert { get; }
    }

    private readonly struct LandmassPreset
    {
        public LandmassPreset(
            float continentNoiseScale,
            float continentWarpScale,
            float continentInfluence,
            float continentSharpness,
            float continentWarpStrength,
            float detailRetention,
            float seaBias)
        {
            ContinentNoiseScale = continentNoiseScale;
            ContinentWarpScale = continentWarpScale;
            ContinentInfluence = continentInfluence;
            ContinentSharpness = continentSharpness;
            ContinentWarpStrength = continentWarpStrength;
            DetailRetention = detailRetention;
            SeaBias = seaBias;
        }

        public float ContinentNoiseScale { get; }

        public float ContinentWarpScale { get; }

        public float ContinentInfluence { get; }

        public float ContinentSharpness { get; }

        public float ContinentWarpStrength { get; }

        public float DetailRetention { get; }

        public float SeaBias { get; }
    }
}
