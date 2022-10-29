namespace GvarJam.Events;

public static class Monster
{
	public const string Name = "GvarJam.Monster.Sound";

	public class SoundAttribute : EventAttribute
	{
		public SoundAttribute() : base( Name ) { }
	}
}
