namespace GvarJam.UI.Elements;

public sealed class StaminaBar : Panel
{
	private readonly Panel _fillPanel;

	public StaminaBar()
	{
		AddClass( "auto-hide" );
		var left = Add.Panel( "left" );
		_ = left.Add.Icon( "bolt", "icon" );

		var right = Add.Panel( "right" );
		var bar = right.Add.Panel( "bar" );
		_fillPanel = bar.Add.Panel( "fill" );
	}

	public override void Tick()
	{
		base.Tick();
		var player = Game.ActiveScene?.GetAllComponents<ColdOpenPlayer>().FirstOrDefault();
		if ( player?.Movement is null ) return;

		_fillPanel.Style.Width = Length.Fraction( player.Movement.Stamina );
		SetClass( "visible", !player.Movement.Stamina.AlmostEqual( 1f ) );
	}
}
