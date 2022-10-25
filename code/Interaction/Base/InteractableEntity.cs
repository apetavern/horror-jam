﻿namespace GvarJam.Interactions;

/// <summary>
/// Represents an interactable entity.
/// </summary>
[Category( "Interactables" )]
public partial class InteractableEntity : AnimatedEntity, IInteractable
{
	/// <summary>
	/// The entity that is currently using the item.
	/// </summary>
	[Net]
	public Entity? User { get; protected set; }

	/// <summary>
	/// The color that the entity will glow when being looked at.
	/// </summary>
	public virtual Color GlowColor { get; set; } = Color.Orange;

	/// <summary>
	/// A dictionary of the items that are required to interact with this entity.
	/// </summary>
	public virtual IReadOnlyDictionary<ItemType, int> RequiredItems => requiredItems;
	private readonly Dictionary<ItemType, int> requiredItems = new();

	/// <summary>
	/// The UI prompt panel for this entity.
	/// </summary>
	protected InteractionPromptPanel? InteractionPromptPanel { get; set; }

	/// <summary>
	/// The maximum distance you can interact with the entity.
	/// </summary>
	private const float UseDistance = 100;

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

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

		if ( IsServer )
			return;

		if ( InteractionPromptPanel is null || !InteractionPromptPanel.IsValid )
		{
			InteractionPromptPanel = new InteractionPromptPanel( this )
			{
				Position = Position + Vector3.Up * Model.RenderBounds.Size.z,
				Rotation = Rotation
			};
		}

		InteractionPromptPanel.Rotation = Rotation.LookAt( CurrentView.Position - InteractionPromptPanel.Position );
	}

	/// <summary>
	/// Invoked when using the item has finished.
	/// </summary>
	/// <param name="user">The entity that is using the item.</param>
	protected virtual void OnUsed( Entity user )
	{
	}

	/// <inheritdoc/>
	public virtual bool IsUsable( Entity user )
	{
		var pawn = (user as Pawn)!;
		foreach ( var (item, amount) in RequiredItems )
		{
			if ( !pawn.HasItem( item, amount ) )
				return false;
		}

		return user.Position.Distance( Position ) < UseDistance && user.GroundEntity is not null && user.Velocity.IsNearZeroLength;
	}

	/// <inheritdoc/>
	public virtual bool OnUse( Entity user )
	{
		OnUsed( user );

		return false;
	}

	/// <inheritdoc/>
	public virtual void Reset()
	{
	}
}

