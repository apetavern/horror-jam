namespace GvarJam.Player;

partial class Pawn
{
	/// <summary>
	/// Is the player currently in a cutscene? Use <see cref="StartCutscene(ModelEntity, string)"/> to set this.
	/// </summary>
	public bool InCutscene { get; private set; } = false;

	private float CutsceneDuration { get; set; }
	private TimeSince TimeSinceCutsceneStart { get; set; }

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

	/// <summary>
	/// Start a cutscene from the perspective of an entity with an attachment.
	/// Use a duration of -1 if you don't want it to end automatically.
	/// </summary>
	public void StartCutscene( AnimatedEntity targetEntity, string targetAttachment, float duration = -1.0f )
	{
		// No checking if we're already in a cutscene here, in case we want to
		// move entity

		Camera = new CutsceneCamera( targetEntity, targetAttachment );

		CutsceneDuration = duration;
		TimeSinceCutsceneStart = 0;

		BlockMovement = true;
		InCutscene = true;
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
		InCutscene = false;
	}

	[ConCmd.Client( "test_cutscene_camera" )]
	public static void TestCutsceneCamera()
	{
		var caller = ConsoleSystem.Caller.Pawn;
		if ( caller is not Pawn pawn )
			return;

		var nearestCamera = Entity.All.OfType<MountedCamera>().GetClosestOrDefault( pawn );
		pawn.StartCutscene( nearestCamera, "lens_position" );
	}

	[ConCmd.Client( "test_cutscene_end" )]
	public static void TestCutsceneEnd()
	{
		var caller = ConsoleSystem.Caller.Pawn;
		if ( caller is not Pawn pawn )
			return;

		pawn.EndCutscene();
	}
}
