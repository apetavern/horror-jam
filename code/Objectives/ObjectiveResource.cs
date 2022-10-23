namespace GvarJam.Objectives;

[GameResource( "Objective", "objctv", "Gvar Jam Objective" )]
public partial class ObjectiveResource : GameResource
{
	[Title( "Start Conditions" )]
	public List<ObjectiveStartCondition> ObjectiveStartConditions { get; set; }

	[Title( "End Conditions" )]
	public List<ObjectiveEndCondition> ObjectiveEndConditions { get; set; }

	public string Id { get; set; } = "my_objective";
	public string Name { get; set; } = "My Cool Objective";
	public string Description { get; set; } = "Lorem Ipsum Dolor Sit Amet";
}
