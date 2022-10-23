namespace GvarJam.Interactions;

public partial class InteractableEntity : ModelEntity, IInteractable
{
	/// <summary>
	/// The entity that is currently using the item.
	/// </summary>
	[Net]
	public Entity? User { get; protected set; }

	public virtual Color GlowColor { get; set; } = Color.Orange;

	protected InteractionPromptPanel? InteractionPromptPanel { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	public virtual bool IsUsable( Entity user )
	{
		return user.Position.Distance( Position ) < 100;
	}

	/// <summary>
	/// Invoked when using the item has finished.
	/// </summary>
	/// <param name="user">The entity that is using the item.</param>
	protected virtual void OnUsed( Entity user )
	{
		DebugOverlay.Text( "Used", Position, 1 );
	}

	public virtual bool OnUse( Entity user )
	{
		OnUsed( user );

		return true;
	}

	public virtual void Reset() {}

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

		if ( !shouldShow )
		{
			InteractionPromptPanel?.Delete();
			return;
		}

		if ( Host.IsServer )
			return;

		if ( InteractionPromptPanel is null || !InteractionPromptPanel.IsValid )
		{
			InteractionPromptPanel = new InteractionPromptPanel( this );
			InteractionPromptPanel.Position = Position + Vector3.Up * Model.RenderBounds.Size.z;
			InteractionPromptPanel.Rotation = Rotation;
		}

		InteractionPromptPanel.Rotation = Rotation.LookAt( CurrentView.Position - InteractionPromptPanel.Position );
	}
}

