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
		RootPanel.AddChild( new NotePanel() );
		RootPanel.AddChild( new Elements.Objectives() );
		RootPanel.AddChild( new ObjectiveOverlay() );
		_ = new InteractionPrompt();

		RootPanel.BindClass( "in-cutscene", () =>
		{
			if ( Local.Pawn is Pawn pawn )
				return pawn.InCutscene;

			return false;
		} );

		Instance = this;
	}

	[ConCmd.Server]
	public void ShowNote( string contents )
	{
		NotePanel.Instance?.ShowNote( contents );
	}

	[ConCmd.Server]
	public void HideNote()
	{
		NotePanel.Instance?.HideNote();
	}
}
