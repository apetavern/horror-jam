namespace GvarJam.HammerEntities;

/// <summary>
/// A locker that a player can hide in.
/// </summary>
[Category( "Environment" )]
[Library( "ent_playerlocker" )]
[HammerEntity]
[EditorModel( "models/slimplayerlocker/slimplayerlocker.vmdl" )]
public sealed partial class PlayerLocker : LockedUseItem
{
	/// <summary>
	/// The time since this controller was interacted with.
	/// </summary>
	[Net, Predicted]
	public TimeSince TimeSinceUsed { get; set; }

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		DisplayName = "Hiding Locker";

		SetModel( "models/slimplayerlocker/slimplayerlocker.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}

	/// <inheritdoc/>
	public override void Simulate( Client cl )
	{
		if ( TimeSinceUsed < 1 )
			return;

		if ( Input.Released( InputButton.Use ) )
			StopUsing();
	}

	/// <inheritdoc/>
	protected override void OnDestroy()
	{
		base.OnDestroy();

		StopUsing();
	}

	/// <inheritdoc/>
	protected override void OnUsed( Entity user )
	{
		base.OnUsed( user );

		if ( user is not Pawn player )
			return;

		player.InteractedEntity = this;
		SnapPlayerToUsePosition( player, 0 );
		
		TimeSinceUsed = 0;

		if ( IsServer )
			User = player;
	}

	/// <inheritdoc/>
	public override void SnapPlayerToUsePosition( Pawn player, float numberOfUnitsInfront )
	{
		base.SnapPlayerToUsePosition( player, numberOfUnitsInfront );

		player.Rotation = Rotation.LookAt( Rotation.Forward.Normal );
		if ( player.Camera is PawnCamera camera )
		{
			var cameraPos = player.EyePosition + Rotation.Forward.Normal * 100;
			var cameraRot = Rotation.LookAt( (cameraPos - Position).Normal );

			camera.OverrideTransform( new Transform() { Position = cameraPos, Rotation = cameraRot.Inverse } );
		}
	}

	/// <inheritdoc/>
	public override void StopUsing()
	{
		var player = User as Pawn;

		if ( player is not null )
		{
			player.InteractedEntity = null;
			ReleasePlayer( player );
		}

		User = null;
	}

	/// <inheritdoc/>
	public override bool IsUsable( Entity user )
	{
		return base.IsUsable( user ) && User is null;
	}

	/// <inheritdoc/>
	public override bool OnUse( Entity user )
	{
		base.OnUse( user );

		return true;
	}
}
