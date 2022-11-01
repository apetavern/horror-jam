namespace GvarJam.Events;

/// <summary>
/// A class of events relating to the monster.
/// </summary>
public static class Monster
{
	/// <summary>
	/// The event for when a monster makes a sound.
	/// </summary>
	public const string Name = "GvarJam.Monster.Sound";

	/// <summary>
	/// The event for when a monster makes a sound.
	/// </summary>
	public class SoundAttribute : EventAttribute
	{
		/// <summary>
		/// Initializes a default instance of <see cref="SoundAttribute"/>.
		/// </summary>
		public SoundAttribute() : base( Name ) { }
	}
}
