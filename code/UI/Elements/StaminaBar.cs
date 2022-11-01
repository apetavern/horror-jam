namespace GvarJam.UI.Elements;

/// <summary>
/// The stamina bar.
/// </summary>
public sealed class StaminaBar : Panel
{
	/// <summary>
	/// The panel that represents how much stamina the player has.
	/// </summary>
	private readonly Panel FillPanel;

	/// <summary>
	/// Initializes a default instance of <see cref="StaminaBar"/>.
	/// </summary>
	public StaminaBar()
	{
		AddClass( "auto-hide" );

		//
		// Might move this to html?
		//
		var left = Add.Panel( "left" );
		_ = left.Add.Icon( "bolt", "icon" );

		var right = Add.Panel( "right" );
		var bar = right.Add.Panel( "bar" );
		FillPanel = bar.Add.Panel( "fill" );
	}

	/// <inheritdoc/>
	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not Pawn { Controller: MovementController controller } )
			return;

		// LampPower is in range 0 .. 1
		FillPanel.Style.Width = Length.Fraction( controller.Stamina );
		SetClass( "visible", !controller.Stamina.AlmostEqual( 1 ) );
	}
}
