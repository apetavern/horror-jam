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
	public override IReadOnlyDictionary<ItemType, int> RequiredItems => requiredItems;
	/// <summary>
	/// The required items to repair the generator.
	/// </summary>
	private readonly Dictionary<ItemType, int> requiredItems = new()
	{
		{ ItemType.GeneratorFuse, 1 }
	};

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		DisplayName = "Generator";

		SetModel( "models/scifilocker/scifilocker.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}

	/// <inheritdoc/>
	protected override void CreateActions()
	{
		base.CreateActions();

		Actions.Add( (0.6f, RepairGenerator) );
		Actions.Add( (0.5f, TurnOnGenerator) );
	}

	/// <inheritdoc/>
	protected override void OnUsed( Entity user )
	{
		base.OnUsed( user );

		if ( IsServer )
			LightManager.FlickerLighting( false, 0 );
	}

	/// <summary>
	/// The first animation, repairs the generator with a new fuse.
	/// </summary>
	/// <param name="user">The entity that is doing the interaction.</param>
	/// <param name="firstTime">Whether or not this has been invoked for the first time.</param>
	/// <param name="timeInAnim">The time in seconds that this animation has been going for.</param>
	/// <returns>Whether or not to skip this animation.</returns>
	private bool RepairGenerator( Entity user, bool firstTime, float timeInAnim )
	{
		return false;
	}

	/// <summary>
	/// The second animation, turns on the generator.
	/// </summary>
	/// <param name="user">The entity that is doing the interaction.</param>
	/// <param name="firstTime">Whether or not this has been invoked for the first time.</param>
	/// <param name="timeInAnim">The time in seconds that this animation has been going for.</param>
	/// <returns>Whether or not to skip this animation.</returns>
	private bool TurnOnGenerator( Entity user, bool firstTime, float timeInAnim )
	{
		return false;
	}
}

