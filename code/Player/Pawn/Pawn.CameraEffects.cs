using Sandbox.Effects;

namespace GvarJam.Player;

//
// Camera effects (leaning, bobbing, etc.) that rely on player movement
// and player properties go here rather than directly in the camera class.
// This helps to separate things out & reduce co-dependencies between the
// two
//
partial class Pawn
{
	/// <summary>
	/// The step of roll rotation in degrees.
	/// </summary>
	private const float LeanDegrees = 0.5f;

	/// <summary>
	/// The maximum amount of roll rotation to have.
	/// </summary>
	private const float LeanMax = LeanDegrees * 2f;

	/// <summary>
	/// The amount of smoothing to apply to the camera roll.
	/// </summary>
	private const float LeanSmooth = 15.0f;

	/// <summary>
	/// The minimum speed the pawn can move at.
	/// </summary>
	private const float MinSpeed = 0f;

	/// <summary>
	/// The maximum speed the pawn can move at.
	/// </summary>
	private float MaxSpeed => Controller?.SprintSpeed ?? 320f;

	/// <summary>
	/// Unknown, FIXME.
	/// </summary>
	private const float FOVIncrease = 5f;

	/// <summary>
	/// The speed at which to lerp camera FOV.
	/// </summary>
	private const float FOVSpeed = 10f;

	/// <summary>
	/// The strength of the view bobbing.
	/// </summary>
	private const float BobStrength = 25f;

	/// <summary>
	/// The current amount of view bobbing on the camera.
	/// </summary>
	private float Bobbing { get; set; }

	/// <summary>
	/// The current roll component rotation on the camera.
	/// </summary>
	private float Roll { get; set; }

	/// <summary>
	/// The current camera FOV.
	/// </summary>
	private float FOV { get; set; }

	/// <inheritdoc/>
	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		var speed = Velocity.Length.LerpInverse( MinSpeed, MaxSpeed );
		var forwardSpeed = Velocity.Normal.Dot( camSetup.Rotation.Forward );

		var left = camSetup.Rotation.Left;
		var up = camSetup.Rotation.Up;

		//
		// Camera bob (up/down)
		//
		if ( GroundEntity != null )
			Bobbing += Time.Delta * BobStrength * speed;

		//
		// Camera roll
		//
		var targetRoll = Velocity.Dot( camSetup.Rotation.Right ) / 180f;
		targetRoll += MathF.Sin( Bobbing / 2f ) * LeanDegrees;
		targetRoll = targetRoll.Clamp( -LeanMax, LeanMax );
		Roll = Roll.LerpTo( targetRoll, Time.Delta * LeanSmooth );

		//
		// Camera FOV
		//
		FOV = FOV.LerpTo( speed * FOVIncrease * MathF.Abs( forwardSpeed ), Time.Delta * FOVSpeed );

		//
		// Apply everything
		//
		float x = MathF.Sin( Bobbing * 0.5f ) * speed * 1;
		float y = MathF.Sin( Bobbing ) * speed * 2;
		camSetup.Position += left * x;
		camSetup.Position += up * y;
		camSetup.Rotation *= Rotation.From( 0, 0, Roll );
		camSetup.FieldOfView += FOV;

		var tx = new PanelTransform();
		tx.AddTranslate( x, y );

		Local.Hud.Style.Transform = tx;
		Local.Hud.Style.Dirty();
	}

	/// <summary>
	/// Applies an amount of the vignette screen effect.
	/// </summary>
	/// <param name="amount">The amount of the effect to apply.</param>
	public void ApplyVignetteAmount( float amount )
	{
		var effects = Map.Camera.FindOrCreateHook<ScreenEffects>();
		effects.Vignette.Intensity = amount;
		effects.Vignette.Smoothness = 1f;
		effects.Vignette.Roundness = 0.3f;
	}

	/// <summary>
	/// Applies an amount of the brightness screen effect.
	/// </summary>
	/// <param name="amount">The amount of the effect to apply.</param>
	public void ApplyBrightnessAmount( float amount )
	{
		var effects = Map.Camera.FindOrCreateHook<ScreenEffects>();
		effects.Brightness = amount;
	}

	/// <summary>
	/// Applies an amount of the motion blue effect.
	/// </summary>
	/// <param name="amount">The amount of the effect to apply.</param>
	public void ApplyMotionBlur( float amount )
	{
		var effects = Map.Camera.FindOrCreateHook<ScreenEffects>();
		effects.MotionBlur.Scale = amount;
	}

	/// <summary>
	/// Applies an amount of the film grain effect.
	/// </summary>
	/// <param name="amount">The amount of the effect to apply.</param>
	public void ApplyFilmGrain( float amount )
	{
		var effects = Map.Camera.FindOrCreateHook<ScreenEffects>();
		effects.FilmGrain.Intensity = amount;
	}
}
