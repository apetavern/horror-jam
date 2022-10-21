namespace GvarJam;

partial class Pawn : AnimatedEntity
{
	[Net]
	public PawnAnimator Animator { get; private set; }

	public CameraMode Camera
	{
		get => Components.Get<CameraMode>();
		private set
		{
			Components.RemoveAny<CameraMode>();
			Components.Add( value );
		}
	}

	[Net]
	public BasePlayerController Controller { get; private set; }

	/// <summary>
	/// Called when the entity is first created 
	/// </summary>
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen/citizen.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		Animator = new StandardPlayerAnimator();
		Camera = new Camera();
		Controller = new WalkController();

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
	}

	/// <summary>
	/// Called every tick, clientside and serverside.
	/// </summary>
	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Controller?.Simulate( cl, this, Animator );
	}

	/// <summary>
	/// Called every frame on the client
	/// </summary>
	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		Controller?.FrameSimulate( cl, this, Animator );
	}
}
