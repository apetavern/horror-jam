namespace GvarJam.Interactions;

/// <summary>
/// Represents an item that takes time to use.
/// </summary>
[Library( "ent_note" )]
[HammerEntity]
[EditorModel( "models/items/battery/battery.vmdl" )]

public partial class Note : InstantUseItem
{
	[Net, Property]
	public string NoteContents { get; set; }

	protected override void OnUsed( Entity user )
	{
		base.OnUsed( user );

		Log.Info( NoteContents );
	}
}

