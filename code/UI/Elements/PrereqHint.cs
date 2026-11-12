namespace GvarJam.UI.Elements;

public sealed class PrereqHint : Panel
{
	private readonly Label _label;

	public PrereqHint()
	{
		_label = Add.Label( "Hint" );
		AddClass( "auto-hide" );
	}

	public override void Tick()
	{
		SetClass( "visible", false );

		var player = Game.ActiveScene?.GetAllComponents<ColdOpenPlayer>().FirstOrDefault();
		if ( player is null ) return;

		var interactable = player.Interaction.FindInteractable();
		if ( interactable is null || !interactable.IsValid() ) return;
		if ( player.WorldPosition.Distance( interactable.WorldPosition ) > 100f ) return;
		if ( interactable.IsUsable( player ) ) return;
		if ( interactable.HasBeenUsed ) return;

		var items = interactable.RequiredItems;
		if ( items.Count == 0 ) return;

		_label.Text = $"Need {items.First().Key.GetItemName()}";
		SetClass( "visible", true );
	}
}
