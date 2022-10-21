namespace GvarJam;

public partial class MovementController : WalkController
{
	private TimeSince TimeSinceSprinted;

	// 0 to 1
	[Net, Predicted] public float Stamina { get; set; }

	// Is the player currently sprinting?
	[Net, Predicted] public bool IsSprinting { get; set; }

	// Per second, while sprinting
	private float StaminaReductionRate => 0.5f;

	// Per second, while not sprinting for over StaminaReplenishDelay
	private float StaminaReplenishRate => 0.5f;
	// Seconds
	private float StaminaReplenishDelay => 1.0f;

	// How much stamina is required to engage sprint?
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
		DebugOverlay.ScreenText( $"{Stamina}", 5 );
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
