namespace GvarJam.Player;

/// <summary>
/// The controller for the pawn.
/// </summary>
public sealed partial class MovementController : WalkController
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

	/// <inheritdoc/>
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

		if ( IsSprinting && !Velocity.IsNearZeroLength )
		{
			//
			// Sprint reduction
			//
			Stamina -= StaminaReductionRate * Time.Delta;
			timeSinceStaminaUsed = 0;
		}
		else if ( (!IsSprinting || Velocity.IsNearZeroLength) && timeSinceStaminaUsed > StaminaReplenishDelay )
		{
			//
			// Sprint regen
			//
			Stamina += StaminaReplenishRate * Time.Delta;
		}

		Stamina = Stamina.Clamp( 0, 1 );
	}

	/// <inheritdoc/>
	public override float GetWishSpeed()
	{
		if ( Pawn is Pawn pawn && (pawn.IsInteracting || pawn.BlockMovement) )
			return 0;

		var ws = Duck.GetWishSpeed();
		if ( ws >= 0 ) return ws;

		if ( Input.Down( InputButton.Run ) && IsSprinting ) return SprintSpeed;
		if ( Input.Down( InputButton.Walk ) ) return WalkSpeed;

		return DefaultSpeed;
	}

	/// <inheritdoc/>
	public override void CheckJumpButton()
	{
		if ( Pawn is Pawn pawn && (pawn.IsInteracting || pawn.BlockMovement) )
			return;

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
		timeSinceStaminaUsed = 0;

		float startz = Velocity.z;

		if ( Duck.IsActive )
			flMul *= 0.8f;

		Velocity = Velocity.WithZ( startz + flMul * flGroundFactor );
		Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

		AddEvent( "jump" );
	}
}
