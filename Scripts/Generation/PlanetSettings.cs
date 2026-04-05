using Godot;

[GlobalClass]
public partial class PlanetSettings : Resource
{
    [Export]
    public PlanetSizeOption PlanetSize { get; set; } = PlanetSizeOption.Medium;

    [Export]
    public PlanetLandmassMode LandmassMode { get; set; } = PlanetLandmassMode.MediumContinents;

    [Export(PropertyHint.Range, "0,100,0.1")]
    public float WaterPercent { get; set; } = 55.0f;

    [Export(PropertyHint.Range, "0,100,0.1")]
    public float WetlandsPercent { get; set; } = 8.0f;

    [Export(PropertyHint.Range, "0,100,0.1")]
    public float PlainsPercent { get; set; } = 22.0f;

    [Export(PropertyHint.Range, "0,100,0.1")]
    public float ForestPercent { get; set; } = 10.0f;

    [Export(PropertyHint.Range, "0,100,0.1")]
    public float DesertPercent { get; set; } = 5.0f;

    [Export(PropertyHint.Range, "0,100,0.1")]
    public float MountainPercent { get; set; } = 15.0f;

    [Export(PropertyHint.Range, "0.001,10.0,0.001")]
    public float NoiseScale { get; set; } = 1.35f;

    [Export(PropertyHint.Range, "0.001,10.0,0.001")]
    public float MountainNoiseScale { get; set; } = 3.2f;

    [Export(PropertyHint.Range, "0.001,10.0,0.001")]
    public float ContinentNoiseScale { get; set; } = 0.38f;

    [Export(PropertyHint.Range, "0.001,10.0,0.001")]
    public float ContinentWarpScale { get; set; } = 0.85f;

    [Export(PropertyHint.Range, "0.0,1.0,0.01")]
    public float ContinentInfluence { get; set; } = 0.72f;

    [Export(PropertyHint.Range, "0.1,4.0,0.01")]
    public float ContinentSharpness { get; set; } = 1.55f;

    [Export(PropertyHint.Range, "0.0,1.0,0.01")]
    public float ContinentWarpStrength { get; set; } = 0.22f;

    [Export(PropertyHint.Range, "1,999999,1")]
    public int TerrainSeed { get; set; } = 1337;

    [Export(PropertyHint.Range, "1,999999,1")]
    public int ContinentSeed { get; set; } = 4242;

    [Export(PropertyHint.Range, "1,999999,1")]
    public int MountainSeed { get; set; } = 7331;

    [Export(PropertyHint.Range, "0.0,3.0,0.01")]
    public float WaterHeight { get; set; } = 0.05f;

    [Export(PropertyHint.Range, "0.0,3.0,0.01")]
    public float WetlandsHeight { get; set; } = 0.12f;

    [Export(PropertyHint.Range, "0.0,5.0,0.01")]
    public float PlainsHeight { get; set; } = 0.28f;

    [Export(PropertyHint.Range, "0.0,5.0,0.01")]
    public float ForestHeight { get; set; } = 0.36f;

    [Export(PropertyHint.Range, "0.0,5.0,0.01")]
    public float DesertHeight { get; set; } = 0.22f;

    [Export(PropertyHint.Range, "0.0,5.0,0.01")]
    public float MountainHeight { get; set; } = 1.2f;

    [Export]
    public Color WaterColor { get; set; } = new(0.10f, 0.34f, 0.70f, 1.0f);

    [Export]
    public Color WetlandsColor { get; set; } = new(0.18f, 0.47f, 0.24f, 1.0f);

    [Export]
    public Color PlainsColor { get; set; } = new(0.49f, 0.71f, 0.28f, 1.0f);

    [Export]
    public Color ForestColor { get; set; } = new(0.11f, 0.42f, 0.16f, 1.0f);

    [Export]
    public Color DesertColor { get; set; } = new(0.83f, 0.72f, 0.42f, 1.0f);

    [Export]
    public Color MountainColor { get; set; } = new(0.55f, 0.55f, 0.58f, 1.0f);

    public int GetSubdivisionCount()
    {
        return PlanetSize switch
        {
            PlanetSizeOption.Small => 3,
            PlanetSizeOption.Large => 5,
            _ => 4
        };
    }

    public float GetPlanetRadius()
    {
        return PlanetSize switch
        {
            PlanetSizeOption.Small => 3.5f,
            PlanetSizeOption.Large => 6.5f,
            _ => 5.0f
        };
    }

    public float GetPreviewCameraDistance()
    {
        return PlanetSize switch
        {
            PlanetSizeOption.Small => 11.0f,
            PlanetSizeOption.Large => 18.0f,
            _ => 14.0f
        };
    }
}
