namespace GvarJam.Cameras;

public partial class ControllableCamera : CameraMode
{
	[Net]
	public MountedCamera? ControlFromCamera { get; set; }

	public CameraMode SetupFromMountedCamera( MountedCamera camera )
	{
		ControlFromCamera = camera;

		Position = camera.Position;
		Rotation = camera.Rotation;

		return this;
	}

	public override void Update()
	{
		if ( ControlFromCamera is null )
			return;

		var attachment = ControlFromCamera.GetAttachment( "lens_position" );

		if ( attachment is null )
			return;

		Position = attachment.Value.Position;
		Rotation = attachment.Value.Rotation;

	}
}
