namespace GvarJam.UI.Elements;

public sealed class BatteryLevel : Panel
{
	private readonly Panel _fillPanel;

	public BatteryLevel()
	{
		AddClass( "auto-hide" );
		Add.Panel( "frame" );
		_fillPanel = Add.Panel( "fill" );
	}

	public override void Tick()
	{
		var player = Game.ActiveScene?.GetAllComponents<ColdOpenPlayer>().FirstOrDefault();
		if ( player is null ) return;

		SetClass( "visible", !player.Lamp.LampPower.AlmostEqual( 100f ) && player.Lamp.HasHelmet );
		SetClass( "danger", player.Lamp.LampPower <= 30f );
		_fillPanel.Style.Width = Length.Percent( player.Lamp.LampPower );
	}
}
