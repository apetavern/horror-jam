using SandboxEditor;


namespace GvarJam.HammerEntities;

[Category( "Environment" )]
[Library( "ent_mountedcamera" )]
[HammerEntity]
[EditorModel( "models/mountedcamera/mountedcamera.vmdl" )]
public partial class MountedCamera : AnimatedEntity
{
	public Pawn player;
	public override void Spawn()
	{
		base.Spawn();
		SetModel( "models/mountedcamera/mountedcamera.vmdl" );
		player = All.OfType<Pawn>().First();
	}
	Vector3 lookpos;
	Rotation lookrot;
	Rotation flatlookrot;
	[Event.Tick]
	public void Tick()
	{
		if ( player is not null )
		{

			float playerdist = Vector3.DistanceBetween( Position - Vector3.Up * 15f, player.EyePosition )/200f;
			lookpos = Vector3.Lerp( lookpos, Position - Vector3.Up * 15f + (player.EyePosition - (Position - Vector3.Up * 15f)).Normal * 25f * playerdist, 0.5f );

			lookrot = Rotation.Slerp( lookrot, Rotation.LookAt( player.EyePosition - Vector3.Up*10f - (Position - Vector3.Up * 15f), Vector3.Up ) * new Angles(0,90,90).ToRotation(), 10f );

			SetAnimParameter( "position", lookpos );
			SetAnimParameter( "rotation", lookrot );
			flatlookrot = Rotation.Slerp( flatlookrot, Rotation.LookAt( (player.Position - Position).WithZ( 0 ), Vector3.Up ), 10f );
			Rotation = flatlookrot;
		}
	}
}

