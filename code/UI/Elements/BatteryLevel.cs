namespace GvarJam.UI.Elements;

internal class BatteryLevel : Panel
{
	private Panel FramePanel;
	private Panel FillPanel;

	public BatteryLevel()
	{
		FramePanel = Add.Panel( "frame" );
		FillPanel = Add.Panel( "fill" );
	}

	public override void Tick()
	{
		if ( Local.Pawn is not Pawn player )
			return;

		SetClass( "visible", !player.LampPower.AlmostEqual( 100f ) );
		SetClass( "danger", player.LampPower <= 30f );

		// LampPower is in range 0 .. 100
		FillPanel.Style.Width = Length.Percent( player.LampPower );
	}
}
