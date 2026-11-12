namespace GvarJam.Objectives;

[AssetType( Name = "Objective", Extension = "objctv", Category = "Gvar Jam" )]
public sealed partial class ObjectiveResource : GameResource
{
	[Category( "Metadata" )]
	public string ObjectiveName { get; set; } = "My Cool Objective";

	[Category( "Metadata" )]
	public string Description { get; set; } = "Lorem Ipsum Dolor Sit Amet";

	[Title( "Conditions" ), Category( "Objective Start" )]
	public List<ObjectiveStartCondition> ObjectiveStartConditions { get; set; } = new();

	[Title( "Conditions" ), Category( "Objective End" )]
	public List<ObjectiveEndCondition> ObjectiveEndConditions { get; set; } = new();

	[Title( "Events" ), Category( "Objective Start" )]
	public List<ObjectiveEvent> ObjectiveStartEvents { get; set; } = new();

	[Title( "Events" ), Category( "Objective End" )]
	public List<ObjectiveEvent> ObjectiveEndEvents { get; set; } = new();
}
