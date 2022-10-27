namespace GvarJam.Interactions;

[Library( "ent_generatorfuse" )]
[HammerEntity]
[EditorModel( "models/items/battery/battery.vmdl" )]
public sealed partial class GeneratorFuse : DelayedUseItem
{
	/// <inheritdoc/>
	public override float TimeToUse => 2.6f;
	/// <inheritdoc/>
	protected override bool DeleteOnUse => true;

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		Name = "Battery";

		SetModel( "models/items/battery/battery.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
	}
}
