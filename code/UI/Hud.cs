using GvarJam.UI.Elements;

namespace GvarJam.UI;

/// <summary>
/// The hud entity.
/// </summary>
public sealed partial class Hud : HudEntity<RootPanel>
{
	public static Hud Instance { get; set; }

	public Hud()
	{
		if ( !IsClient )
			return;

		RootPanel.StyleSheet.Load( "/UI/Hud.scss" );
		RootPanel.AddChild( new StaminaBar() );
		RootPanel.AddChild( new BatteryLevel() );
		RootPanel.AddChild( new InteractionProgress() );
		RootPanel.AddChild( new Crosshair() );
		RootPanel.AddChild( new LetterboxBars() );
		RootPanel.AddChild( new SpaceToStart() );
		_ = new InteractionPrompt();

		RootPanel.BindClass( "in-cutscene", () =>
		{
			if ( Local.Pawn is Pawn pawn )
				return pawn.InCutscene;

			return false;
		} );

		RootPanel.BindClass( "in-delayed-cutscene", () =>
		{
			if ( Local.Pawn is Pawn pawn )
				return pawn.InCutscene && pawn.RequiresInputToStart;

			return false;
		} );

		Instance = this;
	}

	[ConCmd.Server]
	public void ShowNote( string contents )
	{
		// TODO: Display a panel with the note contents
		Log.Info( contents );
	}
}
