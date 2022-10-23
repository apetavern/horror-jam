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

		player.ResetInterpolation();
	}

	public virtual void Simulate() { }
}

