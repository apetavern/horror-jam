namespace GvarJam.HammerEntities;

public sealed class PlayerLockerComponent : LockedUseComponent
{
	public override string LegacyTypeName => "PlayerLocker";
	[Sync] public TimeSince TimeSinceUsed { get; set; } = 10f;

	protected override void OnStart()
	{
		base.OnStart();
		DisplayName = "Hiding Locker";

		var model = Model.Load( "models/slimplayerlocker/slimplayerlocker.vmdl" );
		var mr = GameObject.GetOrAddComponent<ModelRenderer>();
		mr.Model = model;

		var col = GameObject.GetOrAddComponent<ModelCollider>();
		col.Model = model;
	}

	public override void SimulateLocked( ColdOpenPlayer user )
	{
		if ( user is not null )
		{
			var baseAngles = WorldRotation.Angles();
			var deltaYaw = MathX.DeltaDegrees( baseAngles.yaw, user.ViewAngles.yaw ).Clamp( -75f, 75f );
			var clampedPitch = user.ViewAngles.pitch.Clamp( -25f, 35f );
			user.ViewAngles = new Angles( clampedPitch, baseAngles.yaw + deltaYaw, 0f );
			var cameraPos = WorldPosition + Vector3.Up * 64f + WorldRotation.Forward.Normal * 54f;
			user.Camera.OverrideTransform( new Transform( cameraPos, user.ViewAngles.ToRotation() ) );
		}

		if ( TimeSinceUsed < 1f ) return;

		if ( Input.Released( "Use" ) )
			StopUsing();
	}

	protected override void OnDestroy()
	{
		StopUsing();
		base.OnDestroy();
	}

	protected override void OnUsed( ColdOpenPlayer user )
	{
		base.OnUsed( user );
		user.Interaction.InteractedEntity = this;
		SnapPlayerToUsePosition( user, 0f );
		TimeSinceUsed = 0;
		User = user;
	}

	public override void SnapPlayerToUsePosition( ColdOpenPlayer player, float unitsInFront )
	{
		base.SnapPlayerToUsePosition( player, unitsInFront );
		player.WorldRotation = Rotation.LookAt( WorldRotation.Forward.Normal, Vector3.Up );
		player.ViewAngles = WorldRotation.Angles();

		var cameraPos = WorldPosition + Vector3.Up * 64f + WorldRotation.Forward.Normal * 54f;
		player.Camera.OverrideTransform( new Transform( cameraPos, WorldRotation ) );
	}

	public override void StopUsing()
	{
		if ( User is not null && User.IsValid() )
		{
			User.Interaction.InteractedEntity = null;
			ReleasePlayer( User );
		}
		User = null;
	}

	public override bool IsUsable( ColdOpenPlayer user )
	{
		return User is null && base.IsUsable( user );
	}

	public override bool OnUse( ColdOpenPlayer user )
	{
		OnUsed( user );
		return true;
	}
}
