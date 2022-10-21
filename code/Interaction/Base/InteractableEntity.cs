namespace GvarJam.Interactions;

public class InteractableEntity : ModelEntity
{
	public virtual Color GlowColor { get; set; } = Color.Orange;

	protected InteractionPromptPanel? InteractionPromptPanel { get; set; }

	/// <summary>
	/// Enable or disable the interaction prompt on this entity.
	/// </summary>
	/// <param name="shouldShow"></param>
	public void ShowInteractionPrompt( bool shouldShow )
	{
		var component = Components.GetOrCreate<Glow>();

		component.Color = GlowColor;
		component.ObscuredColor = Color.Transparent;

		component.Enabled = shouldShow;

		if( !shouldShow )
		{
			InteractionPromptPanel?.Delete();
			return;
		}

		if ( Host.IsServer )
			return;

		if( InteractionPromptPanel is null || !InteractionPromptPanel.IsValid )
		{
			InteractionPromptPanel = new InteractionPromptPanel(this);
			InteractionPromptPanel.Position = Position + Vector3.Up * Model.RenderBounds.Size.z;
			InteractionPromptPanel.Rotation = Rotation;
		}

		InteractionPromptPanel.Rotation = Rotation.LookAt( Local.Pawn.Position - Position );
	}
}

