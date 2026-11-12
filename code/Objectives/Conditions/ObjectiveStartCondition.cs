namespace GvarJam.Objectives;

public struct ObjectiveStartCondition
{
	public enum ConditionType
	{
		InteractWithEntity,
		PreviousObjectiveComplete,
		PlayerEnteredTrigger
	}

	public ConditionType Type { get; set; }

	[ShowIf( nameof( Type ), ConditionType.InteractWithEntity )]
	public string InteractableName { get; set; }

	[ShowIf( nameof( Type ), ConditionType.PreviousObjectiveComplete ), ResourceType( "objctv" )]
	public string ObjectiveName { get; set; }

	[ShowIf( nameof( Type ), ConditionType.PlayerEnteredTrigger )]
	public string TriggerName { get; set; }

	public bool IsMet( ColdOpenPlayer player )
	{
		var scene = player.Scene;
		switch ( Type )
		{
			case ConditionType.InteractWithEntity:
			{
				if ( string.IsNullOrEmpty( InteractableName ) || string.IsNullOrEmpty( player.LastInteractedEntityName ) )
					return false;

				var name = InteractableName;
				var go = scene.GetAllObjects( true ).FirstOrDefault( x => x.Name.Contains( name, StringComparison.OrdinalIgnoreCase ) );
				if ( go is null )
					return false;

				return string.Equals( player.LastInteractedEntityName, go.Name, StringComparison.OrdinalIgnoreCase );
			}
			case ConditionType.PlayerEnteredTrigger:
			{
				if ( string.IsNullOrEmpty( TriggerName ) )
					return false;

				var triggerName = TriggerName;
				foreach ( var trigger in scene.GetAllComponents<ObjectiveTriggerComponent>() )
				{
					if ( trigger.GameObject.Name.Contains( triggerName, StringComparison.OrdinalIgnoreCase ) && trigger.IsTouching( player ) )
						return true;
				}
				return false;
			}
			case ConditionType.PreviousObjectiveComplete:
			{
				var sys = scene.GetSystem<ObjectiveSystem>();
				if ( sys is null || string.IsNullOrEmpty( ObjectiveName ) )
					return false;

				var objName = ObjectiveName;
				bool inActive = sys.ActiveObjectives.Any( x => string.Equals( x.Resource.ResourcePath, objName, StringComparison.OrdinalIgnoreCase ) );
				bool inPending = sys.PendingObjectives.Any( x => string.Equals( x.Resource.ResourcePath, objName, StringComparison.OrdinalIgnoreCase ) );
				return !inActive && !inPending;
			}
		}

		return false;
	}
}
