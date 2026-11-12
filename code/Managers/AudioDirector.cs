using GvarJam.Events;

namespace GvarJam.Managers;

public sealed class AudioDirector : GameObjectSystem<AudioDirector>
{
	[ConVar( "enable_ambience" )]
	public static bool EnableAmbience { get; set; } = true;

	private const float AmbienceVolume = 0.25f;
	private const float MinimumAmbientNoiseDelay = 50f;
	private const float MaximumAmbientNoiseDelay = 80f;
	private const float MinimumAmbientNoiseRange = 100f;
	private const float MaximumAmbientNoiseRange = 300f;

	private SoundHandle _currentMusicAmbience;
	private SoundHandle _currentChaseMusic;
	private TimeUntil _timeUntilNextAmbientNoise = Random.Shared.Float( MinimumAmbientNoiseDelay, MaximumAmbientNoiseDelay );

	private static readonly string[] AmbientMusic =
	{
		"slow_creep_ambient",
		"EerieSoundsToMakeYouShitYourself"
	};

	private static readonly string[] AmbientNoises =
	{
		"spooky_scuttle",
		"spooky_scuttle_long"
	};

	private static readonly string[] ChaseSounds =
	{
		"chase"
	};

	public AudioDirector( Scene scene ) : base( scene )
	{
		Listen( Stage.PhysicsStep, 0, MonitorPlayback, "AudioDirectorTick" );
	}

	public static SoundHandle Play2D( string soundName, float volume = 1.0f )
	{
		var handle = Sound.Play( soundName );
		if ( handle.IsValid() )
		{
			handle.SpacialBlend = 0f;
			handle.Volume = volume;
		}
		return handle;
	}

	public static SoundHandle PlayAt( string soundName, Vector3 position, float volume = 1.0f )
	{
		var handle = Sound.Play( soundName, position );
		if ( handle.IsValid() )
			handle.Volume = volume;
		return handle;
	}

	public static SoundHandle PlayAt( SoundEvent sound, Vector3 position, float volume = 1.0f )
	{
		if ( sound is null ) return null;
		var handle = Sound.Play( sound, position );
		if ( handle.IsValid() )
			handle.Volume = volume;
		return handle;
	}

	public void BeginAmbiencePlayback()
	{
		if ( !EnableAmbience ) return;

		var randomSound = Random.Shared.FromArray( AmbientMusic );
		_currentMusicAmbience = Play2D( randomSound, AmbienceVolume );
	}

	public void ShouldPlayChaseSounds( bool shouldPlay )
	{
		if ( !shouldPlay )
		{
			_currentChaseMusic?.Stop( 0.5f );
			return;
		}

		if ( _currentChaseMusic.IsValid() && _currentChaseMusic.IsPlaying )
			return;

		var randomSound = Random.Shared.FromArray( ChaseSounds );
		_currentChaseMusic = Play2D( randomSound );
	}

	public void SetChaseSoundVolume( float soundVolume )
	{
		if ( _currentChaseMusic.IsValid() )
			_currentChaseMusic.Volume = soundVolume;
	}

	public static void PlayMonsterSound( Scene scene, string soundName, Vector3 position, float volume = 1.0f )
	{
		IMonsterSound.Post( x => x.OnMonsterSound( position, volume ) );
		PlayAt( soundName, position, volume );
	}

	public static void PlayMonsterSound( Scene scene, SoundEvent sound, Vector3 position, float volume = 1.0f )
	{
		if ( sound is null ) return;
		IMonsterSound.Post( x => x.OnMonsterSound( position, volume ) );
		PlayAt( sound, position, volume );
	}

	private void MonitorPlayback()
	{
		if ( Game.IsEditor && !Scene.IsEditor ) { }
		if ( Scene.IsEditor ) return;

		if ( !EnableAmbience )
		{
			_currentMusicAmbience?.Stop();
		}
		else
		{
			if ( !_currentMusicAmbience.IsValid() || !_currentMusicAmbience.IsPlaying )
				BeginAmbiencePlayback();

			if ( _currentMusicAmbience.IsValid() )
				_currentMusicAmbience.Volume = AmbienceVolume;
		}

		if ( _timeUntilNextAmbientNoise > 0 )
			return;

		var player = Scene.GetAllComponents<ColdOpenPlayer>().OrderBy( _ => Guid.NewGuid() ).FirstOrDefault();
		if ( player is null )
			return;

		float posX = Random.Shared.Int( 0, 1 ) == 1
			? Random.Shared.Float( player.WorldPosition.x + MinimumAmbientNoiseRange, player.WorldPosition.x + MaximumAmbientNoiseRange )
			: Random.Shared.Float( player.WorldPosition.x - MaximumAmbientNoiseRange, player.WorldPosition.x - MinimumAmbientNoiseRange );
		float posY = Random.Shared.Int( 0, 1 ) == 1
			? Random.Shared.Float( player.WorldPosition.y + MinimumAmbientNoiseRange, player.WorldPosition.y + MaximumAmbientNoiseRange )
			: Random.Shared.Float( player.WorldPosition.y - MaximumAmbientNoiseRange, player.WorldPosition.y - MinimumAmbientNoiseRange );

		var resultPos = new Vector3( posX, posY, player.WorldPosition.z );
		var tr = Scene.Trace.Sphere( 100f, resultPos, resultPos ).Run();
		if ( !tr.Hit )
			return;

		_timeUntilNextAmbientNoise = Random.Shared.Float( MinimumAmbientNoiseDelay, MaximumAmbientNoiseDelay );
		PlayAt( Random.Shared.FromArray( AmbientNoises ), resultPos );
	}
}
