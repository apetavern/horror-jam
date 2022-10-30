namespace GvarJam.UI.Elements;

[UseTemplate]
public sealed class Credits : Panel
{
	public static Credits Instance { get; private set; }

	public Label GameOutcomeLabel { get; set; }
	public Label ThanksLabel { get; set; }
	public Label CreditsLabel { get; set; }

	public Credits()
	{
		Instance = this;

		AddClass( "hide" );
	}

	[ClientRpc]
	public static void DisplayCredits()
	{
		Instance.RemoveClass( "hide" );
	}
}
