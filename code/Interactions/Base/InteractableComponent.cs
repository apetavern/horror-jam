namespace GvarJam.Interactions;

public abstract class InteractableComponent : Component
{
	[Property, Sync] public string DisplayName { get; set; } = "Default Name";
	[Sync] public ColdOpenPlayer User { get; protected set; }
	[Sync] public bool HasBeenUsed { get; set; }

	public virtual string LegacyTypeName => GetType().Name;
	public virtual Color GlowColor => Color.Green;
	public virtual Color UnusableGlowColor => Color.Red;
	public virtual IReadOnlyDictionary<ItemType, int> RequiredItems => _requiredItems;

	protected readonly Dictionary<ItemType, int> _requiredItems = new();
	private const float UseDistance = 100f;

	public void ShowInteractionPrompt( ColdOpenPlayer user, bool shouldShow )
	{
		var outline = GameObject.GetOrAddComponent<HighlightOutline>();
		if ( !user.IsGrounded || !user.Velocity.IsNearZeroLength )
			outline.Color = IsUsable( user ) ? Color.Yellow : UnusableGlowColor;
		else
			outline.Color = IsUsable( user ) ? GlowColor : UnusableGlowColor;

		outline.ObscuredColor = Color.Transparent;
		outline.Enabled = shouldShow;
	}

	protected virtual void OnUsed( ColdOpenPlayer user )
	{
		user.LastInteractedEntityName = GameObject.Name;
		user.LastInteractedEntityTypeName = LegacyTypeName;
		HasBeenUsed = true;
	}

	public virtual bool IsUsable( ColdOpenPlayer user )
	{
		foreach ( var (item, amount) in RequiredItems )
		{
			if ( !user.Inventory.HasItem( item, amount ) )
				return false;
		}

		return user.WorldPosition.Distance( WorldPosition ) < UseDistance;
	}

	public virtual bool OnUse( ColdOpenPlayer user )
	{
		OnUsed( user );
		return false;
	}

	public virtual void ResetInteraction()
	{
	}
}
