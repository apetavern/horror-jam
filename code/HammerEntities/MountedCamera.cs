using GvarJam.Utility;
using SandboxEditor;

namespace GvarJam.HammerEntities;

[Category( "Environment" )]
[Library( "ent_mountedcamera" )]
[HammerEntity]
[EditorModel( "models/mountedcamera/mountedcamera.vmdl" )]
public partial class MountedCamera : AnimatedEntity
{
	/// <summary>
	/// Whether or not the camera should follow players that are moving around.
	/// </summary>
	private bool IsControlledManually { get; set; }

	/// <summary>
	/// The CameraMode that the player was using prior to operating this camera.
	/// </summary>
	private CameraMode PlayersLastCamera { get; set; }

	private Vector3 LookPos;
	private Rotation LookRot;
	private Rotation FlatLookRot;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/mountedcamera/mountedcamera.vmdl" );
	}

	public void StartManualControl( Pawn player ) 
	{
		player.Camera = new ControllableCamera( this );
	}

	public void EndManualControl( Pawn player ) 
	{

	}

	[Event.Tick.Server]
	public void Tick()
	{
		if ( IsControlledManually )
			return;

		var player = All.OfType<Pawn>().GetClosestOrDefault( this );

		if ( player is null )
			return;

		float playerdist = Vector3.DistanceBetween( Position - Vector3.Up * 15f, player.EyePosition )/200f;
		LookPos = Vector3.Lerp( LookPos, Position - Vector3.Up * 15f + (player.EyePosition - (Position - Vector3.Up * 15f)).Normal * 25f * playerdist, 0.5f );
		LookRot = Rotation.Slerp( LookRot, Rotation.LookAt( player.EyePosition - Vector3.Up*10f - (Position - Vector3.Up * 15f), Vector3.Up ) * new Angles(0,90,90).ToRotation(), 10f );

		SetAnimParameter( "position", LookPos );
		SetAnimParameter( "rotation", LookRot );

		FlatLookRot = Rotation.Slerp( FlatLookRot, Rotation.LookAt( (player.Position - Position).WithZ( 0 ), Vector3.Up ), 10f );
		Rotation = FlatLookRot;
	}
}

