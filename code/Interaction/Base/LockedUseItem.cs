namespace GvarJam.Interactions;

public partial class LockedUseItem : InteractableEntity
{
	public virtual void StopUsing()
	{
		User = null;
	}

	public virtual void SnapPlayerToUsePosition( Pawn player )
	{
		player.Position = Position - Rotation.Forward * Model.RenderBounds.Size.y;
		player.Rotation = Rotation.From( Vector3.VectorAngle( Position - player.Position ) );
		player.EyeRotation = player.Rotation;

		player.Controller = null;

		// Stop animation playback
		player.SetAnimParameter( "move_x", 0 );
		player.SetAnimParameter( "move_y", 0 );
		player.SetAnimParameter( "move_z", 0 );

		// Stop camera bob
		player.Velocity = 0;

		player.ResetInterpolation();
	}

	public virtual void ReleasePlayer ( Pawn player )
	{
		player.Controller = new WalkController();
	}

	public virtual void Simulate() { }
}

