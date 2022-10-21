namespace GvarJam;

public partial class Camera : CameraMode
{
	[Net, Predicted]
	public ViewModeType ViewMode { get; private set; }

	Vector3 lastPos;
	float modeSwitchProgress;
	float ModeSwitchSpeed => 2.0f;

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

		DebugOverlay.ScreenText( ViewMode.ToString(), 0 );
		modeSwitchProgress += Time.Delta;

		var targetPos = ViewMode switch
		{
			ViewModeType.FirstPerson => UpdateFirstPerson( pawn ),
			ViewModeType.ThirdPerson => UpdateThirdPerson( pawn ),
			_ => (Vector3)0,
		};

		Position = Position.LerpTo( targetPos, modeSwitchProgress * ModeSwitchSpeed );

		Rotation = pawn.EyeRotation;
		lastPos = Position;
		Viewer = null;
	}

	public void GoToFirstPerson()
	{
		if ( ViewMode == ViewModeType.FirstPerson )
			return;

		ViewMode = ViewModeType.FirstPerson;
		modeSwitchProgress = 0;
	}

	public void GoToThirdPerson()
	{
		if ( ViewMode == ViewModeType.ThirdPerson )
			return;

		ViewMode = ViewModeType.ThirdPerson;
		modeSwitchProgress = 0;
	}

	//
	//
	//
	private Vector3 UpdateThirdPerson( Entity pawn )
	{
		var targetPos = pawn.EyePosition
			+ pawn.EyeRotation.Backward * 64
			+ pawn.EyeRotation.Right * 16;

		var tr = Trace.Ray( pawn.EyePosition, targetPos ).Ignore( pawn ).Radius( 24f ).Run();
		return tr.EndPosition;
	}

	//
	//
	//
	private Vector3 UpdateFirstPerson( Entity pawn )
	{
		var eyePos = pawn.EyePosition;
		var targetPos = eyePos;

		return targetPos + GetAdditiveNoise();
	}

	//
	// Returns time-based noise, normalized from -1 to 1.
	//
	private float GetNoise( float offset, float speed )
	{
		var noise = Noise.Perlin( offset, offset, Time.Now * speed );
		noise -= 0.5f;
		noise *= 2f;

		return noise;
	}

	//
	// Provides noise for a 'breathing' effect.
	// We could potentially multiply this by some factor later
	// depending on how close a player is to the monster
	// (i.e. some sort of 'sanity' value)
	// 
	private Vector3 GetAdditiveNoise()
	{
		var noise = GetNoise( 0.1f, 16f ) * Vector3.Up
				  + GetNoise( 0.5f, 1f ) * Vector3.Forward
				  + GetNoise( 0.9f, 8f ) * Vector3.Right;

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

		float baseAlpha = modeSwitchProgress * ModeSwitchSpeed * 4.0f;
		baseAlpha = baseAlpha.Clamp( 0, 1 );

		return ViewMode switch
		{
			ViewModeType.FirstPerson => 1.0f - baseAlpha,
			ViewModeType.ThirdPerson => baseAlpha,
			_ => 1.0f,
		};
	}
}
