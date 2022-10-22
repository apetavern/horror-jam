namespace GvarJam.Cameras;

public partial class ControllableCamera : CameraMode
{
	[Net]
	public Entity? ControlFromEntity { get; set; }

	public CameraMode SetupFromEntity( Entity entity )
	{
		ControlFromEntity = entity;

		Position = entity.Position;
		Rotation = entity.Rotation;

		return this;
	}

	public override void Update()
	{
		if ( ControlFromEntity is null )
			return;

		Position = ControlFromEntity.Position;
		Rotation = ControlFromEntity.Rotation;
	}
}
