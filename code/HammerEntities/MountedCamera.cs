using GvarJam.Utility;
using SandboxEditor;

namespace GvarJam.HammerEntities;

[Category( "Environment" )]
[Library( "ent_mountedcamera" )]
[HammerEntity]
[EditorModel( "models/mountedcamera/mountedcamera.vmdl" )]
public partial class MountedCamera : AnimatedEntity
{
	private Vector3 lookPos;
	private Rotation lookRot;
	private Rotation flatLookRot;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/mountedcamera/mountedcamera.vmdl" );
	}

	[Event.Tick.Server]
	public void Tick()
	{
		var player = All.OfType<Pawn>().GetClosestOrDefault( this );
		if ( player is null )
			return;

		float playerdist = Vector3.DistanceBetween( Position - Vector3.Up * 15f, player.EyePosition )/200f;
		lookPos = Vector3.Lerp( lookPos, Position - Vector3.Up * 15f + (player.EyePosition - (Position - Vector3.Up * 15f)).Normal * 25f * playerdist, 0.5f );
		lookRot = Rotation.Slerp( lookRot, Rotation.LookAt( player.EyePosition - Vector3.Up*10f - (Position - Vector3.Up * 15f), Vector3.Up ) * new Angles(0,90,90).ToRotation(), 10f );

		SetAnimParameter( "position", lookPos );
		SetAnimParameter( "rotation", lookRot );
		flatLookRot = Rotation.Slerp( flatLookRot, Rotation.LookAt( (player.Position - Position).WithZ( 0 ), Vector3.Up ), 10f );
		Rotation = flatLookRot;
	}
}

