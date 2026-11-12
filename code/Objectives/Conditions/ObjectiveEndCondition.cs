namespace GvarJam.Objectives;

public struct ObjectiveEndCondition
{
	public enum ConditionType
	{
		InteractWithEntity,
		InteractedWithEntity,
		InteractWithType,
		InteractedWithType,
		PlayerEnteredTrigger,
		PlayerHasItem,
		Timer,
	}

	public ConditionType Type { get; set; }

	[ShowIf( nameof( Type ), ConditionType.InteractWithEntity )]
	public string InteractableName { get; set; }

	[ShowIf( nameof( Type ), ConditionType.InteractedWithEntity )]
	public string InteractedName { get; set; }

	[ShowIf( nameof( Type ), ConditionType.InteractWithType )]
	public string TypeName { get; set; }

	[ShowIf( nameof( Type ), ConditionType.InteractedWithType )]
	public string InteractedTypeName { get; set; }

	[ShowIf( nameof( Type ), ConditionType.PlayerEnteredTrigger )]
	public string TriggerName { get; set; }

	[ShowIf( nameof( Type ), ConditionType.PlayerHasItem )]
	public string ItemType { get; set; }

	[ShowIf( nameof( Type ), ConditionType.Timer )]
	public float TimeDelay { get; set; }

	public bool IsMet( ColdOpenPlayer player, TimeSince timeSinceObjectiveStart )
	{
		var scene = player.Scene;
		switch ( Type )
		{
			case ConditionType.InteractWithEntity:
			{
				if ( string.IsNullOrEmpty( InteractableName ) ) return false;
				var name = InteractableName;
				var go = scene.GetAllObjects( true ).FirstOrDefault( x => x.Name.Contains( name, StringComparison.OrdinalIgnoreCase ) );
				return player.Interaction.IsInteracting && player.Interaction.InteractedEntity?.GameObject == go;
			}
			case ConditionType.InteractedWithEntity:
			{
				if ( string.IsNullOrEmpty( InteractedName ) || string.IsNullOrEmpty( player.LastInteractedEntityName ) )
					return false;
				return player.LastInteractedEntityName.Contains( InteractedName, StringComparison.OrdinalIgnoreCase );
			}
			case ConditionType.InteractWithType:
			{
				return player.Interaction.IsInteracting && MatchesTypeName( player.Interaction.InteractedEntity, TypeName );
			}
			case ConditionType.InteractedWithType:
			{
				return MatchesLegacyTypeName( player.LastInteractedEntityTypeName, InteractedTypeName );
			}
			case ConditionType.PlayerEnteredTrigger:
			{
				if ( string.IsNullOrEmpty( TriggerName ) ) return false;
				var triggerName = TriggerName;
				foreach ( var trigger in scene.GetAllComponents<ObjectiveTriggerComponent>() )
				{
					if ( trigger.GameObject.Name.Contains( triggerName, StringComparison.OrdinalIgnoreCase ) && trigger.IsTouching( player ) )
						return true;
				}
				return false;
			}
			case ConditionType.PlayerHasItem:
			{
				if ( Enum.TryParse<ItemType>( ItemType, out var item ) )
					return player.Inventory.HasItem( item, 1 );
				return false;
			}
			case ConditionType.Timer:
			{
				return timeSinceObjectiveStart > TimeDelay;
			}
		}
		return false;
	}

	public static bool MatchesTypeName( InteractableComponent comp, string targetName )
	{
		if ( comp is null || string.IsNullOrEmpty( targetName ) ) return false;
		return MatchesLegacyTypeName( comp.LegacyTypeName, targetName ) || comp.GetType().Name == targetName;
	}

	public static bool MatchesLegacyTypeName( string actual, string expected )
	{
		if ( string.IsNullOrEmpty( actual ) || string.IsNullOrEmpty( expected ) ) return false;
		return string.Equals( actual, expected, StringComparison.OrdinalIgnoreCase )
			|| string.Equals( actual, expected + "Component", StringComparison.OrdinalIgnoreCase );
	}
}
