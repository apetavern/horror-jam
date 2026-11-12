using GvarJam.Events;

namespace GvarJam.Monster;

public sealed class MonsterComponent : Component, IMonsterSound
{
	private MonsterStates _state = MonsterStates.Dormant;
	public MonsterStates State
	{
		get => _state;
		set
		{
			if ( _state != value )
			{
				var oldState = _state;
				_state = value;
				OnStateChange( oldState, value );
			}
		}
	}

	private SkinnedModelRenderer _renderer;
	private NavMeshAgent _agent;
	private CharacterController _cc;
	private Vector3 _targetPosition;
	private TimeSince _timeInState;
	private TimeSince _timeSinceSawPlayer;
	private float _averageSpeed = 50f;
	private float _soundLevel;
	private Vector3 _lastSound;
	private TimeSince _timeSincePathUpdate = 10f;
	private bool _hasPath;
	private float _smoothedWalkspeed;

	public Vector3 EyePosition => WorldPosition + Vector3.Up * 64f;

	protected override void OnAwake()
	{
		_renderer = GameObject.GetOrAddComponent<SkinnedModelRenderer>();
		_renderer.Model = Model.Load( "models/enemy/monster.vmdl" );
		_renderer.OnGenericEvent += OnAnimGeneric;

		_cc = GameObject.GetOrAddComponent<CharacterController>();
		_cc.Radius = 16f;
		_cc.Height = 72f;
		_cc.StepHeight = 16f;
		_cc.IgnoreLayers.Add( "trigger" );
		_cc.Enabled = false;

		_agent = GameObject.GetOrAddComponent<NavMeshAgent>();
		_agent.Radius = 16f;
		_agent.Height = 72f;
		_agent.Acceleration = 200f;
		_agent.MaxSpeed = 100f;
		_agent.UpdatePosition = false;
		_agent.UpdateRotation = false;
		_agent.Enabled = false;
	}

	private void OnAnimGeneric( SceneModel.GenericEvent e )
	{
		if ( e.Type is not null && e.Type.Contains( "decouple", StringComparison.OrdinalIgnoreCase ) )
		{
			_renderer.BoneMergeTarget = null;
			GameObject.SetParent( null, true );
			Scene.GetAllComponents<SplitizenComponent>().FirstOrDefault()?.DitchBody();
			State = MonsterStates.Hunting;
		}
	}

	public void SetBoneMergeTarget( SkinnedModelRenderer target )
	{
		if ( _renderer is not null )
			_renderer.BoneMergeTarget = target;
	}

	public void SetAnimParam( string name, bool val ) => _renderer?.Set( name, val );

	public void SetPath( Vector3 target, bool force = false )
	{
		if ( !_agent.IsValid() || State == MonsterStates.Dormant ) return;
		if ( !force && _hasPath && _agent.IsNavigating && _targetPosition.Distance( target ) < 24f && _timeSincePathUpdate < 0.25f )
			return;

		_targetPosition = target;
		_hasPath = true;
		_timeSincePathUpdate = 0;
		_agent.MoveTo( target );
	}

	public void SetRandomPath()
	{
		var pt = Scene.NavMesh.GetRandomPoint( WorldPosition, 2048f )
			?? Scene.NavMesh.GetRandomPoint()
			?? WorldPosition;
		SetPath( pt, true );
	}

	protected override void OnFixedUpdate()
	{
		if ( State == MonsterStates.Dormant )
			return;
		if ( _cc.IsValid() && _agent.IsValid() )
		{
			if ( !_cc.Enabled ) _cc.Enabled = true;
			if ( !_agent.Enabled ) { _agent.Enabled = true; _agent.SetAgentPosition( WorldPosition ); }

			var wish = _agent.WishVelocity;
			if ( State == MonsterStates.Hunting )
			{
				var nearPlayer = Scene.GetAllComponents<ColdOpenPlayer>().FirstOrDefault();
				if ( nearPlayer is not null && Vector3.DistanceBetween( WorldPosition, nearPlayer.WorldPosition ) < 50f )
					wish = Vector3.Zero;
			}

			_cc.Velocity = _cc.IsOnGround
				? wish.WithZ( 0 )
				: wish.WithZ( _cc.Velocity.z - 300f * Time.Delta );
			_cc.Move();
			_agent.SetAgentPosition( WorldPosition );

			if ( !wish.WithZ( 0 ).IsNearZeroLength )
			{
				var targetRot = Rotation.LookAt( wish.WithZ( 0 ).Normal, Vector3.Up );
				WorldRotation = Rotation.Lerp( WorldRotation, targetRot, 5f * Time.Delta );
			}
		}

		TickState();
		TickSound();

		if ( WorldPosition.z <= -32f )
		{
			var nearest = Scene.NavMesh.GetClosestPoint( WorldPosition );
			if ( nearest.HasValue )
			{
				WorldPosition = nearest.Value;
				_agent?.SetAgentPosition( WorldPosition );
			}
			else
				Scene.GetSystem<ColdOpenGame>()?.MoveToSpawnpoint( GameObject );
		}

		TickStuck();
		TickAnimator();
	}

	private void TickStuck()
	{
		var speed = _cc?.Velocity.Length ?? 0f;
		_averageSpeed = _averageSpeed.LerpTo( speed, Time.Delta );

		if ( _averageSpeed < 10f )
		{
			if ( State == MonsterStates.Hiding )
			{
				var player = Scene.GetAllComponents<ColdOpenPlayer>().GetClosestOrDefault( GameObject );
				var away = player is not null
					? WorldPosition + (WorldPosition - player.WorldPosition).Normal * 1024f
					: WorldPosition;
				var pt = Scene.NavMesh.GetClosestPoint( away ) ?? Scene.NavMesh.GetRandomPoint( WorldPosition, 1024f ) ?? away;
				SetPath( pt, true );
			}
			else
			{
				SetRandomPath();
				State = MonsterStates.Stalking;
			}
			_averageSpeed = 50f;
		}
	}

	private void TickAnimator()
	{
		if ( _renderer is null ) return;
		var speed = _cc?.Velocity.WithZ( 0 ).Length ?? 0f;
		float targetWalkspeed = speed.LerpInverse( 0f, 200f ) * 2.0f;
		if ( _agent.IsValid() && _agent.IsNavigating && speed > 5f )
			targetWalkspeed = MathF.Max( targetWalkspeed, 0.15f );

		_smoothedWalkspeed = _smoothedWalkspeed.LerpTo( targetWalkspeed, (10f * Time.Delta).Clamp( 0f, 1f ) );
		if ( _smoothedWalkspeed < 0.02f && speed < 2f )
			_smoothedWalkspeed = 0f;

		_renderer.Set( "idle", true );
		_renderer.Set( "walkspeed", _smoothedWalkspeed );
	}

	private void TickState()
	{
		switch ( State )
		{
			case MonsterStates.Hiding:
				SimulateHiding();
				break;
			case MonsterStates.Stalking:
				TickSeen();
				SimulateStalking();
				break;
			case MonsterStates.Hunting:
				TickSeen();
				SimulateHunting();
				break;
		}
	}

	private void TickSeen()
	{
		const float FieldOfView = 90f;
		const int TraceCount = 16;
		const float Radius = 4f;
		const float DefaultDistance = 256f;

		foreach ( var player in Scene.GetAllComponents<ColdOpenPlayer>() )
		{
			if ( player.IsDead ) continue;
			float maxDist = player.Lamp.LampEnabled ? 512f : DefaultDistance;
			var toPlayer = (player.EyePosition - EyePosition);
			float dist = toPlayer.Length;
			if ( dist > maxDist ) continue;

			float dot = Vector3.Dot( WorldRotation.Forward, toPlayer.Normal );
			float minDot = player.Lamp.LampEnabled ? 0.0f : MathF.Cos( (FieldOfView * 0.5f).DegreeToRadian() );
			if ( dot >= minDot && HasLineOfSight( player ) )
			{
				_timeSinceSawPlayer = 0;
				FollowPlayer( player );
				if ( !player.Lamp.LampEnabled )
					State = MonsterStates.Hunting;
				return;
			}
		}

		for ( int i = 0; i < TraceCount; i++ )
		{
			float f = (float)i / (TraceCount - 1);
			float ang = (f * FieldOfView) - (FieldOfView / 2f);
			var direction = (WorldRotation * Rotation.FromYaw( ang )).Forward;

			var startPos = EyePosition + Vector3.Down * 16f;
			var endPos = startPos + direction * DefaultDistance;

			var tr = Scene.Trace.Ray( startPos, endPos ).Radius( Radius ).IgnoreGameObjectHierarchy( GameObject ).Run();
			if ( !tr.Hit || tr.GameObject is null ) continue;

			var door = tr.GameObject.GetComponentInParent<DoorControllerComponent>();
			if ( door is not null && door.IsClosed && Scene.GetAllComponents<ColdOpenPlayer>().Any( p => p.Inventory.HasItem( door.ItemRequiredToOpen ) ) )
			{
				door.Toggle( GameObject );
			}
		}
	}

	private void OnStateChange( MonsterStates oldState, MonsterStates newState )
	{
		if ( newState != MonsterStates.Dormant )
		{
			if ( _cc.IsValid() ) _cc.Enabled = true;
			if ( _agent.IsValid() ) { _agent.Enabled = true; _agent.SetAgentPosition( WorldPosition ); }
		}

		_timeInState = 0;
		_averageSpeed = 50f;
		var audio = Scene.GetSystem<AudioDirector>();
		if ( oldState == MonsterStates.Hiding )
			audio?.ShouldPlayChaseSounds( false );

		var player = Scene.GetAllComponents<ColdOpenPlayer>().FirstOrDefault();
		if ( player is null ) return;

		if ( newState == MonsterStates.Stalking )
		{
			_agent.MaxSpeed = 100f;
			var pt = Scene.NavMesh.GetRandomPoint( player.WorldPosition, 2048f )
				?? player.WorldPosition;

			if ( Vector3.DistanceBetween( player.WorldPosition, WorldPosition ) < 70f )
				audio?.ShouldPlayChaseSounds( true );
			else
				audio?.ShouldPlayChaseSounds( false );

			SetPath( pt, true );
		}
		else if ( newState == MonsterStates.Hunting )
		{
			_agent.MaxSpeed = 200f;
			SetPath( player.WorldPosition, true );
		}
		else if ( newState == MonsterStates.Hiding )
		{
			_agent.MaxSpeed = 200f;
			var away = WorldPosition + (WorldPosition - player.WorldPosition).Normal * 1024f;
			var pt = Scene.NavMesh.GetClosestPoint( away ) ?? away;
			SetPath( pt, true );
		}
	}

	private void SimulateHiding()
	{
		var player = Scene.GetAllComponents<ColdOpenPlayer>().GetClosestOrDefault( GameObject );
		if ( player is not null && (!_agent.IsNavigating || _timeSincePathUpdate > 0.75f) )
		{
			var away = WorldPosition + (WorldPosition - player.WorldPosition).Normal * 1024f;
			var pt = Scene.NavMesh.GetClosestPoint( away ) ?? away;
			SetPath( pt );
		}
		if ( _timeInState > 15f )
			State = MonsterStates.Stalking;
	}

	private void SimulateStalking()
	{
		if ( _agent is not null )
			_agent.MaxSpeed = 100f;
	}

	private void SimulateHunting()
	{
		if ( _agent is not null )
			_agent.MaxSpeed = 200f;
		var player = Scene.GetAllComponents<ColdOpenPlayer>().FirstOrDefault();
		if ( player is null ) return;

		if ( HasLineOfSight( player ) )
		{
			_timeSinceSawPlayer = 0;
			if ( player.Lamp.LampEnabled )
			{
				State = MonsterStates.Hiding;
				return;
			}

			FollowPlayer( player );
			if ( State != MonsterStates.Hunting ) return;

			Scene.GetSystem<AudioDirector>()?.ShouldPlayChaseSounds( true );

			if ( Vector3.DistanceBetween( WorldPosition, player.WorldPosition ) < 70f )
			{
				if ( !player.IsDead && !player.Cutscenes.InCutscene && player.Cutscenes.TimeSinceCutsceneEnded > 5f )
				{
					player.OnKilled();
					AudioDirector.PlayAt( "monster_screech_2", WorldPosition );
				}
			}
		}

		SetPath( _targetPosition );
		bool finished = !_agent.IsNavigating || Vector3.DistanceBetween( WorldPosition, _targetPosition ) < 64f;
		if ( finished && _timeSinceSawPlayer > 15f )
		{
			State = MonsterStates.Stalking;
		}
	}

	private void FollowPlayer( ColdOpenPlayer player )
	{
		if ( player.Lamp.LampEnabled )
			State = MonsterStates.Hiding;
		else
			_targetPosition = player.WorldPosition;
	}

	private bool HasLineOfSight( ColdOpenPlayer player )
	{
		if ( player is null || !player.IsValid() || player.IsHidingInLocker ) return false;
		var tr = Scene.Trace.Ray( EyePosition, player.EyePosition )
			.IgnoreGameObjectHierarchy( GameObject )
			.IgnoreGameObjectHierarchy( player.GameObject )
			.WithoutTags( "trigger" )
			.Run();
		return !tr.Hit;
	}

	private void TickSound()
	{
		_soundLevel = _soundLevel.LerpTo( 0f, Time.Delta );
		if ( _soundLevel > 1f )
		{
			_targetPosition = _lastSound;
			State = MonsterStates.Hunting;
			_timeSinceSawPlayer = 0;
		}
	}

	void IMonsterSound.OnMonsterSound( Vector3 position, float volume )
	{
		float distance = position.Distance( WorldPosition ) * (1.0f - volume);
		if ( distance < 128f )
		{
			_soundLevel += volume;
			_lastSound = position;
		}
	}

	[ConCmd( "set_monster_target" )]
	public static void SetTargetCmd()
	{
		var scene = Game.ActiveScene;
		var player = scene?.GetAllComponents<ColdOpenPlayer>().FirstOrDefault();
		if ( player is null ) return;

		var tr = scene.Trace.Ray( player.EyePosition, player.EyePosition + player.EyeRotation.Forward * 1024f ).Run();
		if ( !tr.Hit ) return;

		foreach ( var m in scene.GetAllComponents<MonsterComponent>() )
			m.SetPath( tr.EndPosition );
	}

	[ConCmd( "set_monster_state" )]
	public static void SetStateCmd( MonsterStates newState )
	{
		foreach ( var m in Game.ActiveScene?.GetAllComponents<MonsterComponent>() ?? Enumerable.Empty<MonsterComponent>() )
			m.State = newState;
	}
}
