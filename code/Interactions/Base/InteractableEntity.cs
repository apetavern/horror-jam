namespace GvarJam.Interactions;

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
	/// The display name of the <see cref="InteractableEntity"/>.
	/// </summary>
	[Net]
	public string? DisplayName { get; set; } = "Default Name";

	/// <summary>
	/// Whether or not the <see cref="InteractableEntity"/> has been previously used.
	/// </summary>
	[Net]
	public bool HasBeenUsed { get; set; } = false;

	/// <summary>
	/// The color that the entity will glow when being looked at.
	/// </summary>
	public virtual Color GlowColor { get; set; } = Color.Green;

	/// <summary>
	/// The color that the entity will glow when being looked at but not usable.
	/// </summary>
	public virtual Color UnusableGlowColor { get; set; } = Color.Red;

	/// <summary>
	/// A dictionary of the items that are required to interact with this entity.
	/// </summary>
	public virtual IReadOnlyDictionary<ItemType, int> RequiredItems => requiredItems;
	/// <summary>
	/// See <see cref="RequiredItems"/>.
	/// </summary>
	private readonly Dictionary<ItemType, int> requiredItems = new();

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
	/// <param name="user">The entity that is looking at this entity.</param>
	/// <param name="shouldShow">Whether or not to show the prompt.</param>
	public void ShowInteractionPrompt( Entity user, bool shouldShow )
	{
		var component = Components.GetOrCreate<Glow>();

		if ( user.GroundEntity is null || !user.Velocity.IsNearZeroLength )
			component.Color = IsUsable( user ) ? Color.Yellow : UnusableGlowColor;
		else
			component.Color = IsUsable( user ) ? GlowColor : UnusableGlowColor;
		component.ObscuredColor = Color.Transparent;

		component.Enabled = shouldShow;
	}

	/// <summary>
	/// Invoked when using the item has finished.
	/// </summary>
	/// <param name="user">The entity that is using the item.</param>
	protected virtual void OnUsed( Entity user )
	{
		(user as Pawn)!.LastInteractedEntityName = Name;
		(user as Pawn)!.LastInteractedEntityTypeName = GetType().Name;
		HasBeenUsed = true;
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

		return user.Position.Distance( Position ) < UseDistance;
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

