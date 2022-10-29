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

	private TimeSince hoveringInteractable = 0f;

	public PrereqHint()
	{
		StyleSheet.Load( "/UI/Hud.scss" );

		label = Add.Label( "Hint" );
		AddClass( "hide" );
	}

	public override void Tick()
	{
		if ( Local.Pawn is not Pawn pawn )
			return;

		// should probably fade out
		if ( hoveringInteractable > 3f )
			AddClass( "hide" );

		var entity = pawn.FindInteractableEntity();
		if ( entity is null || !entity.IsValid || entity is not InteractableEntity interactable )
			return;

		if ( interactable.IsUsable( pawn ) )
			return;

		var items = interactable.RequiredItems;
		label.Text = $"Need {items.First()} to open this.";
		Log.Info( label.Text );

		RemoveClass( "hide" );
		hoveringInteractable = 0f;
	}
}
