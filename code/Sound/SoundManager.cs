namespace GvarJam.SoundManager;

/// <summary>
/// The sound manager for the game.
/// </summary>
public static class SoundManager
{
	/// <summary>
	/// Whether or not to have ambience playing in the background.
	/// </summary>
	[ConVar.Client]
	public static bool EnableAmbience { get; set; } = true;

	/// <summary>
	/// The volume of the ambience.
	/// </summary>
	private static float AmbienceVolume = 0.25f;

	/// <summary>
	/// The current ambience sound playing.
	/// </summary>
	private static Sound CurrentAmbience { get; set; }

	/// <summary>
	/// The list of ambient music to play.
	/// </summary>
	private static string[] AmbientMusic = 
	{
		"slow_creep_ambient",
		"EerieSoundsToMakeYouShitYourself"
	};

	/// <summary>
	/// Picks a random ambient sound and starts playing it.
	/// </summary>
	public static void BeginAmbiencePlayback()
	{
		if ( !EnableAmbience )
			return;

		var randomSound = Rand.FromArray( AmbientMusic );
		CurrentAmbience = Sound.FromScreen( randomSound );
		CurrentAmbience.SetVolume( AmbienceVolume );
	}

	/// <summary>
	/// Ticks the ambient sound. If it has finished, start a new one.
	/// </summary>
	[Event.Tick.Server]
	public static void MonitorPlayback()
	{
		if ( !EnableAmbience )
			CurrentAmbience.Stop();
		else
		{
			if ( CurrentAmbience.Finished )
				BeginAmbiencePlayback();

			// TODO: Scale this based on some sort of danger variable?
			CurrentAmbience.SetVolume( AmbienceVolume );
		}
	}
}
