namespace GvarJam;

public class MovementController : WalkController
{
	private TimeSince TimeSinceSprinted;

	// 0 to 1
	public float Stamina { get; private set; }

	// Per second, while sprinting
	private float StaminaReductionRate => 0.5f;

	// Per second, while not sprinting for over StaminaReplenishDelay
	private float StaminaReplenishRate => 0.5f;
	// Seconds
	private float StaminaReplenishDelay => 1.0f;

	public override void Simulate()
	{
		base.Simulate();

		if ( WishVelocity.Length > DefaultSpeed + 10f )
		{
			Stamina -= StaminaReductionRate * Time.Delta;
			TimeSinceSprinted = 0;
		}
		else if ( TimeSinceSprinted > StaminaReplenishDelay )
		{
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

		if ( Input.Down( InputButton.Run ) && Stamina > 0 ) return SprintSpeed;
		if ( Input.Down( InputButton.Walk ) ) return WalkSpeed;

		return DefaultSpeed;
	}
}
