namespace GvarJam.Player;

/// <summary>
/// The controller for the pawn.
/// </summary>
public sealed partial class MovementController : EntityComponent<Pawn>
{
	/// <summary>
	/// Per second, while sprinting.
	/// </summary>
	private const float StaminaReductionRate = 0.25f;

	/// <summary>
	/// Per second, while not sprinting for over <see cref="StaminaReplenishDelay"/>.
	/// </summary>
	private const float StaminaReplenishRate = 0.5f;

	/// <summary>
	/// How long (in seconds) before stamina replenishes?
	/// </summary>
	private const float StaminaReplenishDelay = 1.0f;

	/// <summary>
	/// How much stamina is required to engage sprint?
	/// </summary>
	private const float MinimumStaminaForSprint = 0.3f;

	/// <summary>
	/// How much stamina does jumping cost? (Applied instantly)
	/// </summary>
	private const float JumpStaminaReduction = 0.2f;

	/// <summary>
	/// The current stamina the pawn has.
	/// </summary>
	[Net, Predicted]
	public float Stamina { get; set; }

	/// <summary>
	/// Whether or not the pawn is sprinting.
	/// </summary>
	[Net, Predicted]
	public bool IsSprinting { get; set; }

	/// <summary>
	/// The time since stamina was last used.
	/// </summary>
	private TimeSince timeSinceStaminaUsed;

	public int StepSize => 24;
	public int GroundAngle => 45;
	public int JumpSpeed => 300;
	public float Gravity => 800f;

	HashSet<string> ControllerEvents = new( StringComparer.OrdinalIgnoreCase );

	bool Grounded => Entity.GroundEntity.IsValid();

	public void Simulate( IClient _ )
	{
		ControllerEvents.Clear();

		if ( Input.Pressed( InputButton.Run ) && Stamina > MinimumStaminaForSprint )
			IsSprinting = true;

		if ( Input.Released( InputButton.Run ) || Stamina <= 0f )
			IsSprinting = false;

		if ( IsSprinting && !Entity.Velocity.IsNearZeroLength )
		{
			Stamina -= StaminaReductionRate * Time.Delta;
			timeSinceStaminaUsed = 0;
		}
		else if ( (!IsSprinting || Entity.Velocity.IsNearZeroLength) && timeSinceStaminaUsed > StaminaReplenishDelay )
		{
			Stamina += StaminaReplenishRate * Time.Delta;
		}

		Stamina = Stamina.Clamp( 0, 1 );

		var movement = Entity.InputDirection.Normal;
		var angles = Camera.Rotation.Angles().WithPitch( 0 );
		var moveVector = Rotation.From( angles ) * movement * 320f;
		var groundEntity = CheckForGround();
		var wishSpeed = GetWishSpeed();

		if ( groundEntity.IsValid() )
		{
			if ( !Grounded )
			{
				Entity.Velocity = Entity.Velocity.WithZ( 0 );
				AddEvent( "grounded" );
			}

			Entity.Velocity = Accelerate( Entity.Velocity, moveVector.Normal, wishSpeed, 500f, 7.5f );
			Entity.Velocity = ApplyFriction( Entity.Velocity, 4.0f );
		}
		else
		{
			Entity.Velocity = Accelerate( Entity.Velocity, moveVector.Normal, wishSpeed, 50f, 3f );
			Entity.Velocity += Vector3.Down * Gravity * Time.Delta;
		}

		if ( Input.Pressed( InputButton.Jump ) )
		{
			DoJump();
		}

		var mh = new MoveHelper( Entity.Position, Entity.Velocity );
		mh.Trace = mh.Trace.Size( Entity.Hull ).Ignore( Entity );

		if ( mh.TryMoveWithStep( Time.Delta, StepSize ) > 0 )
		{
			if ( Grounded )
			{
				mh.Position = StayOnGround( mh.Position );
			}

			Entity.Position = mh.Position;
			Entity.Velocity = mh.Velocity;
		}

		Entity.GroundEntity = groundEntity;
	}

	void DoJump()
	{
		if ( Grounded )
		{
			Entity.Velocity = ApplyJump( Entity.Velocity, "jump" );
		}
	}

	Entity CheckForGround()
	{
		if ( Entity.Velocity.z > 100f )
			return null;

		var trace = Entity.TraceBBox( Entity.Position, Entity.Position + Vector3.Down, 2f );

		if ( !trace.Hit )
			return null;

		if ( trace.Normal.Angle( Vector3.Up ) > GroundAngle )
			return null;

		return trace.Entity;
	}

	Vector3 ApplyFriction( Vector3 input, float frictionAmount )
	{
		float StopSpeed = 100.0f;

		var speed = input.Length;
		if ( speed < 0.1f ) return input;

		// Bleed off some speed, but if we have less than the bleed
		// threshold, bleed the threshold amount.
		float control = (speed < StopSpeed) ? StopSpeed : speed;

		// Add the amount to the drop amount.
		var drop = control * Time.Delta * frictionAmount;

		// scale the velocity
		float newspeed = speed - drop;
		if ( newspeed < 0 ) newspeed = 0;
		if ( newspeed == speed ) return input;

		newspeed /= speed;
		input *= newspeed;

		return input;
	}

	Vector3 Accelerate( Vector3 input, Vector3 wishdir, float wishspeed, float speedLimit, float acceleration )
	{
		if ( speedLimit > 0 && wishspeed > speedLimit )
			wishspeed = speedLimit;

		var currentspeed = input.Dot( wishdir );
		var addspeed = wishspeed - currentspeed;

		if ( addspeed <= 0 )
			return input;

		var accelspeed = acceleration * Time.Delta * wishspeed;

		if ( accelspeed > addspeed )
			accelspeed = addspeed;

		input += wishdir * accelspeed;

		return input;
	}

	float GetWishSpeed()
	{
		if ( Entity.IsInteracting || Entity.BlockMovement )
			return 0;

		if ( Input.Down( InputButton.Run ) && IsSprinting ) return 170f;
		if ( Input.Down( InputButton.Walk ) ) return 110f;

		return 110f;
	}

	Vector3 ApplyJump( Vector3 input, string jumpType )
	{
		AddEvent( jumpType );

		return input + Vector3.Up * JumpSpeed;
	}

	Vector3 StayOnGround( Vector3 position )
	{
		var start = position + Vector3.Up * 2;
		var end = position + Vector3.Down * StepSize;

		// See how far up we can go without getting stuck
		var trace = Entity.TraceBBox( position, start );
		start = trace.EndPosition;

		// Now trace down from a known safe position
		trace = Entity.TraceBBox( start, end );

		if ( trace.Fraction <= 0 ) return position;
		if ( trace.Fraction >= 1 ) return position;
		if ( trace.StartedSolid ) return position;
		if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > GroundAngle ) return position;

		return trace.EndPosition;
	}

	public bool HasEvent( string eventName )
	{
		return ControllerEvents.Contains( eventName );
	}

	void AddEvent( string eventName )
	{
		if ( HasEvent( eventName ) )
			return;

		ControllerEvents.Add( eventName );
	}
}
