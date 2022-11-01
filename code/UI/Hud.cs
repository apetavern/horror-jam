using GvarJam.UI.Elements;

namespace GvarJam.UI;

/// <summary>
/// The hud entity.
/// </summary>
public sealed partial class Hud : HudEntity<RootPanel>
{
	/// <summary>
	/// The only instance of <see cref="Hud"/> in existance.
	/// </summary>
	public static Hud Instance { get; set; }

	/// <summary>
	/// Initializes a default instance of <see cref="Hud"/>.
	/// </summary>
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
		RootPanel.AddChild( new DeathScreen() );
		RootPanel.AddChild( new NotePanel() );
		RootPanel.AddChild( new Elements.Objectives() );
		RootPanel.AddChild( new ObjectiveOverlay() );
		RootPanel.AddChild( new PrereqHint() );
		RootPanel.AddChild( new Credits() );
		_ = new InteractionPrompt();

		RootPanel.BindClass( "in-cutscene", () =>
		{
			if ( Local.Pawn is Pawn pawn )
				return pawn.InCutscene;

			return false;
		} );

		RootPanel.BindClass( "has-helmet", () =>
		{
			if ( Local.Pawn is Pawn pawn )
				return pawn.Helmet.IsValid();

			return false;
		} );

		RootPanel.BindClass( "player-dead", () =>
		{
			if ( Local.Pawn is Pawn pawn )
				return pawn.LifeState == LifeState.Dead;

			return false;
		} );

		Instance = this;
	}
}
