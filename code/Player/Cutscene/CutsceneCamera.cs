namespace GvarJam.Player;

public class CutsceneCamera : CameraMode
{
	public CutsceneCamera( ModelEntity targetEntity, string targetAttachment )
	{
		TargetEntity = targetEntity;
		TargetAttachment = targetAttachment;
	}

	public ModelEntity TargetEntity { get; set; }
	public string TargetAttachment { get; set; }

	public override void Update()
	{
		var transform = TargetEntity.GetAttachment( TargetAttachment ) ?? default;

		Position = transform.Position;
		Rotation = transform.Rotation;

		Viewer = null;
	}
}
