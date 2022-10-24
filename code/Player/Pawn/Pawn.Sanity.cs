namespace GvarJam.Player;

public partial class Pawn
{
	[Net, Predicted]
	public float SanityLevel { get; set; } = 100;

	public void SimulateSanity()
	{

	}
}
