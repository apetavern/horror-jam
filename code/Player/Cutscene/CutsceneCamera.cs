namespace GvarJam.Player;

/// <summary>
/// A camera that follows an entity on its attachment point.
/// </summary>
public sealed partial class CutsceneCamera : CameraMode
{
	/// <summary>
	/// The target entity.
	/// </summary>
	[Net]
	public AnimatedEntity? TargetEntity { get; set; }

	/// <summary>
	/// The target attachment on the <see cref="TargetEntity"/>.
	/// </summary>
	[Net]
	public string? TargetAttachment { get; set; }

	/// <inheritdoc/>
	public override void Update()
	{
		var transform = TargetEntity?.GetAttachment( TargetAttachment ) ?? default;

		Position = transform.Position;
		Rotation = transform.Rotation;

		Viewer = null;
	}
}
