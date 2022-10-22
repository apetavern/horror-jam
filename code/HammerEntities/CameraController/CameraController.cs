namespace GvarJam.HammerEntities;

[Category( "Environment" )]
[Library( "ent_cameracontroller" )]
[HammerEntity]
[EditorModel( "models/editor/camera.vmdl" )]
public partial class CameraController : InteractableEntity
{
	private TimeSince TimeSinceUsed { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/editor/camera.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}

	public override bool IsUsable( Entity user )
	{
		return base.IsUsable( user ) && User is null;
	}

	protected override void OnUsed( Entity user )
	{
		base.OnUsed( user );

		if ( Host.IsClient )
			return;

		if ( user is not Pawn player )
			return;

		User = user;
		TimeSinceUsed = 0;

		//var usableCameras = FindUsableMountedCameras();
		//var targetCamera = usableCameras.FirstOrDefault();

		//if ( targetCamera is null )
			//return;

		//var camera = new ControllableCamera().SetupFromMountedCamera( targetCamera );

		//player.Camera = camera;
		//player.Controller = null;
	}

	protected List<MountedCamera> FindUsableMountedCameras()
	{
		return All.OfType<MountedCamera>().Where( x => x.IsUsable ).ToList();
	}

	[Event.Tick.Server]
	public void Tick()
	{
		// Remove this later
		if ( TimeSinceUsed > 10 && User is Pawn player )
		{
			player.Camera = new PawnCamera();
			player.Controller = new MovementController();

			User = null;
		}
			
	}
}

