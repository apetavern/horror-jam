namespace GvarJam.Objectives;

public struct ObjectiveStartCondition
{
	public enum ConditionType
	{
		PreviousObjectiveComplete,
		PlayerEnteredTrigger
	}

	public ConditionType Type { get; set; }

	[ShowIf( nameof( Type ), ConditionType.PreviousObjectiveComplete ), Icon( "done" )]
	public string ObjectiveName { get; set; }

	[ShowIf( nameof( Type ), ConditionType.PlayerEnteredTrigger ), Icon( "view_in_ar" )]
	public string TriggerName { get; set; }

	public bool IsMet( Pawn pawn )
	{
		switch ( Type )
		{
			case ConditionType.PlayerEnteredTrigger:
				{
					var entity = Entity.FindByName( TriggerName );
					if ( entity is not ObjectiveTrigger trigger )
						return false;

					if ( trigger.TouchingEntities.Contains( pawn ) )
						return true;
				}
				break;
		}

		return false;
	}
}
