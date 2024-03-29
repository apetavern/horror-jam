﻿using Sandbox.Utility;

namespace GvarJam.Player;

/// <summary>
/// The camera for the pawn.
/// </summary>
public sealed partial class PawnCamera : CameraComponent
{
	/// <summary>
	/// The speed at which to switch between <see cref="ViewMode"/>s.
	/// </summary>
	private const float ModeSwitchSpeed = 2.0f;

	/// <summary>
	/// The current view mode of the camera.
	/// </summary>
	[Net, Predicted]
	public ViewModeType ViewMode { get; private set; }

	/// <summary>
	/// The progress of switching between different modes.
	/// </summary>
	private float modeSwitchProgress;

	/// <summary>
	/// The last position of the camera.
	/// </summary>
	private Vector3 lastPos;

	/// <summary>
	/// Whether or not to override the transform of the camera.
	/// </summary>
	private bool shouldOverrideTransform;

	/// <summary>
	/// The value of the overriden transform.
	/// </summary>
	private Transform overrideTransform;

	/// <inheritdoc/>
	protected override void OnActivate()
	{
		var pawn = Entity;
		if ( pawn == null ) return;

		Camera.Position = pawn.EyePosition;
		Camera.Rotation = pawn.EyeRotation;

		lastPos = Camera.Position;
	}

	/// <inheritdoc/>
	public override void Update()
	{
		var pawn = Entity;
		if ( pawn == null ) return;

		modeSwitchProgress += Time.Delta;

		var targetPos = ViewMode switch
		{
			ViewModeType.FirstPerson => UpdateFirstPerson( pawn ),
			ViewModeType.ThirdPerson => UpdateThirdPerson( pawn ),
			_ => (Vector3)0,
		};

		var targetRot = pawn.EyeRotation;

		if ( shouldOverrideTransform )
		{
			targetPos = overrideTransform.Position;
			targetRot = overrideTransform.Rotation;
		}

		Camera.Position = Camera.Position.LerpTo( targetPos, modeSwitchProgress * ModeSwitchSpeed );
		Camera.Rotation = targetRot;
		Camera.FirstPersonViewer = null;
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( 90 );
	}

	/// <summary>
	/// Overrides the transform of the camera.
	/// </summary>
	/// <param name="transform">The transform for the camera to be at.</param>
	public void OverrideTransform( Transform transform )
	{
		shouldOverrideTransform = true;
		overrideTransform = transform;
	}

	/// <summary>
	/// Disables the current override on the transform.
	/// </summary>
	public void DisableTransformOverride()
	{
		shouldOverrideTransform = false;
	}

	/// <summary>
	/// Gets the alpha value that something related to the player should be rendered at.
	/// </summary>
	/// <returns>The alpha value that something related to the player should be rendered at.</returns>
	public float GetPlayerAlpha()
	{
		//
		// TODO: It might be worth scaling this by the inverse distance
		// between the player and the camera, so that the third person
		// camera colliding with a wall doesn't fuck us and make the
		// player look really ugly. I will look into this later
		//

		float baseAlpha = modeSwitchProgress * ModeSwitchSpeed * 4.0f;
		baseAlpha = baseAlpha.Clamp( 0, 1 );

		return ViewMode switch
		{
			ViewModeType.FirstPerson => 1.0f - baseAlpha,
			ViewModeType.ThirdPerson => baseAlpha,
			_ => 1.0f,
		};
	}

	/// <summary>
	/// Sets the <see cref="ViewMode"/> to <see cref="ViewModeType.FirstPerson"/>.
	/// </summary>
	public void GoToFirstPerson()
	{
		if ( ViewMode == ViewModeType.FirstPerson )
			return;

		ViewMode = ViewModeType.FirstPerson;
		modeSwitchProgress = 0;
	}

	/// <summary>
	/// Sets the <see cref="ViewMode"/> to <see cref="ViewModeType.ThirdPerson"/>.
	/// </summary>
	public void GoToThirdPerson()
	{
		if ( ViewMode == ViewModeType.ThirdPerson )
			return;

		ViewMode = ViewModeType.ThirdPerson;
		modeSwitchProgress = 0;
	}

	/// <summary>
	/// Updates the third person view.
	/// </summary>
	/// <param name="pawn">The pawn that owns the camera.</param>
	/// <returns>The new position of the camera.</returns>
	private Vector3 UpdateThirdPerson( Pawn pawn )
	{
		var targetPos = pawn.EyePosition
		                + pawn.EyeRotation.Backward * 64
		                + pawn.EyeRotation.Right * 16;

		var tr = Trace.Ray( pawn.EyePosition, targetPos ).Ignore( pawn ).WithoutTags( "camignore" ).Radius( 16f ).Run();
		targetPos = tr.EndPosition;

		return InterpolatePosition( targetPos );
	}

	/// <summary>
	/// Updates the first person view.
	/// </summary>
	/// <param name="pawn">The pawn that owns the camera.</param>
	/// <returns>The new position of the camera.</returns>
	private Vector3 UpdateFirstPerson( Pawn pawn )
	{
		var eyePos = pawn.EyePosition;
		var targetPos = eyePos;

		return InterpolatePosition( targetPos ) + GetAdditiveNoise();
	}

	/// <summary>
	/// Interpolate a position on the z-axis, useful for making things
	/// like crouching look much smoother.
	/// </summary>
	/// <param name="targetPos">The position to lerp to.</param>
	/// <returns>The lerped pos.</returns>
	private Vector3 InterpolatePosition( Vector3 targetPos )
	{
		//
		// We're doing this here (and calling this in the UpdateXPerson functions)
		// in order to prevent it from being affected by the transitions between
		// first & third person.
		// Not super elegant.. too bad!
		//
		if ( targetPos.Distance( lastPos ) < 300 )
			targetPos = Vector3.Lerp( targetPos.WithZ( lastPos.z ), targetPos, 20.0f * Time.Delta );

		lastPos = targetPos;
		return targetPos;
	}

	/// <summary>
	/// Returns time-based noise, normalized from -1 to 1.
	/// </summary>
	/// <param name="offset"></param>
	/// <param name="speed"></param>
	/// <returns>The noise that was generated.</returns>
	private float GetNoise( float offset, float speed )
	{
		var noise = Noise.Perlin( offset, offset, Time.Now * speed );
		noise -= 0.5f;
		noise *= 2f;

		return noise;
	}

	/// <summary>
	/// Provides noise for a 'breathing' effect.
	/// We could potentially multiply this by some factor later
	/// depending on how close a player is to the monster
	/// (i.e. some sort of 'sanity' value)
	/// </summary>
	/// <returns>The noise that was generated.</returns>
	private Vector3 GetAdditiveNoise()
	{
		var noise = GetNoise( 0.1f, 8f ) * Camera.Rotation.Up
		            + GetNoise( 5.5f, 1f ) * Camera.Rotation.Forward
		            + GetNoise( 16f, 8f ) * Camera.Rotation.Right;

		return noise * 4f;
	}
}
