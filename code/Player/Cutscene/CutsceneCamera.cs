namespace GvarJam.Player;

public class CutsceneCamera : CameraMode
{
	public CutsceneCamera( AnimatedEntity targetEntity, string targetAttachment )
	{
		TargetEntity = targetEntity;
		TargetAttachment = targetAttachment;
	}

	public AnimatedEntity TargetEntity { get; set; }
	public string TargetAttachment { get; set; }

	public override void Update()
	{
		var transform = TargetEntity.GetAttachment( TargetAttachment ) ?? default;

		Position = transform.Position;
		Rotation = transform.Rotation;

		Viewer = null;
	}
}
