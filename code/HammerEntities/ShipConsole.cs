namespace GvarJam.HammerEntities;

[Library( "ent_console" )]
[HammerEntity]
[EditorModel( "models/ctrlpanel/ctrlpanel_01a.vmdl" )]
public partial class ShipConsole : DelayedUseItem
{
	/// <inheritdoc/>
	public override float TimeToUse => 1.0f;
	/// <inheritdoc/>
	protected override bool DeleteOnUse => false;

	/// <inheritdoc/>
	public override IReadOnlyDictionary<ItemType, int> RequiredItems => requiredItems;
	/// <summary>
	/// The required items to use the console.
	/// </summary>
	private readonly Dictionary<ItemType, int> requiredItems = new()
	{
		{ ItemType.KeycardLvl3, 1 }
	};

	[Net, Predicted]
	private bool Used { get; set; }

	/// <inheritdoc/>
	public override void Spawn()
	{
		SetModel( "models/ctrlpanel/ctrlpanel_01a.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static );

		DisplayName = "Control panel";
	}

	/// <inheritdoc/>
	protected override void CreateActions()
	{
		base.CreateActions();

		Actions.Add( (1.0f, UseConsole) );
	}

	/// <inheritdoc/>
	protected override void OnUsed( Entity user )
	{
		base.OnUsed( user );

		Used = true;
	}

	/// <inheritdoc/>
	public override bool IsUsable( Entity user )
	{
		return !Used && base.IsUsable( user );
	}

	/// <summary>
	/// The first animation, use the console.
	/// </summary>
	/// <param name="user">The entity that is doing the interaction.</param>
	/// <param name="firstTime">Whether or not this has been invoked for the first time.</param>
	/// <param name="timeInAnim">The time in seconds that this animation has been going for.</param>
	/// <returns>Whether or not to skip this animation.</returns>
	private bool UseConsole( Entity user, bool firstTime, float timeInAnim )
	{
		return false;
	}
}

