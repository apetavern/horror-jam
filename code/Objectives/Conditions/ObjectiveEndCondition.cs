namespace GvarJam.Objectives;

public struct ObjectiveEndCondition
{
	public enum ConditionType
	{
		InteractWithEntity,
		InteractWithType,
		PlayerEnteredTrigger,
		Timer
	}

	public ConditionType Type { get; set; }

	[ShowIf( nameof( Type ), ConditionType.PlayerEnteredTrigger )]
	public string TriggerName { get; set; }

	[ShowIf( nameof( Type ), ConditionType.InteractWithEntity )]
	public string InteractableName { get; set; }

	[ShowIf( nameof( Type ), ConditionType.InteractWithType )]
	public string TypeName { get; set; }

	[ShowIf( nameof( Type ), ConditionType.Timer )]
	public float TimeDelay { get; set; }

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
					break;
				}
		}
		return false;
	}
}
