using Sandbox;

namespace GvarJam.Player;

partial class Pawn
{
	/// <summary>
	/// Is the player currently in a cutscene? Use <see cref="StartCutscene(AnimatedEntity, string, float)"/> to set this.
	/// </summary>
	[Net]
	public bool InCutscene { get; private set; } = false;

	/// <summary>
	/// The duration of the current cutscene in seconds.
	/// </summary>
	public float CutsceneDuration { get; set; }

	/// <summary>
	/// The time since the cutscene was started.
	/// </summary>
	public TimeSince TimeSinceCutsceneStart { get; set; }

	/// <summary>
	/// A list of the entities to cleanup once the cutscene has finished.
	/// </summary>
	private List<Entity> EntitiesToCleanup { get; set; } = null!;

	[Net]
	public bool RequiresInputToStart { get; set; }

	[Net]
	public bool AwaitingCutsceneInput { get; set; }

	[Net, Local]
	private bool HidingPlayers { get; set; }

	private float DurationAfterDelay { get; set; }

	[Net]
	private bool IsInManualCutscene { get; set; }

	[Net]
	private TimeSince TimeSinceManualCutsceneStart { get; set; }

	[Net]
	private Splitizen SplitizenEntity { get; set; }

	[Net]
	private bool HasSplitizenSplit { get; set; }

	/// <summary>
	/// Start a cutscene from the perspective of an entity with an attachment.
	/// Use a duration of -1 if you don't want it to end automatically.
	/// </summary>
	public void StartCutscene( AnimatedEntity targetEntity, string targetAttachment, float duration = -1.0f, bool hidePlayers = false )
	{
		// No checking if we're already in a cutscene here, in case we want to
		// move entity

		Camera = new CutsceneCamera() { TargetEntity = targetEntity, TargetAttachment = targetAttachment };

		CutsceneDuration = duration;
		TimeSinceCutsceneStart = 0;
		RequiresInputToStart = false;
		HidingPlayers = hidePlayers;

		BlockMovement = true;
		BlockLook = true;
		InCutscene = true;
		IsInManualCutscene = false;
	}

	public void StartManualCutscene( Entity startEntity, float duration = -1.0f, bool hidePlayers = false )
	{
		var distanceToTravel = 1000;

		// Spawn baddy
		var position = startEntity.Position + startEntity.Rotation.Forward * distanceToTravel;
		var groundPos = Trace.Ray( position, position + Vector3.Down * 1000 ).WorldOnly().Run();

		var oldSplitizen = All.OfType<Splitizen>().FirstOrDefault();
		oldSplitizen?.Delete();

		SplitizenEntity = new Splitizen() { Position = groundPos.HitPosition, StopMoving = true };
		
		var facingDirection = (startEntity.Position - SplitizenEntity.Position).Normal;

		SplitizenEntity.Rotation = Rotation.LookAt( facingDirection );

		var light = new SpotLightEntity();
		light.Position = groundPos.HitPosition + facingDirection * 50;
		light.Rotation = Rotation.LookAt( groundPos.HitPosition - light.Position + Vector3.Up * 60 );
		light.Color = Color.White;
		light.Brightness = 0.2f;
		light.OuterConeAngle = 30;
		light.Enabled = true;

		Camera = new ManualCutsceneCamera() { StartPosition = startEntity.Position, LookAtPosition = SplitizenEntity.Position };

		IsInManualCutscene = true;
		TimeSinceManualCutsceneStart = 0;

		CutsceneDuration = duration;

		EntitiesToCleanup = new();
		EntitiesToCleanup.Add( light );

		TimeSinceCutsceneStart = 0;
		RequiresInputToStart = false;
		HidingPlayers = hidePlayers;

		BlockMovement = true;
		BlockLook = true;
		InCutscene = true;
	}

	/// <summary>
	/// Start a cutscene from the perspective of an entity with an attachment.
	/// Use a duration of -1 if you don't want it to end automatically. Deletes entities once cutscene ends.
	/// </summary>
	public void StartCutsceneWithPostCleanup( AnimatedEntity targetEntity, List<AnimatedEntity> sceneModels, string targetAttachment, float duration = -1.0f, bool requiresInputToStart = false, bool hidePlayers = false )
	{
		Camera = new CutsceneCamera() { TargetEntity = targetEntity, TargetAttachment = targetAttachment, AdditionalEntities = sceneModels, AwaitingInput = requiresInputToStart };

		CutsceneDuration = duration;
		TimeSinceCutsceneStart = 0;

		RequiresInputToStart = requiresInputToStart;
		HidingPlayers = hidePlayers;

		BlockMovement = true;
		BlockLook = true;
		InCutscene = true;
		AwaitingCutsceneInput = requiresInputToStart;

		if ( RequiresInputToStart )
		{
			CutsceneDuration = -1;
			DurationAfterDelay = duration;
		}

		// Store references to the entities we need to clean up once the cutscene ends.
		EntitiesToCleanup = new();
		EntitiesToCleanup.Add( targetEntity );

		IsInManualCutscene = false;

		// Commented out because we want to keep hold of the created items for now. If this changes we need to rethink it.
		// EntitiesToCleanup.AddRange( sceneModels );
	}

	/// <summary>
	/// End a cutscene if in one
	/// </summary>
	public void EndCutscene()
	{
		if ( !InCutscene )
			return;

		Camera = new PawnCamera();
		HidingPlayers = false;

		BlockMovement = false;
		BlockLook = false;
		InCutscene = false;

		// Cleanup the cutscene entities
		if ( EntitiesToCleanup is null )
			return;

		foreach ( var ent in EntitiesToCleanup )
			ent.Delete();
	}

	/// <summary>
	/// Simulates the pawns end of the cutscene system.
	/// </summary>
	private void SimulateCutscenes()
	{
		if ( !InCutscene )
			return;

		if( RequiresInputToStart && Input.Released( InputButton.Jump ) )
		{
			(Camera as CutsceneCamera)!.AwaitingInput = false;
			CutsceneDuration = DurationAfterDelay;
			TimeSinceCutsceneStart = 0;
			RequiresInputToStart = false;

			AwaitingCutsceneInput = false;
		}

		if( IsInManualCutscene )
		{
			if ( TimeSinceManualCutsceneStart > 2 && !HasSplitizenSplit )
			{
				SplitizenEntity?.DoSplit();
				HasSplitizenSplit = true;
			}
		}

		if ( CutsceneDuration <= 0f )
			return;

		//
		// Auto-end cutscene
		//
		if ( TimeSinceCutsceneStart > CutsceneDuration )
			EndCutscene();
	}
}
