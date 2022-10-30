using System.Threading.Tasks;

namespace GvarJam.UI.Elements;

[UseTemplate]
public sealed partial class Credits : Panel
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

	private async Task Show()
	{
		await Task.DelaySeconds( 10f );
		Log.Trace( "Displaying credits" );
		Instance.RemoveClass( "hide" );
	}

	[ClientRpc]
	public static void DisplayCredits()
	{
		_ = Instance.Show();
	}
}
