namespace GvarJam.UI.Elements;

internal class StaminaBar : Panel
{
	private Panel FillPanel;

	public StaminaBar()
	{
		_ = Add.Icon( "bolt", "icon" );
		FillPanel = Add.Panel( "fill" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not Pawn { Controller: MovementController controller } )
			return;

		FillPanel.Style.Width = Box.Rect.Width * controller.Stamina * ScaleFromScreen;
		SetClass( "visible", !controller.Stamina.AlmostEqual( 1 ) );
	}
}
