using GvarJam.UI.Elements;

namespace GvarJam.UI;

/// <summary>
/// The hud entity.
/// </summary>
public sealed partial class Hud : HudEntity<RootPanel>
{
	public Hud()
	{
		if ( !IsClient )
			return;

		RootPanel.StyleSheet.Load( "/UI/Hud.scss" );
		RootPanel.AddChild( new StaminaBar() );
		RootPanel.AddChild( new BatteryLevel() );
		RootPanel.AddChild( new InteractionProgress() );
		RootPanel.AddChild( new Crosshair() );
	}
}
