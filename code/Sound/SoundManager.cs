namespace GvarJam.SoundManager;

public static class SoundManager
{
	[ConVar.Client]
	public static bool EnableAmbience { get; set; } = true;

	private static float AmbienceVolume = 0.25f;

	private static Sound CurrentAmbience { get; set; }

	private static string[] AmbientMusic = 
	{
		"slow_creep_ambient"
	};

	public static void BeginAmbiencePlayback()
	{
		if ( !EnableAmbience )
			return;

		var randomSound = Rand.FromArray( AmbientMusic );

		CurrentAmbience = Sound.FromScreen( randomSound );

		CurrentAmbience.SetVolume( AmbienceVolume );
	}

	[Event.Tick]
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
