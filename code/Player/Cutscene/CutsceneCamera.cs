namespace GvarJam.Player;

/// <summary>
/// A camera that follows an entity on its attachment point.
/// </summary>
public sealed partial class CutsceneCamera : CameraComponent
{
	/// <summary>
	/// The target entity.
	/// </summary>
	[Net]
	public AnimatedEntity? TargetEntity { get; set; }

	/// <summary>
	/// Any additional entities involved in the cutscene.
	/// </summary>
	[Net]
	public List<AnimatedEntity> AdditionalEntities { get; set; } = new();

	/// <summary>
	/// Whether or not animations are playing for the entities.
	/// </summary>
	public bool AreAnimsPlaying { get; set; }

	/// <summary>
	/// Whether or not the cutscene is waiting for input from a player.
	/// </summary>
	[Net]
	public bool AwaitingInput { get; set; }

	/// <summary>
	/// The target attachment on the <see cref="TargetEntity"/>.
	/// </summary>
	[Net]
	public string? TargetAttachment { get; set; }

	/// <inheritdoc/>
	public override void Update()
	{
		var transform = TargetEntity?.GetAttachment( TargetAttachment ) ?? default;

		Camera.Position = transform.Position;
		Camera.Rotation = transform.Rotation;

		if( !AreAnimsPlaying && !AwaitingInput )
		{
			TargetEntity?.SetAnimParameter( "Open", true );

			foreach ( var ent in AdditionalEntities )
			{
				ent.SetAnimParameter( "Open", true );
			}

			AreAnimsPlaying = true;
		}
		
		Camera.FirstPersonViewer = null;
	}
}
