namespace GvarJam.Objectives;

public struct ObjectiveEndCondition
{
	public enum ConditionType
	{
		OnInteract,
		OnTrigger,
		OnTimer
	}

	public ConditionType Type { get; set; }

	[ShowIf( nameof( Type ), ConditionType.OnTrigger )]
	public string TriggerName { get; set; }

	[ShowIf( nameof( Type ), ConditionType.OnInteract )]
	public string InteractableName { get; set; }

	[ShowIf( nameof( Type ), ConditionType.OnTimer )]
	public float TimeDelay { get; set; }

	public bool IsMet( Pawn pawn )
	{
		switch ( Type )
		{
			case ConditionType.OnInteract:
				{
					var entity = Entity.FindByName( InteractableName );

					if ( pawn.IsInteracting && pawn.InteractedEntity == entity )
						return true;

					return false;
				}
		}
		return false;
	}
}
