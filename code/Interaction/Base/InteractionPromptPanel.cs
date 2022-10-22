namespace GvarJam.Interactions;

public class InteractionPromptPanel : WorldPanel
{
	public InteractionPromptPanel( InteractableEntity entity )
	{
		if ( entity is null )
			return;

		PanelBounds = new Rect( -250, -150, 500, 150 );

		AddClass( "world-panel" );
		StyleSheet.Load( "/UI/Hud.scss" );

		Add.Label( entity.Name );
	}
}
