namespace GvarJam.HammerEntities;

[Category( "Environment" )]
[Library( "ent_cameracontroller" )]
[HammerEntity]
[EditorModel( "models/cameraconsole/console.vmdl" )]
public partial class CameraController : LockedUseItem
{
	[Net, Predicted]
	public TimeSince TimeSinceUsed { get; set; }

	[Net]
	public MountedCamera? TargetCamera { get; set; }

	[Net]
	public int NumberOfUsableCameras { get; set; }

	[Net]
	public int CurrentCameraIndex { get; set; }

	private ScenePortal? ScenePortal;

	public override void Spawn()
	{
		base.Spawn();

		Name = "Camera Console";

		SetModel( "models/cameraconsole/console.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		StopUsing();
	}

	[Event.Frame]
	private void OnFrame()
	{
		if ( !IsClient )
			return;

		if ( ScenePortal == null || !ScenePortal.IsValid )
			return;

		ScenePortal.Transform = Transform;

		if ( !TargetCamera.IsValid() )
			return;

		var attachment = TargetCamera.GetAttachment( "lens_position" );

		if ( attachment is null )
			return;

		ScenePortal.ViewPosition = attachment.Value.Position;
		ScenePortal.ViewRotation = attachment.Value.Rotation;
		ScenePortal.FieldOfView = TargetCamera.Fov;
		ScenePortal.ZNear = TargetCamera.ZNear;
		ScenePortal.ZFar = TargetCamera.ZFar;
		ScenePortal.Aspect = TargetCamera.Aspect;
	}

	public override bool IsUsable( Entity user )
	{
		return base.IsUsable( user ) && User is null;
	}

	protected override void OnUsed( Entity user )
	{
		base.OnUsed( user );

		if ( user is not Pawn player )
			return;

		player.InteractedEntity = this;

		TimeSinceUsed = 0;

		if ( Host.IsServer )
		{
			// Set the user
			User = player;

			SnapPlayerToUsePosition( player );

			var usableCameras = FindUsableMountedCameras();
			var targetCamera = usableCameras.FirstOrDefault();

			if ( targetCamera is null )
				return;

			TargetCamera = targetCamera;
			CurrentCameraIndex = 0;

			return;
		}

		SetBodyGroup( 0, 1 );
		ScenePortal = new ScenePortal( Map.Scene, Model.Load( "models/cameraconsole/console_screen.vmdl" ), Transform );
	}

	public override void StopUsing()
	{
		var player = User as Pawn;

		if ( player is not null )
		{
			player.InteractedEntity = null;

			ReleasePlayer( player );		
		}

		if ( Host.IsClient )
		{
			ScenePortal?.Delete();
			ScenePortal = null;

			SetBodyGroup( 0, 0 );
		}

		User = null;
	}

	public override void Simulate()
	{
		if ( TimeSinceUsed < 1 )
			return;

		if ( Input.Released( InputButton.Use ) )
		{
			StopUsing();
			return;
		}

		if ( Input.Released( InputButton.Right ) )
		{
			var cameras = FindUsableMountedCameras();

			if ( CurrentCameraIndex + 1 > NumberOfUsableCameras )
				CurrentCameraIndex = 0;
			else
				CurrentCameraIndex += 1;

			TargetCamera = cameras[CurrentCameraIndex];

			return;
		}

		if ( Input.Released( InputButton.Left ) )
		{
			var cameras = FindUsableMountedCameras();

			if ( CurrentCameraIndex - 1 <= 0 )
				CurrentCameraIndex = NumberOfUsableCameras;
			else
				CurrentCameraIndex -= 1;

			TargetCamera = cameras[CurrentCameraIndex];

			return;
		}
	}

	protected List<MountedCamera> FindUsableMountedCameras()
	{
		var cameras = All.OfType<MountedCamera>().Where( x => x.IsViewable ).ToList();
		NumberOfUsableCameras = cameras.Count - 1;

		return cameras;
	}
}

