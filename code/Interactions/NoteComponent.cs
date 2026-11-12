namespace GvarJam.Interactions;

public sealed class NoteComponent : InstantUseComponent
{
	public override string LegacyTypeName => "Note";
	protected override bool DeleteOnUse => false;

	[Property, Sync] public string NoteContents { get; set; } = "this is a note";

	private TimeSince _timeSinceUsed = 10f;
	private const float TimeBetweenUses = 1f;
	private bool _isNoteOpen;

	protected override void OnStart()
	{
		base.OnStart();
		DisplayName = "Note";

		var model = Model.Load( "models/clipboard/clipboard_01a.vmdl" );
		var mr = GameObject.GetOrAddComponent<ModelRenderer>();
		mr.Model = model;

		var col = GameObject.GetOrAddComponent<ModelCollider>();
		col.Model = model;
	}

	protected override void OnUsed( ColdOpenPlayer user )
	{
		base.OnUsed( user );

		if ( _timeSinceUsed < TimeBetweenUses )
			return;

		_timeSinceUsed = 0;

		if ( !_isNoteOpen )
		{
			_isNoteOpen = true;
			NotePanel.Instance?.ShowNote( NoteContents );
			AudioDirector.Play2D( "note_open" );
			user.BlockMovement = true;
		}
		else
		{
			_isNoteOpen = false;
			NotePanel.Instance?.HideNote();
			user.BlockMovement = false;
		}
	}
}
