namespace GvarJam;

public partial class MovementController : WalkController
{
	private TimeSince TimeSinceStaminaUsed;

	/// <summary>
	/// 0 to 1
	/// </summary>
	[Net, Predicted] public float Stamina { get; set; }

	/// <summary>
	/// Is the player currently sprinting?
	/// </summary>
	[Net, Predicted] public bool IsSprinting { get; set; }

	/// <summary>
	/// Per second, while sprinting
	/// </summary>
	private float StaminaReductionRate => 0.5f;

	/// <summary>
	/// Per second, while not sprinting for over StaminaReplenishDelay
	/// </summary>
	private float StaminaReplenishRate => 0.5f;

	/// <summary>
	/// How long (in seconds) before stamina replenishes?
	/// </summary>
	// Seconds
	private float StaminaReplenishDelay => 1.0f;

	/// <summary>
	/// How much stamina is required to engage sprint?
	/// </summary>
	public float MinimumStaminaForSprint => 0.3f;

	/// <summary>
	/// How much stamina does jumping cost? (Applied instantly)
	/// </summary>
	public float JumpStaminaReduction => 0.2f;

	public override void Simulate()
	{
		base.Simulate();

		//
		// Player started sprint?
		//
		if ( Input.Pressed( InputButton.Run ) && Stamina > MinimumStaminaForSprint )
		{
			IsSprinting = true;
		}

		//
		// End sprint?
		//
		if ( Input.Released( InputButton.Run ) || Stamina <= 0f )
		{
			IsSprinting = false;
		}

		if ( IsSprinting )
		{
			//
			// Sprint reduction
			//
			Stamina -= StaminaReductionRate * Time.Delta;
			TimeSinceStaminaUsed = 0;
		}
		else if ( !IsSprinting && TimeSinceStaminaUsed > StaminaReplenishDelay )
		{
			//
			// Sprint regen
			//
			Stamina += StaminaReplenishRate * Time.Delta;
		}

		Stamina = Stamina.Clamp( 0, 1 );
	}

	public override float GetWishSpeed()
	{
		if ( (Pawn as Pawn)!.IsInteracting )
			return 0;

		var ws = Duck.GetWishSpeed();
		if ( ws >= 0 ) return ws;

		if ( Input.Down( InputButton.Run ) && IsSprinting ) return SprintSpeed;
		if ( Input.Down( InputButton.Walk ) ) return WalkSpeed;

		return DefaultSpeed;
	}

	public override void CheckJumpButton()
	{
		// If we are in the water most of the way...
		if ( Swimming )
		{
			// swimming, not jumping
			ClearGroundEntity();

			Velocity = Velocity.WithZ( 100 );

			return;
		}

		if ( GroundEntity == null )
			return;

		ClearGroundEntity();

		float flGroundFactor = 1.0f;
		float flMul = 320f;

		if ( Stamina < JumpStaminaReduction )
			flMul *= 0.75f;

		Stamina -= JumpStaminaReduction;
		TimeSinceStaminaUsed = 0;

		float startz = Velocity.z;

		if ( Duck.IsActive )
			flMul *= 0.8f;

		Velocity = Velocity.WithZ( startz + flMul * flGroundFactor );
		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

		AddEvent( "jump" );
	}
}
