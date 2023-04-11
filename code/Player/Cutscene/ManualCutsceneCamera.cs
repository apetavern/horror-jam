namespace GvarJam.Player;

/// <summary>
/// A camera that is controlled programmatically 
/// </summary>
public sealed partial class ManualCutsceneCamera : CameraComponent
{
	/// <summary>
	/// The starting position of the camera.
	/// </summary>
	[Net]
	public Vector3 StartPosition { get; set; }

	/// <summary>
	/// The world position to look at during the cutscene.
	/// </summary>
	[Net]
	public Vector3 LookAtPosition { get; set; }

	/// <summary>
	/// The distance to travel in the cutscene.
	/// </summary>
	[Net]
	public float TravelDistance { get; set; }

	/// <summary>
	/// The speed to travel at in the cutscene.
	/// </summary>
	[Net]
	public float TravelSpeed { get; set; }
	
	/// <summary>
	/// The time since the cutscene started.
	/// </summary>
	private TimeSince TimeSinceCutsceneStarted { get; set; }

	/// <inheritdoc/>
	protected override void OnActivate()
	{
		Camera.Position = StartPosition;
		// Camera.Rotation = Rotation; // NOTE(gio): not sure about this!

		TimeSinceCutsceneStarted = 0;
	}

	/// <inheritdoc/>
	public override void Update()
	{
		var targetPosition = LookAtPosition + Vector3.Up * 64;

		if ( Vector3.DistanceBetween( Camera.Position, targetPosition ) < 100 )
			return;

		var directionOfTravel = targetPosition - Camera.Position;
		Camera.Position += directionOfTravel * TimeSinceCutsceneStarted / 1000;

		Camera.Rotation = Rotation.LookAt( targetPosition - Camera.Position );
		Camera.Rotation *= Rotation.FromRoll( 10 * TimeSinceCutsceneStarted );
		
		Camera.FirstPersonViewer = null;
	}
}
