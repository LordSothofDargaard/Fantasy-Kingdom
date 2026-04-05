using Godot;

public partial class PlanetPreviewController : Node3D
{
    [Export]
    public NodePath PlanetPivotPath { get; set; } = "PlanetPivot";

    [Export]
    public NodePath CameraPath { get; set; } = "Camera3D";

    [Export]
    public float RotationSpeedDegrees { get; set; } = 8.0f;

    private Node3D _planetPivot = null;
    private Camera3D _camera = null;

    public override void _Ready()
    {
        _planetPivot = GetNode<Node3D>(PlanetPivotPath);
        _camera = GetNode<Camera3D>(CameraPath);
    }

    public override void _Process(double delta)
    {
        if (_planetPivot == null)
        {
            return;
        }

        _planetPivot.RotateY(Mathf.DegToRad(RotationSpeedDegrees) * (float)delta);
    }

    public void FramePlanet(PlanetSettings settings)
    {
        if (_camera == null || settings == null)
        {
            return;
        }

        Vector3 position = _camera.Position;
        position.Z = settings.GetPreviewCameraDistance();
        _camera.Position = position;
    }
}
