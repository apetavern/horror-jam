﻿namespace GvarJam.SoundManager;

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
	private const float MinimumAmbientNoisedelay = 50;

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
	/// The time since the last ambient noise was played.
	/// </summary>
	private static TimeSince TimeSinceAmbientNoise;

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

		if ( TimeSinceAmbientNoise < Rand.Float( MinimumAmbientNoisedelay, MaximumAmbientNoiseDelay ) )
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

		TimeSinceAmbientNoise = 0;
		Sound.FromWorld( To.Everyone, Rand.FromArray( AmbientNoises ), resultPosition );
	}
}
