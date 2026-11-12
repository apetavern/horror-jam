namespace GvarJam.Player;

public sealed class ColdOpenMovement : Component
{
	private const float StaminaReductionRate = 0.25f;
	private const float StaminaReplenishRate = 0.5f;
	private const float StaminaReplenishDelay = 1.0f;
	private const float MinimumStaminaForSprint = 0.3f;
	private const float JumpSpeed = 300f;
	private const float Gravity = 800f;

	[Sync] public float Stamina { get; set; } = 1f;
	[Sync] public bool IsSprinting { get; set; }

	private TimeSince _timeSinceStaminaUsed;
	private bool _jumpedThisFrame;
	private ColdOpenPlayer _player;

	protected override void OnStart()
	{
		_player = GetComponent<ColdOpenPlayer>();
	}

	public bool ConsumeJumpEvent()
	{
		var j = _jumpedThisFrame;
		_jumpedThisFrame = false;
		return j;
	}

	protected override void OnFixedUpdate()
	{
		if ( _player is null || _player.IsDead ) return;
		var cc = _player.Character;
		if ( cc is null ) return;

		if ( Input.Pressed( "Run" ) && Stamina > MinimumStaminaForSprint )
			IsSprinting = true;

		if ( Input.Released( "Run" ) || Stamina <= 0f )
			IsSprinting = false;

		if ( IsSprinting && !cc.Velocity.IsNearZeroLength )
		{
			Stamina -= StaminaReductionRate * Time.Delta;
			_timeSinceStaminaUsed = 0;
		}
		else if ( (!IsSprinting || cc.Velocity.IsNearZeroLength) && _timeSinceStaminaUsed > StaminaReplenishDelay )
		{
			Stamina += StaminaReplenishRate * Time.Delta;
		}

		Stamina = Stamina.Clamp( 0f, 1f );

		var movement = _player.InputDirection.Normal;
		var yawOnly = _player.ViewAngles.WithPitch( 0f ).WithRoll( 0f );
		var moveVector = Rotation.From( yawOnly ) * movement * 320f;
		var wishSpeed = GetWishSpeed();

		if ( cc.IsOnGround )
		{
			cc.Velocity = Accelerate( cc.Velocity, moveVector.Normal, wishSpeed, 500f, 7.5f );
			cc.ApplyFriction( 4.0f, 100f );
		}
		else
		{
			cc.Velocity = Accelerate( cc.Velocity, moveVector.Normal, wishSpeed, 50f, 3f );
			cc.Velocity += Vector3.Down * Gravity * Time.Delta;
		}

		if ( Input.Pressed( "Jump" ) && cc.IsOnGround && !_player.BlockMovement && !_player.Interaction.IsInteracting )
		{
			cc.Punch( Vector3.Up * JumpSpeed );
			_jumpedThisFrame = true;
		}

		cc.Move();
	}

	private Vector3 Accelerate( Vector3 input, Vector3 wishdir, float wishspeed, float speedLimit, float acceleration )
	{
		if ( speedLimit > 0f && wishspeed > speedLimit )
			wishspeed = speedLimit;

		var currentspeed = Vector3.Dot( input, wishdir );
		var addspeed = wishspeed - currentspeed;
		if ( addspeed <= 0f )
			return input;

		var accelspeed = acceleration * Time.Delta * wishspeed;
		if ( accelspeed > addspeed )
			accelspeed = addspeed;

		return input + wishdir * accelspeed;
	}

	private float GetWishSpeed()
	{
		if ( _player.Interaction.IsInteracting || _player.BlockMovement )
			return 0f;

		if ( Input.Down( "Run" ) && IsSprinting ) return 170f;
		if ( Input.Down( "Walk" ) ) return 110f;
		return 110f;
	}
}
