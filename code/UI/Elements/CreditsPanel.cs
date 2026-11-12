namespace GvarJam.UI.Elements;

public sealed class CreditsPanel : Panel
{
	public static CreditsPanel Instance { get; private set; }

	public CreditsPanel()
	{
		Instance = this;
		AddClass( "hide" );
		AddClass( "credits-panel" );

		var container = Add.Panel( "text-container" );
		container.Add.Label( "Game Over", "outcome" );
		container.Add.Label( "Thank you for playing Cold Open!", "thanks" );
		container.Add.Panel( "divider" );

		var credits = container.Add.Panel( "credits" );
		foreach ( var line in new[]
		{
			"APE TAVERN",
			"Trundler - Sound Design, Programming",
			"Alex - UI Design, Programming",
			"Matt944 - Programming",
			"ShadowBrain - 3D Art, Animation, Level Design, Programming",
			"Pukes - 3D Art, Animation",
			"JammieDodger - 3D Art, Level Design",
			"Gooman - Programming",
			"Liron - 3D Art, 2D Art, Concept Art",
			"",
			"SPECIAL THANKS TO",
			"Yuri the Yeti - Spiritual Guidance",
			"matek - Painday Fixer"
		} )
		{
			credits.Add.Label( line );
		}
	}

	public static void DisplayCredits()
	{
		_ = Instance?.ShowAsync();
	}

	private async Task ShowAsync()
	{
		await GameTask.DelaySeconds( 10f );
		if ( IsValid )
			RemoveClass( "hide" );
	}
}
