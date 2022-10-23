namespace GvarJam.Objectives;

[GameResource( "Objective", "objctv", "Gvar Jam Objective" )]
public partial class ObjectiveResource : GameResource
{
	[Title( "Conditions" ), Category( "Objective Start" )]
	public List<ObjectiveStartCondition> ObjectiveStartConditions { get; set; }

	[Title( "Conditions" ), Category( "Objective End" )]
	public List<ObjectiveEndCondition> ObjectiveEndConditions { get; set; }

	[Title( "Events" ), Category( "Objective Start" )]
	public List<ObjectiveEvent> ObjectiveStartEvents { get; set; }

	[Title( "Events" ), Category( "Objective End" )]
	public List<ObjectiveEvent> ObjectiveEndEvents { get; set; }

	public string Id { get; set; } = "my_objective";
	public string Name { get; set; } = "My Cool Objective";
	public string Description { get; set; } = "Lorem Ipsum Dolor Sit Amet";
}
