namespace GvarJam.Interactions;

/// <summary>
/// Represents an item that takes time to use.
/// </summary>
[Library( "ent_console" )]
[HammerEntity]
[EditorModel( "models/ctrlpanel/ctrlpanel_01a.vmdl" )]

public partial class Console : InstantUseItem
{
	protected override bool DeleteOnUse => false;

	private TimeSince TimeSinceUsed { get; set; }

	private int TimeBetweenUses { get; set; } = 1;

	public override void Spawn()
	{
		SetModel( "models/ctrlpanel/ctrlpanel_01a.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static );

		DisplayName = "Control panel";
	}

	protected override void OnUsed( Entity user )
	{
		base.OnUsed( user );

		// This is dirty, but it saves us having to check whether or not the note is still open or not.
		if ( TimeSinceUsed < TimeBetweenUses )
			return;

		TimeSinceUsed = 0;

		if ( Host.IsServer )
			return;

	}
}

