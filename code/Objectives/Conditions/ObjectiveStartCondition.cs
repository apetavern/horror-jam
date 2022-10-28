﻿using System.Diagnostics.CodeAnalysis;

namespace GvarJam.Objectives;

/// <summary>
/// Contains information for a start condition in an objective.
/// </summary>
public struct ObjectiveStartCondition
{
	/// <summary>
	/// The type of start condition.
	/// </summary>
	public enum ConditionType
	{
		/// <summary>
		/// Utilizing the interaction system on an entity.
		/// </summary>
		InteractWithEntity,
		/// <summary>
		/// A previously completed objective.
		/// </summary>
		PreviousObjectiveComplete,
		/// <summary>
		/// Entering a zone of the map.
		/// </summary>
		PlayerEnteredTrigger
	}

	/// <summary>
	/// The start condition.
	/// </summary>
	public ConditionType Type { get; set; }

	/// <summary>
	/// The name of the entity to interact with to complete.
	/// </summary>
	[ShowIf( nameof( Type ), ConditionType.InteractWithEntity )]
	public string InteractableName { get; set; }

	/// <summary>
	/// The name of the objective to be completed.
	/// </summary>
	[ShowIf( nameof( Type ), ConditionType.PreviousObjectiveComplete ), Icon( "done" ), ResourceType( "objctv" )]
	public string ObjectiveName { get; set; }

	/// <summary>
	/// The name of the trigger to enter to start.
	/// </summary>
	[ShowIf( nameof( Type ), ConditionType.PlayerEnteredTrigger ), Icon( "view_in_ar" )]
	public string TriggerName { get; set; }

	/// <summary>
	/// Determines whether or not the pawn has met the start conditions.
	/// </summary>
	/// <param name="pawn">The pawn to check the conditions on.</param>
	/// <returns>Whether or not the conditions were met.</returns>
	public bool IsMet( Pawn pawn )
	{
		switch ( Type )
		{
			case ConditionType.InteractWithEntity:
				{
					{
						var interactableName = InteractableName;
						var entity = Entity.All.FirstOrDefault( x => x.Name.ToLower().Contains( interactableName.ToLower() ));

						if ( pawn.IsInteracting && pawn.InteractedEntity == entity )
							return true;

						return false;
					}
				}
			case ConditionType.PlayerEnteredTrigger:
				{
					var triggerName = TriggerName;
					var entity = Entity.All.OfType<ObjectiveTrigger>().FirstOrDefault( x => x.Name.Contains( triggerName ));

					if ( entity is not ObjectiveTrigger trigger )
						return false;

					return trigger.TouchingEntities.Contains( pawn );
				}
			case ConditionType.PreviousObjectiveComplete:
				{
					var objectiveSystem = ObjectiveSystem.Current;
					var objectiveName = ObjectiveName;

					var activeObjMatch = objectiveSystem.ActiveObjectives.Any( x => x.Resource.ResourcePath == objectiveName );
					var pendingObjMatch = objectiveSystem.PendingObjectives.Any( x => x.Resource.ResourcePath == objectiveName );

					return !activeObjMatch && !pendingObjMatch;
				}
		}

		return false;
	}
}
