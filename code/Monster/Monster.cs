namespace GvarJam.Monster;

public partial class MonsterEntity : AnimatedEntity
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

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/enemy/monster.vmdl" );
		PathFinding = new( this );
	}

	public void SetPath( Vector3 target )
	{
		PathFinding.SetPath( target );
	}

	[Event.Tick]
	public void OnServerTick()
	{
		EyeLocalPosition = Vector3.Up * 64f;

		if ( IsServer )
		{
			if ( DebugEnabled )
			{
				DebugOverlay.Box( Position, Bounds.Mins, Bounds.Maxs, Color.White );
				DrawDebugInfo();
			}

			TickMove();
			TickState();

			TickStuck();
		}

		TickAnimator();
	}

	private float AverageSpeed { get; set; } = 50f;
	private void TickStuck()
	{
		AverageSpeed = AverageSpeed.LerpTo( Velocity.Length, Time.Delta );

		if ( AverageSpeed < 10f )
		{
			// We're stuck, but we don't want to be stuck, so do something
			// so that the player doesn't think I'm a terrible useless
			// programmer

			State = States.Stalking;
			AverageSpeed = 50f;
		}
	}


	private void DrawDebugInfo()
	{
		var player = Entity.All.OfType<Pawn>().First();
		var pos = Position + Vector3.Up * 64 + player.EyeRotation.Right * 12f;

		var info =
			$"grounded?: {IsGrounded}\n" +
			$"speed: {Velocity.Length}\n" +
			$"average speed: {AverageSpeed}";

		DebugOverlay.Text( info, pos, 0, Color.White, 0 );

		var stateInfo =
			$"state: {State}\n" +
			$"time in state: {TimeInState}\n" +
			$"time since saw player: {TimeSinceSawPlayer}\n";

		DebugOverlay.ScreenText( stateInfo, 25, 0 );
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

	private void TickAnimator()
	{
		float walkspeed = Velocity.Length.LerpInverse( 0, 200 ) * 2.0f;

		if ( DebugEnabled )
			DebugOverlay.ScreenText( $"{walkspeed}, {Velocity.Length}", -1 );

		SetAnimParameter( "idle", true );
		SetAnimParameter( "walkspeed", walkspeed );
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

	[ConCmd.Admin( "set_monster_state" )]
	public static void SetState( States newState )
	{
		var caller = ConsoleSystem.Caller;
		var pawn = caller.Pawn;

		foreach ( var monster in Entity.All.OfType<MonsterEntity>() )
		{
			monster.State = newState;
		}
	}
}
