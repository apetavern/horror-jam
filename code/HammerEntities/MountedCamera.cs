namespace GvarJam.HammerEntities;

[Category( "Environment" )]
[Library( "ent_mountedcamera" )]
[HammerEntity]
[EditorModel( "models/mountedcamera/mountedcamera.vmdl" )]
public partial class MountedCamera : AnimatedEntity
{
	private Pawn? TargetedPlayer;

	private Vector3 lookPos;
	private Rotation lookRot;
	private Rotation flatLookRot;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/mountedcamera/mountedcamera.vmdl" );

		TargetedPlayer = All.OfType<Pawn>().First();
	}

	[Event.Tick]
	public void Tick()
	{
		if ( TargetedPlayer is null )
			return;

		float playerdist = Vector3.DistanceBetween( Position - Vector3.Up * 15f, TargetedPlayer.EyePosition )/200f;

		lookPos = Vector3.Lerp( lookPos, Position - Vector3.Up * 15f + (TargetedPlayer.EyePosition - (Position - Vector3.Up * 15f)).Normal * 25f * playerdist, 0.5f );
		lookRot = Rotation.Slerp( lookRot, Rotation.LookAt( TargetedPlayer.EyePosition - Vector3.Up*10f - (Position - Vector3.Up * 15f), Vector3.Up ) * new Angles(0,90,90).ToRotation(), 10f );
		flatLookRot = Rotation.Slerp( flatLookRot, Rotation.LookAt( (TargetedPlayer.Position - Position).WithZ( 0 ), Vector3.Up ), 10f );

		SetAnimParameter( "position", lookPos );
		SetAnimParameter( "rotation", lookRot );

		Rotation = flatLookRot;
	}
}

