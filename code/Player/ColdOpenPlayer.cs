namespace GvarJam.Player;

public enum ViewModeType
{
	FirstPerson,
	ThirdPerson
}

public sealed class ColdOpenPlayer : Component
{
	public SkinnedModelRenderer BodyRenderer { get; private set; }
	public CharacterController Character { get; private set; }
	public ColdOpenMovement Movement { get; private set; }
	public PlayerCameraComponent Camera { get; private set; }
	public PlayerInteractionComponent Interaction { get; private set; }
	public HelmetLampComponent Lamp { get; private set; }
	public InventoryComponent Inventory { get; private set; }
	public CutsceneRunnerComponent Cutscenes { get; private set; }

	[Sync] public bool BlockMovement { get; set; }
	[Sync] public bool BlockLook { get; set; }
	[Sync] public bool IsDead { get; private set; }
	[Sync] public string LastInteractedEntityName { get; set; }
	[Sync] public string LastInteractedEntityTypeName { get; set; }
	public bool IsHidingInLocker => Interaction?.InteractedEntity is PlayerLockerComponent;

	public Angles ViewAngles { get; set; }
	public Vector3 InputDirection { get; set; }
	public Vector3 Velocity
	{
		get => Character?.Velocity ?? Vector3.Zero;
		set { if ( Character is not null ) Character.Velocity = value; }
	}
	public bool IsGrounded => Character?.IsOnGround ?? false;

	public Vector3 EyeLocalPosition => Vector3.Up * (64f * WorldScale.z);
	public Vector3 EyePosition => WorldPosition + EyeLocalPosition;
	public Rotation EyeRotation => ViewAngles.ToRotation();

	private TimeSince _timeSinceLastFootstep;
	private GameObject _ragdoll;
	private readonly List<ModelRenderer> _bodyAndClothingRenderers = new();

	protected override void OnAwake()
	{
		GameObject.Tags.Add( "player" );

		Character = GameObject.GetOrAddComponent<CharacterController>();
		Character.Radius = 16f;
		Character.Height = 64f;
		Character.StepHeight = 24f;
		Character.GroundAngle = 45f;
		Character.IgnoreLayers.Add( "trigger" );

		var bodyGo = new GameObject( true, "Body" );
		bodyGo.SetParent( GameObject, false );
		BodyRenderer = bodyGo.AddComponent<SkinnedModelRenderer>();
		BodyRenderer.Model = Model.Load( "models/player/playermodel.vmdl" );
		BodyRenderer.CreateBoneObjects = true;
		BodyRenderer.CreateAttachments = true;
		BodyRenderer.OnFootstepEvent += OnAnimFootstep;
		_bodyAndClothingRenderers.Add( BodyRenderer );

		var jumpsuit = ResourceLibrary.Get<Clothing>( "models/citizen_clothes/shirt/jumpsuit/blue_jumpsuit.clothing" );
		if ( jumpsuit is not null )
		{
			var clothes = new ClothingContainer();
			clothes.Add( jumpsuit );
			clothes.Apply( BodyRenderer );
			foreach ( var r in BodyRenderer.GameObject.GetComponentsInChildren<ModelRenderer>() )
			{
				if ( !_bodyAndClothingRenderers.Contains( r ) )
					_bodyAndClothingRenderers.Add( r );
			}
		}

		Inventory = GameObject.GetOrAddComponent<InventoryComponent>();
		Movement = GameObject.GetOrAddComponent<ColdOpenMovement>();
		Camera = GameObject.GetOrAddComponent<PlayerCameraComponent>();
		Interaction = GameObject.GetOrAddComponent<PlayerInteractionComponent>();
		Lamp = GameObject.GetOrAddComponent<HelmetLampComponent>();
		Cutscenes = GameObject.GetOrAddComponent<CutsceneRunnerComponent>();

		ViewAngles = WorldRotation.Angles();
	}

	public void RegisterHideableRenderer( ModelRenderer r )
	{
		if ( r is not null && !_bodyAndClothingRenderers.Contains( r ) )
			_bodyAndClothingRenderers.Add( r );
	}

	protected override void OnUpdate()
	{
		BuildInput();

		if ( IsDead )
		{
			if ( Input.Released( "Jump" ) )
				Respawn();
			UpdateAlpha();
			return;
		}

		if ( Input.Pressed( "View" ) )
		{
			if ( Camera.ViewMode == ViewModeType.FirstPerson )
				Camera.GoToThirdPerson();
			else
				Camera.GoToFirstPerson();
		}

		if ( !BlockLook && !Interaction.IsInteracting )
		{
			WorldRotation = ViewAngles.WithPitch( 0f ).ToRotation();
		}

		UpdateAnimation();
		UpdateAlpha();
		Lamp.SyncLampTransform();

		if ( ColdOpenGame.Debug )
		{
			var i = 0;
			foreach ( var val in Enum.GetValues<ItemType>() )
			{
				var count = Inventory.Items.TryGetValue( (int)val, out var c ) ? c : 0;
				DebugOverlay.ScreenText( new Vector2( 20, 20 * (15 + i) ), $"{val}: {count}" );
				i++;
			}
		}
	}

	private void BuildInput()
	{
		InputDirection = Input.AnalogMove;
		if ( BlockLook )
			return;

		var look = Input.AnalogLook;
		if ( ViewAngles.pitch > 90f || ViewAngles.pitch < -90f )
			look = look.WithYaw( look.yaw * -1f );

		var va = ViewAngles + look;
		va.pitch = va.pitch.Clamp( -89f, 89f );
		va.roll = 0f;
		ViewAngles = va.Normal;
	}

	private void UpdateAnimation()
	{
		if ( BodyRenderer is null || !BodyRenderer.IsValid() ) return;

		var helper = new CitizenAnimationHelper { Target = BodyRenderer };
		helper.WithVelocity( Velocity );
		helper.WithLook( EyePosition + EyeRotation.Forward * 100f );
		helper.IsGrounded = IsGrounded;
		helper.HoldType = CitizenAnimationHelper.HoldTypes.None;

		if ( Movement.ConsumeJumpEvent() )
			helper.TriggerJump();
	}

	private void UpdateAlpha()
	{
		float alpha = IsDead ? 0f
			: Cutscenes.HidingPlayers ? 0f
			: IsHidingInLocker ? 0f
			: Camera.GetPlayerAlpha();

		foreach ( var r in _bodyAndClothingRenderers )
		{
			if ( !r.IsValid() ) continue;
			r.Tint = r.Tint.WithAlpha( alpha );
		}
	}

	private void OnAnimFootstep( SceneModel.FootstepEvent e )
	{
		if ( IsDead || _timeSinceLastFootstep < 0.2f )
			return;

		float volume = e.Volume * Velocity.WithZ( 0 ).Length.LerpInverse( 0f, 200f );
		_timeSinceLastFootstep = 0;

		var tr = Scene.Trace.Ray( e.Transform.Position, e.Transform.Position + Vector3.Down * 20f )
			.Radius( 1f )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();

		if ( !tr.Hit ) return;

		var sound = e.FootId == 0 ? tr.Surface?.SoundCollection.FootLeft : tr.Surface?.SoundCollection.FootRight;
		if ( sound is not null )
		{
			AudioDirector.PlayMonsterSound( Scene, sound, tr.EndPosition, volume );
		}
	}

	public void OnKilled()
	{
		if ( IsDead ) return;

		IsDead = true;
		BlockLook = true;
		BlockMovement = true;
		Interaction.InteractedEntity = null;
		Camera.GoToThirdPerson();

		foreach ( var r in _bodyAndClothingRenderers )
		{
			if ( r.IsValid() ) r.Enabled = false;
		}

		// Spawn ragdoll
		_ragdoll = new GameObject( true, "PlayerRagdoll" );
		_ragdoll.WorldPosition = WorldPosition + WorldRotation.Forward * 3f;
		_ragdoll.WorldRotation = WorldRotation;
		_ragdoll.Tags.Add( "trigger" );

		var ragSkinned = _ragdoll.AddComponent<SkinnedModelRenderer>();
		ragSkinned.Model = Model.Load( "models/player/playermodel.vmdl" );
		var phys = _ragdoll.AddComponent<ModelPhysics>();
		phys.Model = ragSkinned.Model;
		phys.Renderer = ragSkinned;

		if ( Lamp.HasHelmet )
		{
			var helmGo = new GameObject( true, "RagdollHelmet" );
			helmGo.SetParent( _ragdoll, false );
			var hm = helmGo.AddComponent<ModelRenderer>();
			hm.Model = Model.Load( "models/cosmetics/spacehelmet.vmdl" );
		}

		var jumpsuit = ResourceLibrary.Get<Clothing>( "models/citizen_clothes/shirt/jumpsuit/blue_jumpsuit.clothing" );
		if ( jumpsuit is not null )
		{
			var clothes = new ClothingContainer();
			clothes.Add( jumpsuit );
			clothes.Apply( ragSkinned );
		}
	}

	public void Respawn()
	{
		IsDead = false;
		BlockLook = false;
		BlockMovement = false;
		Camera.GoToFirstPerson();

		foreach ( var r in _bodyAndClothingRenderers )
		{
			if ( r.IsValid() ) r.Enabled = true;
		}

		if ( _ragdoll.IsValid() )
		{
			var toDestroy = _ragdoll;
			_ragdoll = null;
			_ = toDestroy.DeleteAfterSecondsAndNotVisible( 10f );
		}

		if ( Movement is not null )
			Movement.Stamina = 1f;

		if ( Lamp is not null && Lamp.HasHelmet )
		{
			Lamp.BatteryInserted = true;
			Lamp.LampPower = 100f;
		}

		foreach ( var monster in Scene.GetAllComponents<MonsterComponent>() )
		{
			if ( monster.State == MonsterStates.Hunting )
				monster.State = MonsterStates.Stalking;
		}

		var game = Scene.GetSystem<ColdOpenGame>();
		game?.RespawnWorldBatteries();
		Scene.GetSystem<AudioDirector>()?.ShouldPlayChaseSounds( false );
		game?.MoveToSpawnpoint( GameObject );
	}
}
