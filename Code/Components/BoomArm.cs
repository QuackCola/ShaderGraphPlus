using Sandbox;
using Sandbox.Diagnostics;

[Title("Boom Arm")]
[Category("Camera")]
[Icon("start")]
public sealed class BoomArm : Component
{

    [Property]
    public CameraComponent _camera;


    [Property]
    public float _armLength;

    [Property]
    public Vector3 _socketOffset;

    //private Vector3 _finalOffset;

    protected override void OnStart()
    {
        _camera = Components.GetInDescendantsOrSelf<CameraComponent>();

        Assert.NotNull(_camera, $"{nameof(BoomArm).ToTitleCase()} need to have a camera object as child.");


    }

    protected override void OnUpdate()
    {
        if (_camera is null)
            return;

        UpdateCameraPosition();
    }

    public void UpdateCameraPosition()
    {
        // Get the current camera position with the armLength applied to the x coordinate.
        var CameraLocalPosition = new Vector3(_camera.LocalPosition.x - _armLength, _camera.LocalPosition.y, _camera.LocalPosition.z);

        CameraLocalPosition.x -= _socketOffset.x;
        CameraLocalPosition.y -= _socketOffset.y;
        CameraLocalPosition.z += _socketOffset.z; // I want the camera to offest up on the Z axis so i add instead of subtracting.        

        _camera.LocalPosition = CameraLocalPosition;
    }

    protected override void DrawGizmos()
    {
        // if (VisualDebugging is false) return;
        if (_camera is null) return;

    }
}
