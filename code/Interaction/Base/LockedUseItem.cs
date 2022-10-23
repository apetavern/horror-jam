namespace GvarJam.Interactions;

public partial class LockedUseItem : InteractableEntity
{
	public virtual void StopUsing()
	{
		User = null;
	}

	public virtual void SnapPlayerToUsePosition( Pawn player, float numberOfUnitsInfront )
	{
		player.Position = Position + Rotation.Backward * numberOfUnitsInfront;
		player.Rotation = Rotation.From( Vector3.VectorAngle( Position - player.Position ) );

		player.BlockMovement = true;

		// Stop animation playback
		player.SetAnimParameter( "move_x", 0 );
		player.SetAnimParameter( "move_y", 0 );
		player.SetAnimParameter( "move_z", 0 );

		// Stop camera bob
		player.Velocity = 0;

		if ( player.Camera is PawnCamera camera )
		{
			var cameraPos = player.EyePosition + Vector3.Up * 12 + player.Rotation.Right * 35 + player.Rotation.Backward * 25;
			var cameraRot = Rotation.LookAt( (Position + Vector3.Up * Model.RenderBounds.Size.z / 2) - cameraPos, Vector3.Up );

			camera.OverrideTransform( new Transform() { Position = cameraPos, Rotation = cameraRot } );
		}


		player.ResetInterpolation();
	}

	public virtual void ReleasePlayer( Pawn player )
	{
		player.BlockMovement = false;

		if ( player.Camera is PawnCamera camera )
			camera.DisableTransformOverride();
	}

	public virtual void Simulate() { }
}

