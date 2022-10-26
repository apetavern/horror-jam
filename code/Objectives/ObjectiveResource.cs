namespace GvarJam.Objectives;

/// <summary>
/// Represents a resource for an objective in the game.
/// </summary>
[GameResource( "Objective", "objctv", "Gvar Jam Objective" )]
public sealed partial class ObjectiveResource : GameResource
{
	/// <summary>
	/// The name of the objective.
	/// </summary>
	[Category( "Metadata" )]
	public string ObjectiveName { get; set; } = "My Cool Objective";

	/// <summary>
	/// The description of the objective.
	/// </summary>
	[Category( "Metadata" )]
	public string Description { get; set; } = "Lorem Ipsum Dolor Sit Amet";

	/// <summary>
	/// The start conditions for the objective.
	/// </summary>
	[Title( "Conditions" ), Category( "Objective Start" )]
	public List<ObjectiveStartCondition> ObjectiveStartConditions { get; set; } = new();

	/// <summary>
	/// The end conditions for the objective.
	/// </summary>
	[Title( "Conditions" ), Category( "Objective End" )]
	public List<ObjectiveEndCondition> ObjectiveEndConditions { get; set; } = new();

	/// <summary>
	/// The events to invoke on pawns when the objective starts.
	/// </summary>
	[Title( "Events" ), Category( "Objective Start" )]
	public List<ObjectiveEvent> ObjectiveStartEvents { get; set; } = new();

	/// <summary>
	/// The events to invoke on pawns when the objective ends.
	/// </summary>
	[Title( "Events" ), Category( "Objective End" )]
	public List<ObjectiveEvent> ObjectiveEndEvents { get; set; } = new();

}
