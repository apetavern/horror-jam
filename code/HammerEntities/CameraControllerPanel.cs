namespace GvarJam.HammerEntities;

public sealed class CameraControllerPanelComponent : PanelComponent
{
	private Label _label;
	private string _zoneName = "";

	protected override void OnTreeFirstBuilt()
	{
		base.OnTreeFirstBuilt();
		Panel.Style.FontColor = Color.White;
		Panel.Style.Opacity = 0.5f;
		Panel.Style.FontSize = 26;
		Panel.Style.JustifyContent = Justify.FlexEnd;
		_label = Panel.Add.Label( _zoneName );
	}

	public void UpdateName( string name )
	{
		_zoneName = name;
		if ( _label is not null )
			_label.Text = name;
	}
}
