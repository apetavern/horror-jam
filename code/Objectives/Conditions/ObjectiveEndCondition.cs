namespace GvarJam.Objectives;

public struct ObjectiveEndCondition
{
	public enum ConditionType
	{
		OnInteractWithEntity,
		OnInteractWithType,
		OnTrigger,
		OnTimer
	}

	public ConditionType Type { get; set; }

	[ShowIf( nameof( Type ), ConditionType.OnTrigger )]
	public string TriggerName { get; set; }

	[ShowIf( nameof( Type ), ConditionType.OnInteractWithEntity )]
	public string InteractableName { get; set; }

	[ShowIf( nameof( Type ), ConditionType.OnInteractWithType )]
	public string TypeName { get; set; }

	[ShowIf( nameof( Type ), ConditionType.OnTimer )]
	public float TimeDelay { get; set; }

	public bool IsMet( Pawn pawn )
	{
		switch ( Type )
		{
			case ConditionType.OnInteractWithEntity:
				{
					var entity = Entity.FindByName( InteractableName );

					if ( pawn.IsInteracting && pawn.InteractedEntity == entity )
						return true;

					return false;
				}
			case ConditionType.OnInteractWithType:
				{
					if ( pawn.IsInteracting && pawn.InteractedEntity?.GetType().Name == TypeName )
						return true;

					return false;
				}
		}
		return false;
	}
}
