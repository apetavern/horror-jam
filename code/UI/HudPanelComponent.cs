using GvarJam.UI.Elements;

namespace GvarJam.UI;

public sealed class HudPanelComponent : PanelComponent
{
	public static HudPanelComponent Instance { get; private set; }
	private GameObject _promptGo;

	protected override void OnTreeFirstBuilt()
	{
		base.OnTreeFirstBuilt();
		Instance = this;

		Panel.AddClass( "hud-root" );
		Panel.AddChild( new StaminaBar() );
		Panel.AddChild( new BatteryLevel() );
		Panel.AddChild( new InteractionProgress() );
		Panel.AddChild( new Crosshair() );
		Panel.AddChild( new LetterboxBars() );
		Panel.AddChild( new SpaceToStart() );
		Panel.AddChild( new DeathScreen() );
		Panel.AddChild( new NotePanel() );
		Panel.AddChild( new ObjectivePanel() );
		Panel.AddChild( new ObjectiveOverlay() );
		Panel.AddChild( new PrereqHint() );
		Panel.AddChild( new CreditsPanel() );

		Panel.BindClass( "in-cutscene", () =>
		{
			var p = Game.ActiveScene?.GetAllComponents<ColdOpenPlayer>().FirstOrDefault();
			return p is not null && p.Cutscenes.InCutscene;
		} );

		Panel.BindClass( "has-helmet", () =>
		{
			var p = Game.ActiveScene?.GetAllComponents<ColdOpenPlayer>().FirstOrDefault();
			return p is not null && p.Lamp.HasHelmet;
		} );

		Panel.BindClass( "player-dead", () =>
		{
			var p = Game.ActiveScene?.GetAllComponents<ColdOpenPlayer>().FirstOrDefault();
			return p is not null && p.IsDead;
		} );

		if ( _promptGo is null && Scene is not null )
		{
			_promptGo = new GameObject( true, "InteractionPromptWorldPanel" );
			_promptGo.SetParent( GameObject, false );
			var wp = _promptGo.AddComponent<Sandbox.WorldPanel>();
			wp.PanelSize = new Vector2( 800, 150 );
			wp.LookAtCamera = true;
			_promptGo.AddComponent<InteractionPromptComponent>();
		}
	}
}
