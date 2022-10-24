namespace GvarJam.Player;

//
// Camera effects (leaning, bobbing, etc.) that rely on player movement
// and player properties go here rather than directly in the camera class.
// This helps to separate things out & reduce co-dependencies between the
// two
//
partial class Pawn
{
	float Bobbing { get; set; }
	float Roll { get; set; }
	float FOV { get; set; }

	//
	//
	//
	float LeanDegrees => 0.5f;
	float LeanMax => LeanDegrees * 2f;
	float LeanSmooth => 15.0f;
	float MinSpeed => 0f;
	float MaxSpeed => Controller?.SprintSpeed ?? 320f;
	float FOVIncrease => 5f;
	float FOVSpeed => 10f;
	float BobStrength => 25f;

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
}
