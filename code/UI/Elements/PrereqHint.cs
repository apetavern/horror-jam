namespace GvarJam.UI.Elements;

/// <summary>
/// A hint for showing pre-requisites of interactables.
/// </summary>
public sealed class PrereqHint : Panel
{
	/// <summary>
	/// The label explaining what item is needed.
	/// </summary>
	private readonly Label label;

	public PrereqHint()
	{
		StyleSheet.Load( "/UI/Hud.scss" );

		label = Add.Label( "Hint" );
		AddClass( "auto-hide" );
	}

	public override void Tick()
	{
		if ( Local.Pawn is not Pawn pawn )
			return;

		SetClass( "visible", false );

		var entity = pawn.FindInteractableEntity();
		if ( entity is null || !entity.IsValid || entity is not InteractableEntity interactable )
			return;

		if ( interactable.IsUsable( pawn ) )
			return;

		var items = interactable.RequiredItems;

		if ( items.Count == 0 )
			return;

		var friendlyName = GetFriendlyName( items.First().Key.ToString() );

		label.Text = $"Need {friendlyName} to open this.";
		SetClass( "visible", true );
	}

	private string GetFriendlyName( string name )
	{
		string res = "";

		for ( int i = 0; i < name.Length; i++ )
		{
			char c = name[i];

			if ( char.IsUpper( c ) && i != 0 )
			{
				res += " ";
			}

			res += c;
		}

		return res;
	}
}
