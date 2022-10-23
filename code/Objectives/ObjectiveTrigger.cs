namespace GvarJam.Objectives;

[AutoApplyMaterial( "textures/tools/toolsobjective.vmat" )]
[Solid, VisGroup( VisGroup.Trigger ), HideProperty( "enable_shadows" )]
[Title( "Objective Trigger" ), Icon( "select_all" )]
[HammerEntity]
public class ObjectiveTrigger : BaseTrigger
{
}
