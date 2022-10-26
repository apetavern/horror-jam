namespace GvarJam.Interactions;

/// <summary>
/// The world panel UI for the Camera Controller.
/// </summary>
public sealed class CameraControllerPanel : WorldPanel
{
	/// <summary>
	/// The label that has the name of the zone the current camera is viewing.
	/// </summary>
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

	/// <summary>
	/// Updates the name label on the controller.
	/// </summary>
	/// <param name="name">The new name to put on the label.</param>
	public void UpdateName(string name) 
	{
		if ( label is null )
			return;

		label.Text = name;
	}
}
