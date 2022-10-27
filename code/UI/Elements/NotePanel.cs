using System.ComponentModel;

namespace GvarJam.UI.Elements;

/// <summary>
/// Show notes
/// </summary>
public sealed class NotePanel : Panel
{
	public static NotePanel Instance { get; set; }

	private Label NoteContents { get; set; }

	public NotePanel()
	{
		Instance = this;

		var note = Add.Panel( "Note" );
		NoteContents = note.Add.Label( "", "NoteText" );
	}

	public void ShowNote( string noteContents )
	{
		NoteContents.Text = noteContents;

		SetClass( "display", true );
	}

	public void HideNote()
	{
		SetClass( "display", false );
	}
}
