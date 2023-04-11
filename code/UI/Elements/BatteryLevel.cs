namespace GvarJam.UI.Elements;

/// <summary>
/// The battery meter.
/// </summary>
public sealed class BatteryLevel : Panel
{
	/// <summary>
	/// The panel that represents the charge of a battery.
	/// </summary>
	private Panel FillPanel;

	/// <summary>
	/// Initializes a default instance of <see cref="BatteryLevel"/>.
	/// </summary>
	public BatteryLevel()
	{
		AddClass( "auto-hide" );

		Add.Panel( "frame" );
		FillPanel = Add.Panel( "fill" );
	}

	/// <inheritdoc/>
	public override void Tick()
	{
		if ( Game.LocalPawn is not Pawn player )
			return;

		SetClass( "visible", !player.LampPower.AlmostEqual( 100f ) && player.Helmet is not null );
		SetClass( "danger", player.LampPower <= 30f );

		// LampPower is in range 0 .. 100
		FillPanel.Style.Width = Length.Percent( player.LampPower );
	}
}
