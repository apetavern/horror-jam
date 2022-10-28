using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sandbox.Easing;

namespace GvarJam.Monster;

[Category( "Enemy" )]
[Library( "ent_splitizen" )]
[HammerEntity]
[EditorModel( "models/enemy/basic_splitizen.vmdl" )]
public partial class Splitizen : AnimatedEntity
{
	[ConVar.Replicated( "debug_monster" )]
	public static bool DebugEnabled { get; set; }

	public BBox Bounds => new(
	new Vector3( -12, -12, 0 ),
	new Vector3( 12, 12, 72 )
	);

	private PathFinding PathFinding { get; set; }
	private float Friction => 2.0f;
	private float Gravity => 300f;
	private bool IsGrounded { get; set; }

	[Net]MonsterEntity Monster { get; set; }

	[Net]AnimatedEntity SplitTop { get; set; }

	public bool Ditched;

	public override void Spawn()
	{
		base.Spawn();
		SetModel( "models/player/playermodel.vmdl" );

		SetMaterialOverride( "models/enemy/materials/citizen/splitizen_skin.vmat" );
		SetBodyGroup( 0, 1 );
		SetBodyGroup( 1, 1 );
		SetBodyGroup( 3, 1 );

		new ModelEntity( "models/citizen_clothes/jacket/longsleeve/models/jeans.vmdl" ).SetParent( this, true );

		SplitTop = new( "models/enemy/basic_splitizen.vmdl" );

		SplitTop.SetParent( this, true );

		Monster = new MonsterEntity();

		Monster.SetParent( this, true );

		PathFinding = new( this );
	}

	public void DoSplit()
	{
		Monster.SetAnimParameter( "split", true );
		SplitTop.SetAnimParameter( "split", true );
		Split = true;
	}

	public void DitchBody()
	{
		Ditched = true;
		SplitTop.SetAnimParameter( "split", true );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
		PhysicsBody.ApplyForce( -Rotation.Forward * 10000f );
	}

	public bool Split = false;

	TimeSince TimeSinceAlive;

	[Event.Tick]
	public void OnTick()
	{
		if ( Ditched )
		{
			return;
		}

		EyeLocalPosition = Vector3.Up * 64f;

		if ( IsServer )
		{
			if ( DebugEnabled )
			{
				DebugOverlay.Box( Position, Bounds.Mins, Bounds.Maxs, Color.White );
				DrawDebugInfo();
			}

			TickMove();



			//TickStuck();
		}


		CitizenAnimationHelper helper = new CitizenAnimationHelper( this );

		helper.WithVelocity( Velocity );
		helper.IsGrounded = IsGrounded;
		//TickAnimator();
	}

	public void SetPath( Vector3 target )
	{
		PathFinding.SetPath( target );
	}

	private void DrawDebugInfo()
	{
		var player = Entity.All.OfType<Pawn>().First();
		var pos = Position + Vector3.Up * 64 + player.EyeRotation.Right * 12f;

		var info =
			$"grounded?: {IsGrounded}\n" +
			$"speed: {Velocity.Length}\n";

		DebugOverlay.Text( info, pos, 0, Color.White, 0 );
	}

	private void TickMove()
	{
		// Movehelper setup
		var moveHelper = new MoveHelper( Position, Velocity );
		moveHelper.Trace = moveHelper.Trace.Ignore( this ).Size( Bounds );

		// Grounded check
		IsGrounded = moveHelper.TraceDirection( Vector3.Down ).Hit;

		if ( IsGrounded )
		{
			// Move along path
			moveHelper.Velocity = PathFinding.GetWishVelocity( moveHelper.Velocity, Friction );

			if ( moveHelper.Velocity.Normal.Length > 0.1f )
			{
				Rotation = Rotation.Lerp( Rotation, Rotation.LookAt( moveHelper.Velocity.WithZ( 0 ).Normal ), 5f * Time.Delta );
				Rotation = Rotation.Angles().WithPitch( 0 ).WithRoll( 0 ).ToRotation();
			}

			moveHelper.ApplyFriction( Friction, Time.Delta );
		}
		else
		{
			// Gravity
			moveHelper.Velocity -= Vector3.Up * Gravity * Time.Delta;
		}

		// Stick to ground ( good for slopes )
		StayOnGround( moveHelper );

		if ( DebugEnabled )
		{
			var startPos = Position + Vector3.Up * 32;
			DebugOverlay.Arrow( startPos, startPos + moveHelper.Velocity, Color.Green );
		}

		moveHelper.TryUnstuck();
		moveHelper.TryMoveWithStep( Time.Delta, 16f );
		Position = moveHelper.Position;
		Velocity = moveHelper.Velocity;
	}

	/// <summary>
	/// Try to keep a walking player on the ground when running down slopes etc
	/// </summary>
	public virtual void StayOnGround( MoveHelper moveHelper )
	{
		var start = Position + Vector3.Up * 2;
		var end = Position + Vector3.Down * 16f;

		// See how far up we can go without getting stuck
		var trace = moveHelper.TraceFromTo( Position, start );
		start = trace.EndPosition;

		// Now trace down from a known safe position
		trace = moveHelper.TraceFromTo( start, end );

		if ( trace.Fraction <= 0 ) return;
		if ( trace.Fraction >= 1 ) return;
		if ( trace.StartedSolid ) return;
		if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > moveHelper.MaxStandableAngle ) return;

		Position = trace.EndPosition;
	}

	[ConCmd.Admin( "set_monster_target" )]
	public static void SetTarget()
	{
		var caller = ConsoleSystem.Caller;
		var pawn = caller.Pawn;

		var tr = Trace.Ray( pawn.EyePosition, pawn.EyePosition + pawn.EyeRotation.Forward * 1024f ).WorldOnly().Run();
		if ( !tr.Hit )
			return;

		var targetPos = tr.EndPosition;
		DebugOverlay.Sphere( targetPos, 4f, Color.Red, 2f, false );

		foreach ( var monster in Entity.All.OfType<MonsterEntity>() )
		{
			monster.SetPath( targetPos );
		}
	}
}

