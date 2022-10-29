namespace GvarJam.Player;

/// <summary>
/// A camera that is controlled programmatically 
/// </summary>
public sealed partial class ManualCutsceneCamera : CameraMode
{
	[Net]
	public Vector3 StartPosition { get; set; }

	[Net]
	public Vector3 LookAtPosition { get; set; }

	[Net]
	public float TravelDistance { get; set; }

	[Net]
	public float TravelSpeed { get; set; }

	private TimeSince TimeSinceCutsceneStarted { get; set; }

	public override void Activated()
	{
		base.Activated();

		Position = StartPosition;
		Rotation = Rotation;

		TimeSinceCutsceneStarted = 0;
	}

	/// <inheritdoc/>
	public override void Update()
	{
		var targetPosition = LookAtPosition + Vector3.Up * 64;

		if ( Vector3.DistanceBetween( Position, targetPosition ) < 100 )
			return;

		var directionOfTravel = targetPosition - Position;
		Position += directionOfTravel * TimeSinceCutsceneStarted / 1000;

		Rotation = Rotation.LookAt( targetPosition - Position );
		Rotation *= Rotation.FromRoll( 10 * TimeSinceCutsceneStarted );
		
		Viewer = null;
	}
}
