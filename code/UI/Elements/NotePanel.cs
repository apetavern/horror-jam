namespace GvarJam.UI.Elements;

public sealed class NotePanel : Panel
{
	public static NotePanel Instance { get; private set; }
	private readonly Label _noteContents;

	public NotePanel()
	{
		Instance = this;
		var note = Add.Panel( "Note" );
		note.Add.Panel( "NoteHeader" );
		_noteContents = note.Add.Label( "", "NoteText" );
		note.Add.Panel( "NoteFooter" );
	}

	public void ShowNote( string noteContents )
	{
		_noteContents.Text = noteContents;
		SetClass( "display", true );
	}

	public void HideNote()
	{
		SetClass( "display", false );
	}
}
