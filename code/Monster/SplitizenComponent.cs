namespace GvarJam.Monster;

public sealed class SplitizenComponent : Component
{
	[ConVar( "debug_monster", ConVarFlags.Replicated )]
	public static bool DebugEnabled { get; set; } = false;

	[Sync] public bool StopMoving { get; set; }
	public bool Ditched { get; private set; }
	public bool Split { get; private set; }

	private SkinnedModelRenderer _bodyRenderer;
	private SkinnedModelRenderer _splitTopRenderer;
	private SkinnedModelRenderer _cosmeticMonster;
	private NavMeshAgent _agent;
	private CharacterController _cc;
	private MonsterComponent _monster;
	private Vector3 _startPos;
	private HelmetPickupComponent _helmet;
	private bool _setAPath;

	protected override void OnStart()
	{
		_startPos = WorldPosition;
		GameObject.Tags.Add( "splitizen" );

		_bodyRenderer = GameObject.GetOrAddComponent<SkinnedModelRenderer>();
		_bodyRenderer.Model = Model.Load( "models/player/playermodel.vmdl" );
		_bodyRenderer.CreateBoneObjects = true;
		_bodyRenderer.MaterialOverride = Material.Load( "models/enemy/materials/citizen/splitizen_skin.vmat" );
		_bodyRenderer.SetBodyGroup( 0, 1 );
		_bodyRenderer.SetBodyGroup( 1, 1 );
		_bodyRenderer.SetBodyGroup( 2, 1 );
		_bodyRenderer.SetBodyGroup( 3, 1 );

		var jeansModel = Model.Load( "models/citizen_clothes/trousers/jeans/models/jeans.vmdl" )
			?? Model.Load( "models/citizen_clothes/jacket/longsleeve/models/jeans.vmdl" );
		if ( jeansModel is not null && !jeansModel.IsError )
		{
			var jeansGo = new GameObject( true, "Jeans" );
			jeansGo.SetParent( GameObject, false );
			var jr = jeansGo.AddComponent<SkinnedModelRenderer>();
			jr.Model = jeansModel;
			jr.BoneMergeTarget = _bodyRenderer;
		}

		var cosmeticGo = new GameObject( true, "CosmeticMonster" );
		cosmeticGo.SetParent( GameObject, false );
		_cosmeticMonster = cosmeticGo.AddComponent<SkinnedModelRenderer>();
		_cosmeticMonster.Model = Model.Load( "models/enemy/monster.vmdl" );
		_cosmeticMonster.BoneMergeTarget = _bodyRenderer;

		var topGo = new GameObject( true, "SplitTop" );
		topGo.SetParent( GameObject, false );
		_splitTopRenderer = topGo.AddComponent<SkinnedModelRenderer>();
		_splitTopRenderer.Model = Model.Load( "models/enemy/basic_splitizen.vmdl" );
		_splitTopRenderer.CreateBoneObjects = true;
		_splitTopRenderer.BoneMergeTarget = _bodyRenderer;

		_cc = GameObject.GetOrAddComponent<CharacterController>();
		_cc.Radius = 16f;
		_cc.Height = 72f;
		_cc.StepHeight = 16f;
		_cc.IgnoreLayers.Add( "trigger" );

		_agent = GameObject.GetOrAddComponent<NavMeshAgent>();
		_agent.Radius = 16f;
		_agent.Height = 72f;
		_agent.MaxSpeed = 100f;
		_agent.UpdatePosition = false;
		_agent.UpdateRotation = false;
		_helmet = Scene.GetAllComponents<HelmetPickupComponent>().FirstOrDefault();

		if ( !StopMoving )
		{
			WorldRotation = Rotation.LookAt( Vector3.Random.WithZ( 0 ).Normal, Vector3.Up );
			_ = RandomPatrolLoop();
		}
	}

	private async Task RandomPatrolLoop()
	{
		while ( this.IsValid() && !StopMoving && !Ditched )
		{
			var pt = Scene.NavMesh.GetRandomPoint( WorldPosition, 2048f );
			if ( pt.HasValue )
			{
				SetPath( pt.Value );
				_setAPath = true;
				await GameTask.DelaySeconds( Random.Shared.Float( 5f, 20f ) );
			}
			else
			{
				await GameTask.DelaySeconds( 0.5f );
			}
		}
	}

	public async void DoSplit()
	{
		await GameTask.DelaySeconds( 2f );
		if ( !this.IsValid() ) return;

		if ( _cosmeticMonster.IsValid() )
			_cosmeticMonster.GameObject.Destroy();

		var monGo = new GameObject( true, "Monster" );
		monGo.SetParent( GameObject, false );
		_monster = monGo.AddComponent<MonsterComponent>();
		_monster?.SetBoneMergeTarget( _bodyRenderer );

		_splitTopRenderer?.Set( "split", true );
		_monster?.SetAnimParam( "split", true );

		await GameTask.DelaySeconds( Time.Delta );
		if ( !this.IsValid() ) return;

		_splitTopRenderer?.Set( "split", true );
		_monster?.SetAnimParam( "split", true );
		Split = true;

		await GameTask.DelaySeconds( 4f );
		if ( this.IsValid() )
			DitchBody();
	}

	public void DitchBody()
	{
		if ( Ditched ) return;
		Ditched = true;

		if ( _agent.IsValid() )
			_agent.Enabled = false;
		if ( _cc.IsValid() )
			_cc.Enabled = false;
		var phys = GameObject.GetOrAddComponent<ModelPhysics>();
		phys.Model = _bodyRenderer.Model;
		phys.Renderer = _bodyRenderer;
	}

	protected override void OnFixedUpdate()
	{
		if ( Ditched ) return;

		if ( !StopMoving )
		{
			if ( WorldPosition.z <= -32f )
			{
				WorldPosition = Scene.NavMesh.GetClosestPoint( WorldPosition ) ?? (WorldPosition + Vector3.Up * 32f);
			}

			if ( (!_setAPath || (_agent.IsValid() && !_agent.IsNavigating)) && Scene.NavMesh.GetRandomPoint( WorldPosition, 2048f ) is Vector3 pt )
			{
				SetPath( pt );
				_setAPath = true;
			}

			if ( _helmet.IsValid() && Vector3.DistanceBetween( WorldPosition, _helmet.WorldPosition ) < 1024f )
				SetPath( _startPos );

			if ( _cc.IsValid() && _agent.IsValid() )
			{
				var wish = _agent.WishVelocity;
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
		}
		if ( _splitTopRenderer.IsValid() )
		{
			var pelvisPos = _splitTopRenderer.GetBoneObject( "pelvis" )?.WorldPosition ?? (WorldPosition + Vector3.Up * 36f);
			var wallTr = Scene.Trace.Ray( pelvisPos, pelvisPos + WorldRotation.Forward * Random.Shared.Float( 25f, 65f ) )
				.IgnoreGameObjectHierarchy( GameObject )
				.Run();

			bool atDoor = wallTr.GameObject?.GetComponentInParent<DoorControllerComponent>() is not null;
			_splitTopRenderer.Set( "bangdoor", atDoor );
		}

		var helper = new CitizenAnimationHelper { Target = _bodyRenderer };
		helper.WithVelocity( _cc?.Velocity ?? Vector3.Zero );
		helper.IsGrounded = true;
	}

	public void SetPath( Vector3 target )
	{
		if ( !StopMoving && _agent.IsValid() )
			_agent.MoveTo( target );
	}
}
