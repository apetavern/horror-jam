namespace GvarJam.Interactions;

/// <summary>
/// The UI world panel for interaction prompts.
/// </summary>
public sealed class InteractionPromptPanel : WorldPanel
{
	private readonly InteractableEntity interactableEntity;
	private readonly Label label;

	public InteractionPromptPanel( InteractableEntity entity )
	{
		if ( entity is null )
			return;

		interactableEntity = entity;

		PanelBounds = new Rect( -250, -150, 500, 150 );

		AddClass( "world-panel" );
		StyleSheet.Load( "/UI/Hud.scss" );

		label = Add.Label( entity.Name );
	}

	public override void Tick()
	{
		base.Tick();

		label.Text = interactableEntity.Name;
	}
}
