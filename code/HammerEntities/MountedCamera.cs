namespace GvarJam.HammerEntities;

/// <summary>
/// A mounted camera that views a room.
/// </summary>
[Category( "Environment" )]
[Library( "ent_mountedcamera" )]
[HammerEntity]
[EditorModel( "models/mountedcamera/mountedcamera.vmdl" )]
public sealed partial class MountedCamera : AnimatedEntity
{
	/// <summary>
	/// Used to turn on lights in rooms you're looking at.
	/// </summary>
	[Net, Predicted]
	public bool IsBeingViewed { get; set; } = false;

	/// <summary>
	/// Whether or not this camera is viewable on the camera console.
	/// </summary>
	[Net, Property]
	public bool IsViewable { get; set; } = true;

	/// <summary>
	/// The name of the zone it is monitoring.
	/// </summary>
	[Net, Property]
	public string ZoneName { get; set; } = "UNKNOWN";

	/// <summary>
	/// Field of view in degrees
	/// </summary>
	[Property]
	public float Fov { get; set; } = 120.0f;

	/// <summary>
	/// Distance to the near plane
	/// </summary>
	[Property]
	public float ZNear { get; set; } = 4.0f;

	/// <summary>
	/// Distance to the far plane
	/// </summary>
	[Property]
	public float ZFar { get; set; } = 10000.0f;

	/// <summary>
	/// Aspect ratio
	/// </summary>
	[Property]
	public float Aspect { get; set; } = 1.0f;

	/// <summary>
	/// The maximum distance the camera can track a pawn at.
	/// </summary>
	private const float CameraTrackingReachUnits = 300f;

	/// <summary>
	/// The current look position of the cameras pawn tracking.
	/// </summary>
	private Vector3 LookPos;

	/// <summary>
	/// The current look rotation of the cameras pawn tracking.
	/// </summary>
	private Rotation LookRot;

	/// <summary>
	/// Look rotation but with flat Z so the camear doesn't tilt.
	/// Used for lerping the rotation a bit so it's not too jittery too.
	/// </summary>
	private Rotation FlatLookRot;

	/// <summary>
	/// Starting rotation to return the base rotation when the player is no longer being tracked.
	/// </summary>
	private Rotation StartRot;

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/mountedcamera/mountedcamera.vmdl" );
		StartRot = Rotation;
		Transmit = TransmitType.Always;
	}

	/// <summary>
	/// The server tick of the camera to stare at a pawn if it is close enough.
	/// </summary>
	[Event.Tick.Server]
	private void Tick()
	{
		var player = All.OfType<Pawn>().GetClosestOrDefault( this );
		if ( player is null )
			return;

		var shouldTrack = Vector3.DistanceBetween( player.Position, Position ) < CameraTrackingReachUnits;

		SetAnimParameter( "tracking", shouldTrack );

		if ( !shouldTrack )
		{
			FlatLookRot = Rotation.Slerp( FlatLookRot, StartRot, 10f );
			Rotation = FlatLookRot;
			return;
		}


		float playerdist = Vector3.DistanceBetween( Position - Vector3.Up * 15f, player.EyePosition ) / 200f;
		LookPos = Vector3.Lerp( LookPos, Position - Vector3.Up * 15f + (player.EyePosition - (Position - Vector3.Up * 15f)).Normal * 25f * playerdist, 0.5f );
		LookRot = Rotation.Slerp( LookRot, Rotation.LookAt( player.EyePosition - Vector3.Up * 10f - (Position - Vector3.Up * 15f), Vector3.Up ) * new Angles( 0, 90, 90 ).ToRotation(), 10f );

		SetAnimParameter( "position", LookPos );
		SetAnimParameter( "rotation", LookRot );

		FlatLookRot = Rotation.Slerp( FlatLookRot, Rotation.LookAt( (player.Position - Position).WithZ( 0 ), Vector3.Up ), 10f );
		Rotation = FlatLookRot;
	}
}

