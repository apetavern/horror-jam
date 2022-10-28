namespace GvarJam.Interactions;

/// <summary>
/// Represents an item that takes time to use.
/// </summary>
[Library( "ent_note" )]
[HammerEntity]
[EditorModel( "models/clipboard/clipboard_01a.vmdl" )]

public partial class Note : InstantUseItem
{
	protected override bool DeleteOnUse => false;

	[Net, Property]
	public string NoteContents { get; set; } = "this is a note";

	private TimeSince TimeSinceUsed { get; set; }

	private int TimeBetweenUses { get; set; } = 1;

	private bool IsNoteOpen { get; set; }

	public override void Spawn()
	{
		SetModel( "models/clipboard/clipboard_01a.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

		Name = "Note";
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

		if ( !IsNoteOpen )
		{
			Hud.Instance?.ShowNote( NoteContents );
			IsNoteOpen = true;
			Sound.FromScreen( "note_open" );
		}
		else
		{
			Hud.Instance?.HideNote();
			IsNoteOpen = false;
		}
	}
}

