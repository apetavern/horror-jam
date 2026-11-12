namespace GvarJam.Interactions;

public abstract class LockedUseComponent : InteractableComponent
{
	private Vector3 _preUsePosition;

	public virtual void SimulateLocked( ColdOpenPlayer user )
	{
	}

	public virtual void StopUsing()
	{
		User = null;
	}

	public virtual void SnapPlayerToUsePosition( ColdOpenPlayer player, float unitsInFront )
	{
		_preUsePosition = player.WorldPosition;

		player.WorldPosition = WorldPosition + WorldRotation.Backward * unitsInFront;
		var lookDir = (WorldPosition - player.WorldPosition).WithZ( 0 );
		if ( !lookDir.IsNearZeroLength )
			player.WorldRotation = Rotation.LookAt( lookDir.Normal, Vector3.Up );

		player.BlockMovement = true;

		if ( player.BodyRenderer.IsValid() )
		{
			player.BodyRenderer.Set( "move_x", 0f );
			player.BodyRenderer.Set( "move_y", 0f );
			player.BodyRenderer.Set( "move_z", 0f );
		}

		player.Velocity = Vector3.Zero;

		var renderer = GetComponent<ModelRenderer>();
		var height = renderer?.Bounds.Size.z ?? 64f;
		var cameraPos = player.EyePosition + Vector3.Up * 12f + player.WorldRotation.Right * 35f + player.WorldRotation.Backward * 25f;
		var cameraRot = Rotation.LookAt( (WorldPosition + Vector3.Up * height / 2f) - cameraPos, Vector3.Up );
		player.Camera.OverrideTransform( new Transform( cameraPos, cameraRot ) );

		GameObject.Tags.Add( "trigger" );
	}

	public virtual void ReleasePlayer( ColdOpenPlayer player )
	{
		player.BlockMovement = false;
		player.Camera.DisableTransformOverride();
		player.WorldPosition = _preUsePosition;
		GameObject.Tags.Remove( "trigger" );
	}
}
