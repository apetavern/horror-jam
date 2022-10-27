using System.Numerics;

namespace GvarJam.HammerEntities;

[Category( "Environment" )]
[Library( "ent_generator" )]
[HammerEntity]
[EditorModel( "models/scifilocker/scifilocker.vmdl" )]
public sealed partial class Generator : DelayedUseItem
{
	/// <inheritdoc/>
	public override float TimeToUse => 1.1f;
	/// <inheritdoc/>
	protected override bool DeleteOnUse => false;
	/// <inheritdoc/>
	protected override bool ResetOnUse => false;

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		Name = "Generator";

		SetModel( "models/scifilocker/scifilocker.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}
}

