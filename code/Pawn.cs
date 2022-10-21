using GvarJam.HammerEntities;
using System.Collections.Generic;

namespace GvarJam;

/// <summary>
/// The players pawn.
/// </summary>
public sealed partial class Pawn : AnimatedEntity
{
	/// <summary>
	/// The active animator of the pawn.
	/// </summary>
	[Net]
	public PawnAnimator Animator { get; private set; } = null!;

	/// <summary>
	/// The active camera for the pawn.
	/// </summary>
	public Camera Camera
	{
		get => Components.Get<Camera>();
		private set
		{
			Components.RemoveAny<CameraMode>();
			Components.Add( value );
		}
	}

	/// <summary>
	/// The active controller for the pawn.
	/// </summary>
	[Net]
	public BasePlayerController Controller { get; private set; } = null!;

	/// <summary>
	/// The pawns helmet.
	/// </summary>
	[Net]
	private ModelEntity Helmet { get; set; } = null!;
	/// <summary>
	/// The pawns lamp.
	/// </summary>
	[Net]
	private SpotLightEntity Lamp { get; set; } = null!;

	/// <summary>
	/// This is a list of stuff we apply the alpha change to
	/// ( This gets stored so that we're not constantly allocating )
	/// </summary>
	private List<ModelEntity> PlayerAndChildren { get; set; } = null!;

	/// <summary>
	/// Called when the entity is first created 
	/// </summary>
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen/citizen.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		Helmet = new ModelEntity( "models/cosmetics/spacehelmet.vmdl" )
		{
			EnableHideInFirstPerson = true
		};

		Lamp = new SpotLightEntity
		{
			Parent = Helmet,
			Transform = Helmet.GetAttachment( "light" )!.Value
		};

		Lamp.OuterConeAngle *= 0.4f;
		Lamp.InnerConeAngle *= 0.1f;
		Lamp.Brightness = 0.5f;

		Helmet.SetParent( this, true );

		Animator = new StandardPlayerAnimator();
		Camera = new Camera();
		Controller = new MovementController();

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		//
		// Set up hideable entities
		//
		PlayerAndChildren = new List<ModelEntity>();
		PlayerAndChildren.Add( this );
		PlayerAndChildren.AddRange( Children.OfType<ModelEntity>() );
	}

	/// <inheritdoc/>
	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( Input.Pressed( InputButton.View ) )
		{
			if ( Camera.ViewMode == ViewModeType.FirstPerson )
				Camera.GoToThirdPerson();
			else
				Camera.GoToFirstPerson();
		}

		if ( Input.Pressed( InputButton.Reload ) && IsServer)
		{
			MountedCamera cam = new MountedCamera();
			cam.Position = Position + Rotation.Forward * 100f + Vector3.Up * 100f;
		}

		SimulateLamp();
		SimulateInteraction();

		var rotation = Rotation;
		Controller?.Simulate( cl, this, Animator );
		if ( IsInteracting )
			Rotation = rotation;
	}

	/// <inheritdoc/>
	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		var rotation = Rotation;
		Controller?.FrameSimulate( cl, this, Animator );
		if ( IsInteracting )
			Rotation = rotation;
	}

	/// <summary>
	/// Sets the alpha on the pawns render color before the game renders.
	/// </summary>
	[Event.PreRender]
	public void OnPreRender()
	{
		//
		// CACHING THE CHILDREN will definitely become a problem later!!
		// This is a fast but probably not elegant solution
		// If you need this changing or fixing, find Alex
		//
		var alpha = GetAlpha();
		PlayerAndChildren?.ForEach( x => x.RenderColor = x.RenderColor.WithAlpha( alpha ) );
	}

	/// <summary>
	/// Controls the alpha/transparency of the player. It's mainly used for transitions between the two camera modes.
	/// </summary>
	/// <returns>The transparency to render the player at.</returns>
	private float GetAlpha()
	{
		if ( Camera is Camera camera )
			return camera.GetPlayerAlpha();

		return 1.0f;
	}
}
