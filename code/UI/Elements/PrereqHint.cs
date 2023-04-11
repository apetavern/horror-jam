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

	/// <summary>
	/// Initializes a default instance of <see cref="PrereqHint"/>.
	/// </summary>
	public PrereqHint()
	{
		StyleSheet.Load( "/UI/Hud.scss" );

		label = Add.Label( "Hint" );
		AddClass( "auto-hide" );
	}

	/// <inheritdoc/>
	public override void Tick()
	{
		if ( Game.LocalPawn is not Pawn pawn )
			return;

		SetClass( "visible", false );

		var entity = pawn.FindInteractableEntity();
		if ( entity is null || !entity.IsValid || entity is not InteractableEntity interactable )
			return;

		if ( pawn.Position.Distance( interactable.Position ) > 100 )
			return;

		if ( interactable.IsUsable( pawn ) )
			return;

		if ( interactable.HasBeenUsed )
			return;

		var items = interactable.RequiredItems;
		if ( items.Count == 0 )
			return;

		label.Text = $"Need {items.First().Key.GetItemName()}";
		SetClass( "visible", true );
	}
}
