namespace GvarJam.Cameras;

public partial class PawnCamera : CameraMode
{
	[Net, Predicted]
	public ViewModeType ViewMode { get; private set; }
	private float ModeSwitchProgress { get; set; }

	private const float ModeSwitchSpeed = 2.0f;

	private Vector3 lastPos;

	public override void Activated()
	{
		var pawn = Local.Pawn;
		if ( pawn == null ) return;

		Position = pawn.EyePosition;
		Rotation = pawn.EyeRotation;

		lastPos = Position;
	}

	public override void Update()
	{
		var pawn = Local.Pawn;
		if ( pawn == null ) return;

		ModeSwitchProgress += Time.Delta;

		var targetPos = ViewMode switch
		{
			ViewModeType.FirstPerson => UpdateFirstPerson( pawn ),
			ViewModeType.ThirdPerson => UpdateThirdPerson( pawn ),
			_ => (Vector3)0,
		};

		Position = Position.LerpTo( targetPos, ModeSwitchProgress * ModeSwitchSpeed );
		Rotation = pawn.EyeRotation;
		Viewer = null;
	}

	public void GoToFirstPerson()
	{
		if ( ViewMode == ViewModeType.FirstPerson )
			return;

		ViewMode = ViewModeType.FirstPerson;
		ModeSwitchProgress = 0;
	}

	public void GoToThirdPerson()
	{
		if ( ViewMode == ViewModeType.ThirdPerson )
			return;

		ViewMode = ViewModeType.ThirdPerson;
		ModeSwitchProgress = 0;
	}

	private Vector3 UpdateThirdPerson( Entity pawn )
	{
		var targetPos = pawn.EyePosition
			+ pawn.EyeRotation.Backward * 64
			+ pawn.EyeRotation.Right * 16;

		var tr = Trace.Ray( pawn.EyePosition, targetPos ).Ignore( pawn ).Radius( 24f ).Run();
		targetPos = tr.EndPosition;

		return InterpolatePosition( targetPos );
	}

	private Vector3 UpdateFirstPerson( Entity pawn )
	{
		var eyePos = pawn.EyePosition;
		var targetPos = eyePos;

		return InterpolatePosition( targetPos ) + GetAdditiveNoise();
	}

	/// <summary>
	/// Interpolate a position on the z-axis, useful for making things
	/// like crouching look much smoother.
	/// </summary>
	/// <param name="targetPos"></param>
	/// <returns></returns>
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
	/// <returns></returns>
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
	/// <returns></returns>
	private Vector3 GetAdditiveNoise()
	{
		var noise = GetNoise( 0.1f, 8f ) * Rotation.Up
				  + GetNoise( 5.5f, 1f ) * Rotation.Forward
				  + GetNoise( 16f, 8f ) * Rotation.Right;

		return noise * 4f;
	}

	public float GetPlayerAlpha()
	{
		//
		// TODO: It might be worth scaling this by the inverse distance
		// between the player and the camera, so that the third person
		// camera colliding with a wall doesn't fuck us and make the
		// player look really ugly. I will look into this later
		//

		float baseAlpha = ModeSwitchProgress * ModeSwitchSpeed * 4.0f;
		baseAlpha = baseAlpha.Clamp( 0, 1 );

		return ViewMode switch
		{
			ViewModeType.FirstPerson => 1.0f - baseAlpha,
			ViewModeType.ThirdPerson => baseAlpha,
			_ => 1.0f,
		};
	}
}
