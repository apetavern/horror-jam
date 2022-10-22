namespace GvarJam.HammerEntities;

[Category( "Environment" )]
[Library( "ent_cameracontroller" )]
[HammerEntity]
[EditorModel( "models/cameraconsole/console.vmdl" )]
public partial class CameraController : InteractableEntity
{
	private TimeSince TimeSinceUsed { get; set; }

	[Net]
	public MountedCamera? TargetCamera { get; set; }

	private ScenePortal? ScenePortal;

	public override void Spawn()
	{
		base.Spawn();

		Name = "Camera Console";

		SetModel( "models/cameraconsole/console.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}

	public override void ClientSpawn()
	{
		//base.ClientSpawn();
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

		User = user;
		TimeSinceUsed = 0;
		
		if( Host.IsServer )
		{
			var usableCameras = FindUsableMountedCameras();
			var targetCamera = usableCameras.FirstOrDefault();

			if ( targetCamera is null )
				return;

			TargetCamera = targetCamera;

			return;
		}

		SetBodyGroup( 0, 1 );
		ScenePortal = new ScenePortal( Map.Scene, Model.Load( "models/cameraconsole/console_screen.vmdl" ), Transform );
	}

	public void StopUsing()
	{
		User = null;

		if ( Host.IsServer )
			return;

		ScenePortal?.Delete();
		ScenePortal = null;

		SetBodyGroup( 0, 0 );
	}

	[Event.Tick]
	public void Tick()
	{
		if ( TimeSinceUsed > 10 )
			StopUsing();
	}

	protected List<MountedCamera> FindUsableMountedCameras()
	{
		return All.OfType<MountedCamera>().Where( x => x.IsUsable ).ToList();
	}
}

