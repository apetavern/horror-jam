namespace GvarJam.Interactions;

/// <summary>
/// The world panel UI for interaction prompts.
/// </summary>
public sealed class InteractionPromptPanel : WorldPanel
{
	/// <summary>
	/// The interactable entity the prompt is showing.
	/// </summary>
	private readonly InteractableEntity interactableEntity;
	/// <summary>
	/// The label showing the interactable entitys name.
	/// </summary>
	private readonly Label label;

	public InteractionPromptPanel( InteractableEntity entity )
	{
		interactableEntity = entity;

		PanelBounds = new Rect( -250, -150, 500, 150 );

		AddClass( "world-panel" );
		StyleSheet.Load( "/UI/Hud.scss" );

		label = Add.Label( entity.Name );
	}

	/// <inheritdoc/>
	public override void Tick()
	{
		base.Tick();

		label.Text = interactableEntity.Name;
	}
}
