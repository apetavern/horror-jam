namespace GvarJam.UI.Elements;

[UseTemplate]
public sealed class Credits : Panel
{
	public Label GameOutcomeLabel { get; set; }
	public Label ThanksLabel { get; set; }
	public Label CreditsLabel { get; set; }

	public Credits()
	{
		AddClass( "hide" );
	}

	public override void Tick()
	{
		if ( ObjectiveSystem.Current is null )
			return;

		if ( ObjectiveSystem.Current.ActiveObjectives.Count == 0 
			&& ObjectiveSystem.Current.PendingObjectives.Count == 0 )
		{
			RemoveClass( "hide" );
		} 
		else
		{
			AddClass( "hide" );
		}
	}

}
