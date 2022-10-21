namespace GvarJam
{
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
		float LeanMax => 0.015f;
		float LeanMul => 0.005f;
		float LeanDegrees => 12f;
		float LeanSmooth => 15.0f;
		float MinSpeed => 0f;
		float MaxSpeed => 320f;
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
			Roll = Roll.LerpTo( Velocity.Dot( camSetup.Rotation.Right ) * LeanMul, Time.Delta * LeanSmooth );
			Roll.Clamp( -LeanMax, LeanMax );
			Roll += MathF.Sin( Bobbing / 2f ) * LeanMul * LeanDegrees;

			//
			// Camera FOV
			//
			FOV = FOV.LerpTo( speed * FOVIncrease * MathF.Abs( forwardSpeed ), Time.Delta * FOVSpeed );

			//
			// Apply everything
			//
			camSetup.Position += up * MathF.Sin( Bobbing ) * speed * 2;
			camSetup.Position += left * MathF.Sin( Bobbing * 0.5f ) * speed * 1;
			camSetup.Rotation *= Rotation.From( 0, 0, Roll );
			camSetup.FieldOfView += FOV;
		}
	}
}
