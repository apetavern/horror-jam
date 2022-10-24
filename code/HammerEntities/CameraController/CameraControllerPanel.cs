namespace GvarJam.Interactions;

/// <summary>
/// The UI world panel for the Camera Controller.
/// </summary>
public sealed class CameraControllerPanel : WorldPanel
{
	private Label? label;

	public CameraControllerPanel( CameraController controller )
	{
		if ( controller is null )
			return;

		PanelBounds = new Rect( -250, -75, 500, 75 );

		Style.FontColor = Color.White;
		Style.Opacity = 0.5f;
		Style.FontSize = 26;
		Style.JustifyContent = Justify.FlexEnd;

		label = Add.Label( controller.TargetCamera?.ZoneName.ToUpper() );
	}

	public void UpdateName(string name) 
	{
		if ( label is null )
			return;

		label.Text = name;
	}
}
