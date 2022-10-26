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
	private float CutsceneDuration { get; set; }
	
	/// <summary>
	/// The time since the cutscene was started.
	/// </summary>
	private TimeSince TimeSinceCutsceneStart { get; set; }

	/// <summary>
	/// A list of the entities to cleanup once the cutscene has finished.
	/// </summary>
	private List<AnimatedEntity> EntitiesToCleanup { get; set; } = null!;

	/// <summary>
	/// Start a cutscene from the perspective of an entity with an attachment.
	/// Use a duration of -1 if you don't want it to end automatically.
	/// </summary>
	public void StartCutscene( AnimatedEntity targetEntity, string targetAttachment, float duration = -1.0f )
	{
		// No checking if we're already in a cutscene here, in case we want to
		// move entity

		Camera = new CutsceneCamera() { TargetEntity = targetEntity, TargetAttachment = targetAttachment };

		CutsceneDuration = duration;
		TimeSinceCutsceneStart = 0;

		BlockMovement = true;
		BlockLook = true;
		InCutscene = true;
	}

	/// <summary>
	/// Start a cutscene from the perspective of an entity with an attachment.
	/// Use a duration of -1 if you don't want it to end automatically. Deletes entities once cutscene ends.
	/// </summary>
	public void StartCutsceneWithPostCleanup( AnimatedEntity targetEntity, List<AnimatedEntity> sceneModels, string targetAttachment, float duration = -1.0f )
	{
		StartCutscene( targetEntity, targetAttachment, duration );

		// Store references to the entities we need to clean up once the cutscene ends.
		EntitiesToCleanup = new();
		EntitiesToCleanup.Add( targetEntity );
		EntitiesToCleanup.AddRange( sceneModels );
	}

	/// <summary>
	/// End a cutscene if in one
	/// </summary>
	public void EndCutscene()
	{
		if ( !InCutscene )
			return;

		Camera = new PawnCamera();

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

		if ( CutsceneDuration <= 0f )
			return;

		//
		// Auto-end cutscene
		//
		if ( TimeSinceCutsceneStart > CutsceneDuration )
			EndCutscene();
	}
}
