namespace GvarJam.Managers;

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
	private const float AmbienceVolume = 0.25f;

	/// <summary>
	/// The minimum time in seconds before an ambient noise is played.
	/// </summary>
	private const float MinimumAmbientNoiseDelay = 50;

	/// <summary>
	/// The maximum time in seconds before an ambient noise is played.
	/// </summary>
	private const float MaximumAmbientNoiseDelay = 80;

	/// <summary>
	/// The minimum range in source units to place the ambient noise from a random pawn.
	/// </summary>
	private const float MinimumAmbientNoiseRange = 100;

	/// <summary>
	/// The maximum range in source units to place the ambient noise from a random pawn.
	/// </summary>
	private const float MaximumAmbientNoiseRange = 300;

	/// <summary>
	/// The current ambience sound playing.
	/// </summary>
	private static Sound CurrentMusicAmbience { get; set; }

	/// <summary>
	/// The current chase sound playing.
	/// </summary>
	private static Sound CurrentChaseMusic { get; set; }

	/// <summary>
	/// The list of ambient music to play.
	/// </summary>
	private static string[] AmbientMusic =
	{
		"slow_creep_ambient",
		"EerieSoundsToMakeYouShitYourself"
	};

	/// <summary>
	/// The list of ambient noises to play throughout the map.
	/// </summary>
	private static string[] AmbientNoises =
	{
		"spooky_scuttle",
		"spooky_scuttle_long"
	};

	/// <summary>
	/// The list of sounds to play when being chased by a monster.
	/// </summary>
	private static string[] ChaseSounds =
	{
		"chase"
	};

	/// <summary>
	/// The time until the next ambient noise needs to be played.
	/// </summary>
	private static TimeUntil TimeUntilNextAmbientNoise = Rand.Float( MinimumAmbientNoiseDelay, MaximumAmbientNoiseDelay );

	/// <summary>
	/// Picks a random ambient sound and starts playing it.
	/// </summary>
	public static void BeginAmbiencePlayback()
	{
		if ( !EnableAmbience )
			return;

		var randomSound = Rand.FromArray( AmbientMusic );
		CurrentMusicAmbience = Sound.FromScreen( randomSound );
		CurrentMusicAmbience.SetVolume( AmbienceVolume );
	}

	/// <summary>
	/// Toggle the chase sound playback.
	/// </summary>
	/// <param name="shouldPlay"></param>
	public static void ShouldPlayChaseSounds( bool shouldPlay )
	{
		if ( !shouldPlay )
		{
			CurrentChaseMusic.Stop();
			return;
		}

		if ( !CurrentChaseMusic.Finished )
			return;

		var randomSound = Rand.FromArray( ChaseSounds );
		CurrentChaseMusic = Sound.FromScreen( randomSound );
	}

	/// <summary>
	/// Sets the volume for the chase music.
	/// </summary>
	/// <param name="soundVolume"></param>
	public static void SetChaseSoundVolume( float soundVolume )
	{
		CurrentChaseMusic.SetVolume( soundVolume );
	}

	/// <summary>
	/// Ticks the ambient sound. If it has finished, start a new one.
	/// </summary>
	[Event.Tick.Server]
	public static void MonitorPlayback()
	{
		if ( !EnableAmbience )
			CurrentMusicAmbience.Stop();
		else
		{
			if ( CurrentMusicAmbience.Finished )
				BeginAmbiencePlayback();

			// TODO: Scale this based on some sort of danger variable?
			CurrentMusicAmbience.SetVolume( AmbienceVolume );
		}

		if ( TimeUntilNextAmbientNoise > 0 )
			return;

		var chosenPawn = Entity.All.OfType<Pawn>().OrderBy( x => Guid.NewGuid() ).FirstOrDefault();
		if ( chosenPawn is null )
			return;

		Vector3 resultPosition = Vector3.Zero;
		var positionX = Rand.Int( 0, 1 ) == 1
			? Rand.Float( chosenPawn.Position.x + MinimumAmbientNoiseRange, chosenPawn.Position.x + MaximumAmbientNoiseRange )
			: Rand.Float( chosenPawn.Position.x - MaximumAmbientNoiseRange, chosenPawn.Position.x - MinimumAmbientNoiseRange );
		var positionY = Rand.Int( 0, 1 ) == 1
			? Rand.Float( chosenPawn.Position.y + MinimumAmbientNoiseRange, chosenPawn.Position.y + MaximumAmbientNoiseRange )
			: Rand.Float( chosenPawn.Position.y - MaximumAmbientNoiseRange, chosenPawn.Position.y - MinimumAmbientNoiseRange );

		resultPosition = new Vector3( positionX, positionY, chosenPawn.Position.z );
		var tr = Trace.Sphere( 100, resultPosition, resultPosition ).Run();

		if ( !tr.Hit )
			return;

		TimeUntilNextAmbientNoise = Rand.Float( MinimumAmbientNoiseDelay, MaximumAmbientNoiseDelay );
		Sound.FromWorld( To.Everyone, Rand.FromArray( AmbientNoises ), resultPosition );
	}

	/// <summary>
	/// Plays a sound that the monster can hear.
	/// </summary>
	public static void PlayMonsterSound( string soundName, Vector3 position, float volume = 1.0f )
	{
		ServerFootstep( soundName, position, volume );
	}

	//
	// sorry :(
	// monster only exists on server but footstep anim events are
	// only fired on client..
	//
	[ConCmd.Server]
	public static void ServerFootstep( string soundName, Vector3 position, float volume = 1.0f )
	{
		Event.Run( Events.Monster.Name, position, volume );

		Sound.FromWorld( To.Everyone, soundName, position ).SetVolume( volume );
	}
}
