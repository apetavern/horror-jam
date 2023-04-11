using GvarJam.UI.Elements;

namespace GvarJam.Interactions;

/// <summary>
/// Represents an item that takes time to use.
/// </summary>
[Library( "ent_note" )]
[HammerEntity]
[EditorModel( "models/clipboard/clipboard_01a.vmdl" )]
public partial class Note : InstantUseItem
{
	/// <inheritdoc/>
	protected override bool DeleteOnUse => false;

	/// <summary>
	/// The contents of this note.
	/// </summary>
	[Net, Property]
	public string NoteContents { get; set; } = "this is a note";

	/// <summary>
	/// The time since the note was last used.
	/// </summary>
	private TimeSince TimeSinceUsed { get; set; }

	/// <summary>
	/// The minimum amount of time before you can use the note again.
	/// </summary>
	private int TimeBetweenUses { get; set; } = 1;

	/// <summary>
	/// Whether or not the note is currently open.
	/// </summary>
	private bool IsNoteOpen { get; set; }

	/// <inheritdoc/>
	public override void Spawn()
	{
		SetModel( "models/clipboard/clipboard_01a.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

		DisplayName = "Note";
	}

	/// <inheritdoc/>
	protected override void OnUsed( Entity user )
	{
		base.OnUsed( user );

		// This is dirty, but it saves us having to check whether or not the note is still open or not.
		if ( TimeSinceUsed < TimeBetweenUses )
			return;

		if ( user is not Pawn player )
			return;

		TimeSinceUsed = 0;

		if ( !IsNoteOpen )
		{
			IsNoteOpen = true;

			if ( Game.IsClient )
			{
				NotePanel.Instance.ShowNote( NoteContents );
				Sound.FromScreen( "note_open" );
			}

			if ( Game.IsServer )
				player.BlockMovement = true;	
		}
		else
		{
			IsNoteOpen = false;

			if ( Game.IsClient )
				NotePanel.Instance.HideNote();
			

			if ( Game.IsServer )
				player.BlockMovement = false;
		}
	}
}

