namespace GvarJam;

public class Camera : CameraMode
{
	enum ViewMode
	{
		FirstPerson,
		ThirdPerson
	}

	Vector3 lastPos;
	ViewMode viewMode;

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

		DebugOverlay.ScreenText( $"{viewMode}" );
		modeSwitchProgress += Time.Delta;

		var targetPos = viewMode switch
		{
			ViewMode.FirstPerson => UpdateFirstPerson( pawn ),
			ViewMode.ThirdPerson => UpdateThirdPerson( pawn ),
			_ => (Vector3)0,
		};

		Position = Position.LerpTo( targetPos, modeSwitchProgress * ModeSwitchSpeed );

		Rotation = pawn.EyeRotation;
		lastPos = Position;
		Viewer = null;
	}

	//
	//
	//
	private Vector3 UpdateThirdPerson( Entity pawn )
	{
		var targetPos = pawn.EyePosition
			+ pawn.EyeRotation.Backward * 64
			+ pawn.EyeRotation.Right * 16;

		return targetPos;
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

	public override void BuildInput( InputBuilder input )
	{
		base.BuildInput( input );

		if ( input.Pressed( InputButton.View ) )
		{
			viewMode = (viewMode == ViewMode.FirstPerson) ? ViewMode.ThirdPerson : ViewMode.FirstPerson;
			modeSwitchProgress = 0;
		}
	}

	public float GetPlayerAlpha()
	{
		float baseAlpha = modeSwitchProgress * ModeSwitchSpeed * 4.0f;
		baseAlpha = baseAlpha.Clamp( 0, 1 );

		return viewMode switch
		{
			ViewMode.FirstPerson => 1.0f - baseAlpha,
			ViewMode.ThirdPerson => baseAlpha,
			_ => 1.0f,
		};
	}
}
