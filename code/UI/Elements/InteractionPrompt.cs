namespace GvarJam.UI.Elements;

/// <summary>
/// The world panel UI for interaction prompts.
/// </summary>
public sealed class InteractionPrompt : WorldPanel
{
	/// <summary>
	/// The interactable entity the prompt is showing.
	/// </summary>
	private InteractableEntity? interactableEntity;
	/// <summary>
	/// The label showing the interactable entitys name.
	/// </summary>
	private readonly Label label;

	public InteractionPrompt()
	{
		PanelBounds = new Rect( -250, -150, 500, 150 );

		AddClass( "world-panel" );
		StyleSheet.Load( "/UI/Hud.scss" );

		label = Add.Label( "Name" );
		AddClass( "hide" );
	}

	/// <inheritdoc/>
	public override void Tick()
	{
		if ( Local.Pawn is not Pawn pawn )
			return;

		base.Tick();

		var entity = pawn.FindInteractableEntity();
		if ( entity != interactableEntity && interactableEntity is not null && interactableEntity.IsValid )
		{
			interactableEntity?.ShowInteractionPrompt( pawn, false );
			interactableEntity = null;
			AddClass( "hide" );
		}

		if ( entity is null || !entity.IsValid || entity is not InteractableEntity intEntity )
			return;

		if ( intEntity != interactableEntity )
		{
			label.Text = intEntity.DisplayName;
			Position = intEntity.WorldSpaceBounds.Center + Vector3.Up * intEntity.Model.RenderBounds.Size.z * 0.6f;
			Rotation = intEntity.Rotation;

			RemoveClass( "hide" );
		}

		intEntity.ShowInteractionPrompt( pawn, true );
		interactableEntity = intEntity;
		Rotation = Rotation.LookAt( CurrentView.Position - Position );
	}
}
