namespace GvarJam.Player;

public partial class CutsceneCamera : CameraMode
{
	[Net]
	public AnimatedEntity TargetEntity { get; set; }

	[Net]
	public string TargetAttachment { get; set; }

	public override void Update()
	{
		var transform = TargetEntity.GetAttachment( TargetAttachment ) ?? default;

		Position = transform.Position;
		Rotation = transform.Rotation;

		Viewer = null;
	}
}
