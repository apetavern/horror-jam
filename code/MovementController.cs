namespace GvarJam;

public partial class MovementController : WalkController
{
	private TimeSince TimeSinceSprinted;

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
			TimeSinceSprinted = 0;
		}
		else if ( !IsSprinting && TimeSinceSprinted > StaminaReplenishDelay )
		{
			//
			// Sprint regen
			//
			Stamina += StaminaReplenishRate * Time.Delta;
		}

		Stamina = Stamina.Clamp( 0, 1 );
		DebugOverlay.ScreenText( $"Stamina: {Stamina}", 2 );
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
}
