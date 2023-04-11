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
	// TODO(gio): fix sprint speed
	private float MaxSpeed => 320f;

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
	private const float BobStrength = 15f;

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

	[Event.Client.PostCamera]
	public void PostCameraSetup()
	{
		var speed = Velocity.Length.LerpInverse( MinSpeed, MaxSpeed );
		var forwardSpeed = Velocity.Normal.Dot( Sandbox.Camera.Rotation.Forward );

		var left = Sandbox.Camera.Rotation.Left;
		var up = Sandbox.Camera.Rotation.Up;

		//
		// Camera bob (up/down)
		//
		if ( GroundEntity != null )
			Bobbing += Time.Delta * BobStrength * speed;

		//
		// Camera roll
		//
		var targetRoll = Velocity.Dot( Sandbox.Camera.Rotation.Right ) / 180f;
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
		float x = MathF.Sin( Bobbing * 0.5f ) * speed;
		float y = MathF.Sin( Bobbing ) * speed;
		Sandbox.Camera.Position += left * x;
		Sandbox.Camera.Position += up * y;
		Sandbox.Camera.Rotation *= Rotation.From( 0, 0, Roll );
		Sandbox.Camera.FieldOfView += FOV;

		var tx = new PanelTransform();
		tx.AddTranslate( x, y );

		Game.RootPanel.Style.Transform = tx;
		Game.RootPanel.Style.Dirty();
	}
}
