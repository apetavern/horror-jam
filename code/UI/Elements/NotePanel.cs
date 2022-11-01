namespace GvarJam.UI.Elements;

/// <summary>
/// Show notes
/// </summary>
public sealed class NotePanel : Panel
{
	/// <summary>
	/// The only instance of <see cref="NotePanel"/> in existance.
	/// </summary>
	public static NotePanel Instance { get; set; }

	/// <summary>
	/// The panel that holds the contents of the note.
	/// </summary>
	private Label NoteContents { get; set; }

	/// <summary>
	/// Initializes a default instance of <see cref="NotePanel"/>.
	/// </summary>
	public NotePanel()
	{
		Instance = this;

		var note = Add.Panel( "Note" );
		note.Add.Panel( "NoteHeader" );
		NoteContents = note.Add.Label( "", "NoteText" );
		note.Add.Panel( "NoteFooter" );
	}

	/// <summary>
	/// Shows a note with the provided text.
	/// </summary>
	/// <param name="noteContents">The text to display on the note.</param>
	public void ShowNote( string noteContents )
	{
		NoteContents.Text = noteContents;
		SetClass( "display", true );
	}

	/// <summary>
	/// Hides any shown note.
	/// </summary>
	public void HideNote()
	{
		SetClass( "display", false );
	}
}
