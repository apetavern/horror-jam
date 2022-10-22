﻿namespace GvarJam.UI.Elements;

internal class StaminaBar : Panel
{
	private Panel FillPanel;

	public StaminaBar()
	{
		//
		// Might move this to html?
		//
		var left = Add.Panel( "left" );
		_ = left.Add.Icon( "bolt", "icon" );

		var right = Add.Panel( "right" );
		var bar = right.Add.Panel( "bar" );
		FillPanel = bar.Add.Panel( "fill" );

		_ = right.Add.Label( "Stamina", "label" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not Pawn { Controller: MovementController controller } )
			return;

		FillPanel.Style.Width = Length.Fraction( controller.Stamina );
		SetClass( "visible", !controller.Stamina.AlmostEqual( 1 ) );
	}
}
