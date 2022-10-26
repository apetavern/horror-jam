namespace GvarJam.Objectives;

/// <summary>
/// Represents a trigger zone for objective completion.
/// </summary>
[AutoApplyMaterial( "textures/tools/toolsobjective.vmat" )]
[Solid, VisGroup( VisGroup.Trigger ), HideProperty( "enable_shadows" )]
[Title( "Objective Trigger" ), Icon( "select_all" )]
[HammerEntity]
public sealed class ObjectiveTrigger : BaseTrigger
{
}
