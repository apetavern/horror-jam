using System.Threading.Tasks;

namespace GvarJam.UI.Elements;

/// <summary>
/// The panel that shows credits to its creators.
/// </summary>
[UseTemplate]
public sealed partial class Credits : Panel
{
	/// <summary>
	/// The only instance of <see cref="Credits"/> in existance.
	/// </summary>
	public static Credits Instance { get; private set; }

	/// <summary>
	/// Initializes a default instance of <see cref="Credits"/>.
	/// </summary>
	public Credits()
	{
		Instance = this;

		AddClass( "hide" );
	}

	/// <summary>
	/// Shows the credits screen to the player.
	/// </summary>
	/// <returns>The asynchronous task that spawns from this invoke.</returns>
	private async Task Show()
	{
		await Task.DelaySeconds( 10f );
		Instance.RemoveClass( "hide" );
	}

	/// <summary>
	/// Starts the display task to the player.
	/// </summary>
	[ClientRpc]
	public static void DisplayCredits()
	{
		_ = Instance.Show();
	}
}
