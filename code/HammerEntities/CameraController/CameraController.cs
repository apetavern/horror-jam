namespace GvarJam.HammerEntities;

/// <summary>
/// A controller to view camera feeds.
/// </summary>
[Category( "Environment" )]
[Library( "ent_cameracontroller" )]
[HammerEntity]
[EditorModel( "models/cameraconsole/console.vmdl" )]
public sealed partial class CameraController : LockedUseItem
{
	/// <summary>
	/// The time since this controller was interacted with.
	/// </summary>
	[Net, Predicted]
	public TimeSince TimeSinceUsed { get; set; }

	/// <summary>
	/// The currently viewed camera.
	/// </summary>
	[Net]
	public MountedCamera? TargetCamera { get; set; }

	/// <summary>
	/// The number of usable cameras.
	/// </summary>
	[Net]
	public int NumberOfUsableCameras { get; set; }

	/// <summary>
	/// The index of the currently viewed camera.
	/// </summary>
	[Net]
	public int CurrentCameraIndex { get; set; }

	/// <summary>
	/// The screen model entity.
	/// </summary>
	[Net]
	private ModelEntity Screen { get; set; } = null!;

	/// <summary>
	/// The scene portal to the view of the currently viewed camera.
	/// </summary>
	private ScenePortal? scenePortal;

	/// <summary>
	/// The world UI panel for the zone name.
	/// </summary>
	private CameraControllerPanel? zoneName { get; set; }

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/cameraconsole/console.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static );

		Screen = new ModelEntity( "models/cameraconsole/console_screen.vmdl", this );
		Screen.SetMaterialGroup( 1 );

		Name = "Camera Controller";

		// Set initial camera
		var usableCameras = FindUsableMountedCameras();
		var targetCamera = usableCameras.FirstOrDefault();

		if ( targetCamera is null )
			return;

		TargetCamera = targetCamera;
		CurrentCameraIndex = 0;
	}

	/// <inheritdoc/>
	public override void Simulate( Client cl )
	{

		if ( User is Pawn player && GetAttachment( "rhand_attach" ).HasValue )
		{
			player.SetAnimParameter( "right_hand_ik", GetAttachment( "rhand_attach" )!.Value );
		}

		if ( User is Pawn player2 && GetAttachment( "lhand_attach" ).HasValue )
		{
			player2.SetAnimParameter( "left_hand_ik", GetAttachment( "lhand_attach" )!.Value );
		}

		if ( TimeSinceUsed < 1 )
			return;

		if ( Input.Released( InputButton.Use ) )
		{
			StopUsing();

			return;
		}

		if ( Input.Released( InputButton.Right ) )
		{
			SetAnimParameter( "right", true );

			if ( CurrentCameraIndex + 1 > NumberOfUsableCameras )
				CurrentCameraIndex = 0;
			else
				CurrentCameraIndex += 1;

			SwitchCameraView();
			return;
		}

		if ( Input.Released( InputButton.Left ) )
		{
			SetAnimParameter( "left", true );

			if ( CurrentCameraIndex - 1 <= 0 )
				CurrentCameraIndex = NumberOfUsableCameras;
			else
				CurrentCameraIndex -= 1;

			SwitchCameraView();
			return;
		}
	}

	/// <inheritdoc/>
	protected override void OnDestroy()
	{
		base.OnDestroy();

		StopUsing();
	}

	/// <inheritdoc/>
	protected override void OnUsed( Entity user )
	{
		base.OnUsed( user );

		if ( user is not Pawn player )
			return;

		player.InteractedEntity = this;

		if ( GetAttachment( "rhand_attach" ).HasValue )
		{
			player.SetAnimParameter( "b_IKright", true );
		}

		if ( GetAttachment( "lhand_attach" ).HasValue )
		{
			player.SetAnimParameter( "b_IKleft", true );
		}

		player.SetAnimLookAt( "aim_head", Position + Vector3.Up * 68f );
		player.SetAnimLookAt( "aim_eyes", Position + Vector3.Up * 68f );

		SnapPlayerToUsePosition( player, 45 );

		TimeSinceUsed = 0;

		if ( IsServer )
		{
			// Set the user
			User = player;

			return;
		}

		// Hide the screen when it's being used.
		if ( Screen is not null )
			Screen.RenderColor = Color.Transparent;

		if ( IsClient )
		{
			var attachment = GetAttachment( "zonename" );

			if ( attachment is null )
				return;

			zoneName = new CameraControllerPanel( this );
			zoneName.Position = attachment.Value.Position;
			zoneName.Rotation = attachment.Value.Rotation;
		}

		scenePortal = new ScenePortal( Map.Scene, Model.Load( "models/cameraconsole/console_screen.vmdl" ), Transform );
	}

	/// <inheritdoc/>
	public override void StopUsing()
	{
		var player = User as Pawn;

		if ( player is not null )
		{
			player.InteractedEntity = null;

			player.SetAnimParameter( "b_IKleft", false );
			player.SetAnimParameter( "b_IKright", false );

			ReleasePlayer( player );
		}

		if ( IsClient )
		{
			scenePortal?.Delete();
			scenePortal = null;

			if( Screen is not null ) 
				Screen.RenderColor = Color.White;

			if ( zoneName is not null )
				zoneName.Delete();
		}

		User = null;
	}

	/// <inheritdoc/>
	public override bool IsUsable( Entity user )
	{
		return base.IsUsable( user ) && User is null;
	}

	/// <inheritdoc/>
	public override bool OnUse( Entity user )
	{
		base.OnUse( user );

		return true;
	}

	/// <summary>
	/// Switches the camera that is being viewed.
	/// </summary>
	private void SwitchCameraView()
	{
		var cameras = FindUsableMountedCameras();
		TargetCamera = cameras[CurrentCameraIndex];

		if ( IsClient && zoneName is not null )
			zoneName.UpdateName( TargetCamera.ZoneName.ToUpper() );

		Sound.FromEntity( "joystick_click", this );
	}

	/// <summary>
	/// Finds all viewable cameras.
	/// </summary>
	/// <returns>All viewable cameras.</returns>
	private List<MountedCamera> FindUsableMountedCameras()
	{
		var cameras = All.OfType<MountedCamera>().Where( x => x.IsViewable ).ToList();
		NumberOfUsableCameras = cameras.Count - 1;

		return cameras;
	}

	/// <summary>
	/// Updates the scene portal in the camera controllers screen.
	/// </summary>
	[Event.Frame]
	private void OnFrame()
	{
		if ( !IsClient )
			return;

		if ( scenePortal == null || !scenePortal.IsValid )
			return;

		scenePortal.Transform = Transform;

		if ( !TargetCamera.IsValid() )
			return;

		var attachment = TargetCamera.GetAttachment( "lens_position" );

		if ( attachment is null )
			return;

		scenePortal.ViewPosition = attachment.Value.Position;
		scenePortal.ViewRotation = attachment.Value.Rotation;
		scenePortal.FieldOfView = TargetCamera.Fov;
		scenePortal.ZNear = TargetCamera.ZNear;
		scenePortal.ZFar = TargetCamera.ZFar;
		scenePortal.Aspect = TargetCamera.Aspect;
	}
}

