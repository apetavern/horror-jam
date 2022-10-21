using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GvarJam.HammerEntities;

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
		if(player is not null )
		{
			lookpos = Vector3.Lerp( lookpos, GetBoneTransform( "TopCylinder" ).Position + (player.EyePosition - GetBoneTransform("TopCylinder").Position).ClampLength(20f) + Vector3.Up *5f, 0.5f );
			DebugOverlay.Axis( lookpos, lookrot );
			lookrot = Rotation.Slerp( lookrot, Rotation.LookAt( player.EyePosition - GetBoneTransform( "TopCylinder" ).Position, Vector3.Up ), 10f );
			SetAnimParameter( "position", lookpos );//Head pos
			SetAnimParameter( "rotation", lookrot );
			flatlookrot = Rotation.Slerp( flatlookrot, Rotation.LookAt( (player.Position - Position).WithZ( 0 ), Vector3.Up ), 10f );
			Rotation = flatlookrot;
		}
	}
}

