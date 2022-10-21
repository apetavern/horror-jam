namespace GvarJam.Interactions;

public class InteractionPromptPanel : WorldPanel
{
	public InteractionPromptPanel( InteractableEntity entity )
	{
		if ( entity is null )
			return;

		PanelBounds = new Rect( -250, -150, 500, 150 );

		Add.Label( entity.Name );

		Style.FontColor = Color.White;
		Style.TextStrokeColor = Color.Black;
		Style.TextStrokeWidth = 20f;
		Style.FontSize = 24;
		Style.JustifyContent = Justify.Center;
		Style.AlignItems = Align.Center;
		Style.FlexDirection = FlexDirection.Column;
	}
}
