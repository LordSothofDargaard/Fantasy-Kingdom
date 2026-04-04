using Godot;

[Tool]
public partial class Node3d : Node3D
{
    [Export]
    public NodePath PlanetGeneratorPath { get; set; } = "PlanetGenerator";

    public override void _Ready()
    {
        if (HasNode(PlanetGeneratorPath))
        {
            PlanetGenerator planetGenerator = GetNode<PlanetGenerator>(PlanetGeneratorPath);
            planetGenerator.GeneratePlanet();
        }
    }
}
