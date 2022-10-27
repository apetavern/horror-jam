namespace GvarJam.Objectives;

/// <summary>
/// Contains information for an end condition in an objective.
/// </summary>
public struct ObjectiveEndCondition
{
	/// <summary>
	/// The type of end condition.
	/// </summary>
	public enum ConditionType
	{
		/// <summary>
		/// Utilizing the interaction system on an entity.
		/// </summary>
		InteractWithEntity,
		/// <summary>
		/// Utilizing the interaction system on a type of entity.
		/// </summary>
		InteractWithType,
		/// <summary>
		/// Entering a zone of the map.
		/// </summary>
		PlayerEnteredTrigger,
		/// <summary>
		/// Player has a specific item in their inventory.
		/// </summary>
		PlayerHasItem,
		/// <summary>
		/// After a time.
		/// </summary>
		Timer
	}

	/// <summary>
	/// The end condition.
	/// </summary>
	public ConditionType Type { get; set; }

	/// <summary>
	/// The name of the entity to interact with to complete.
	/// </summary>
	[ShowIf( nameof( Type ), ConditionType.InteractWithEntity )]
	public string InteractableName { get; set; }

	/// <summary>
	/// The type name of the entity to interact with to complete.
	/// </summary>
	[ShowIf( nameof( Type ), ConditionType.InteractWithType )]
	public string TypeName { get; set; }

	/// <summary>
	/// The name of the trigger to enter to complete.
	/// </summary>
	[ShowIf( nameof( Type ), ConditionType.PlayerEnteredTrigger )]
	public string TriggerName { get; set; }

	/// <summary>
	/// The name of the item that must be in the player's inventory.
	/// </summary>
	[ShowIf( nameof( Type ), ConditionType.PlayerHasItem )]
	public string ItemType { get; set; }

	/// <summary>
	/// The amount of time in seconds to delay for before completing.
	/// </summary>
	[ShowIf( nameof( Type ), ConditionType.Timer )]
	public float TimeDelay { get; set; }

	/// <summary>
	/// Determines whether or not the pawn has met the end conditions.
	/// </summary>
	/// <param name="pawn">The pawn to check the conditions on.</param>
	/// <returns>Whether or not the conditions were met.</returns>
	public bool IsMet( Pawn pawn )
	{
		switch ( Type )
		{
			case ConditionType.InteractWithEntity:
				{
					var entity = Entity.FindByName( InteractableName );

					if ( pawn.IsInteracting && pawn.InteractedEntity == entity )
						return true;

					return false;
				}
			case ConditionType.InteractWithType:
				{
					if ( pawn.IsInteracting && pawn.InteractedEntity?.GetType().Name == TypeName )
						return true;

					return false;
				}
			case ConditionType.PlayerEnteredTrigger:
				{
					var entity = Entity.FindByName( TriggerName );
					if ( entity is not ObjectiveTrigger trigger )
						return false;

					if ( trigger.TouchingEntities.Contains( pawn ) )
						return true;
					return false;
				}
			case ConditionType.PlayerHasItem:
				{
					foreach ( var item in pawn.Items.Keys )
					{
						if ( item.GetType().Name == ItemType )
						{
							return true;
						}
					}
					break;
				}
		}
		return false;
	}
}
