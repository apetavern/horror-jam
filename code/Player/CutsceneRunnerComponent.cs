namespace GvarJam.Player;

public sealed class CutsceneRunnerComponent : Component
{
	private ColdOpenPlayer _player;

	[Sync] public bool InCutscene { get; private set; }
	[Sync] public bool RequiresInputToStart { get; set; }
	[Sync] public bool AwaitingCutsceneInput { get; set; }
	[Sync] public bool HidingPlayers { get; private set; }

	public float CutsceneDuration { get; set; }
	public TimeSince TimeSinceCutsceneStart { get; set; }
	public TimeSince TimeSinceCutsceneEnded { get; set; } = 10f;

	private float _durationAfterDelay;
	private readonly List<GameObject> _entitiesToCleanup = new();

	// Attachment cutscene
	private SkinnedModelRenderer _targetRenderer;
	private string _targetAttachment;
	private readonly List<SkinnedModelRenderer> _additionalRenderers = new();
	private bool _areAnimsPlaying;

	// Manual monster reveal cutscene
	private bool _isInManualCutscene;
	private TimeSince _timeSinceManualStart;
	private Vector3 _manualCamPos;
	private Vector3 _manualLookAt;
	private SplitizenComponent _splitizen;
	private bool _hasSplitizenSplit;

	protected override void OnAwake()
	{
		_player = GetComponent<ColdOpenPlayer>();
	}

	public void StartCutscene( SkinnedModelRenderer targetRenderer, string targetAttachment, float duration = -1f, bool hidePlayers = false )
	{
		_player ??= GetComponent<ColdOpenPlayer>();
		_targetRenderer = targetRenderer;
		_additionalRenderers.Clear();
		_areAnimsPlaying = false;

		CutsceneDuration = duration;
		TimeSinceCutsceneStart = 0;
		RequiresInputToStart = false;
		AwaitingCutsceneInput = false;
		HidingPlayers = hidePlayers;
		_isInManualCutscene = false;

		_player.BlockMovement = true;
		_player.BlockLook = true;
		InCutscene = true;
	}

	public void StartCutsceneWithPostCleanup( SkinnedModelRenderer targetRenderer, List<SkinnedModelRenderer> sceneModels, string targetAttachment, float duration = -1f, bool requiresInputToStart = false, bool hidePlayers = false )
	{
		_player ??= GetComponent<ColdOpenPlayer>();
		_targetRenderer = targetRenderer;
		_targetAttachment = targetAttachment;
		_additionalRenderers.Clear();
		if ( sceneModels is not null )
			_additionalRenderers.AddRange( sceneModels );
		_areAnimsPlaying = false;

		CutsceneDuration = requiresInputToStart ? -1f : duration;
		_durationAfterDelay = duration;
		TimeSinceCutsceneStart = 0;
		RequiresInputToStart = requiresInputToStart;
		AwaitingCutsceneInput = requiresInputToStart;
		HidingPlayers = hidePlayers;
		_isInManualCutscene = false;

		_entitiesToCleanup.Clear();
		if ( targetRenderer is not null )
			_entitiesToCleanup.Add( targetRenderer.GameObject );

		_player.BlockMovement = true;
		_player.BlockLook = true;
		InCutscene = true;
	}

	public void StartManualCutscene( GameObject startObj, float duration = -1f, bool hidePlayers = false )
	{
		_player ??= GetComponent<ColdOpenPlayer>();
		const float distanceToTravel = 1000f;
		var pos = startObj.WorldPosition + startObj.WorldRotation.Forward * distanceToTravel;
		var groundTr = Scene.Trace.Ray( pos, pos + Vector3.Down * 1000f ).WithoutTags( "player", "trigger" ).Run();
		var spawnPos = groundTr.HitPosition + Vector3.Up * 2f;

		foreach ( var old in Scene.GetAllComponents<SplitizenComponent>().ToList() )
			old.GameObject.Destroy();

		var splitGo = new GameObject( true, "CutsceneSplitizen" );
		splitGo.WorldPosition = spawnPos;
		_splitizen = splitGo.AddComponent<SplitizenComponent>();
		_splitizen.StopMoving = true;

		var facingDir = (startObj.WorldPosition - splitGo.WorldPosition).Normal;
		splitGo.WorldRotation = Rotation.LookAt( facingDir, Vector3.Up );

		var lightGo = new GameObject( true, "CutsceneMonsterLight" );
		lightGo.WorldPosition = groundTr.HitPosition + facingDir * 50f;
		lightGo.WorldRotation = Rotation.LookAt( groundTr.HitPosition - lightGo.WorldPosition + Vector3.Up * 60f, Vector3.Up );
		var light = lightGo.AddComponent<SpotLight>();
		light.LightColor = Color.Red * 0.2f;
		light.ConeOuter = 80f;

		_manualCamPos = startObj.WorldPosition;
		_manualLookAt = splitGo.WorldPosition;
		_isInManualCutscene = true;
		_timeSinceManualStart = 0;
		_hasSplitizenSplit = false;
		CutsceneDuration = duration;

		_entitiesToCleanup.Clear();
		_entitiesToCleanup.Add( lightGo );

		TimeSinceCutsceneStart = 0;
		RequiresInputToStart = false;
		AwaitingCutsceneInput = false;
		HidingPlayers = hidePlayers;

		_player.BlockMovement = true;
		_player.BlockLook = true;
		InCutscene = true;
	}

	public void EndCutscene()
	{
		if ( !InCutscene ) return;
		_player ??= GetComponent<ColdOpenPlayer>();

		_player.Camera.DisableTransformOverride();
		HidingPlayers = false;
		_player.BlockMovement = false;
		_player.BlockLook = false;
		InCutscene = false;
		_isInManualCutscene = false;
		_targetRenderer = null;
		TimeSinceCutsceneEnded = 0;

		foreach ( var go in _entitiesToCleanup )
		{
			if ( go.IsValid() )
				go.Destroy();
		}
		_entitiesToCleanup.Clear();
	}

	protected override void OnUpdate()
	{
		if ( !InCutscene ) return;

		if ( RequiresInputToStart && Input.Released( "Jump" ) )
		{
			CutsceneDuration = _durationAfterDelay;
			TimeSinceCutsceneStart = 0;
			RequiresInputToStart = false;
			AwaitingCutsceneInput = false;
		}

		if ( _isInManualCutscene )
		{
			if ( _timeSinceManualStart > 2f && !_hasSplitizenSplit )
			{
				_splitizen?.DoSplit();
				_hasSplitizenSplit = true;
			}

			var targetPos = _manualLookAt + Vector3.Up * 64f;
			if ( Vector3.DistanceBetween( _manualCamPos, targetPos ) >= 100f )
			{
				var dir = targetPos - _manualCamPos;
				_manualCamPos += dir * _timeSinceManualStart / 1000f;
			}
			var rot = Rotation.LookAt( targetPos - _manualCamPos, Vector3.Up ) * Rotation.FromRoll( 10f * _timeSinceManualStart );
			_player.Camera.OverrideTransform( new Transform( _manualCamPos, rot ) );
		}
		else if ( _targetRenderer.IsValid() )
		{
			var attachObj = _targetRenderer.GetAttachmentObject( _targetAttachment );
			if ( attachObj is not null )
				_player.Camera.OverrideTransform( attachObj.WorldTransform );

			if ( _targetRenderer.AnimationGraph is null )
			{
				if ( AwaitingCutsceneInput )
				{
					if ( _targetRenderer.Sequence.Name != "waiting" && _targetRenderer.Sequence.SequenceNames.Contains( "waiting" ) )
						_targetRenderer.Sequence.Name = "waiting";
				}
				else if ( !_areAnimsPlaying )
				{
					var hitSeq = _targetRenderer.Sequence.SequenceNames.FirstOrDefault( n => n.Contains( "Hit", StringComparison.OrdinalIgnoreCase ) )
						?? _targetRenderer.Sequence.SequenceNames.FirstOrDefault();
					if ( !string.IsNullOrEmpty( hitSeq ) )
					{
						_targetRenderer.Sequence.Name = hitSeq;
						_targetRenderer.Sequence.Looping = false;
						_targetRenderer.Sequence.PlaybackRate = hitSeq.Equals( "Hitbutton", StringComparison.OrdinalIgnoreCase ) ? 1.5f : 1.0f;
					}
					foreach ( var r in _additionalRenderers )
					{
						if ( r.IsValid() ) r.Set( "Open", true );
					}
					_areAnimsPlaying = true;
				}
				else if ( _targetRenderer.Sequence.IsFinished && _targetRenderer.Sequence.Name != "Fallout" && _targetRenderer.Sequence.SequenceNames.Contains( "Fallout" ) )
				{
					_targetRenderer.Sequence.Name = "Fallout";
					_targetRenderer.Sequence.Looping = false;
					_targetRenderer.Sequence.PlaybackRate = 1.75f;
				}
			}
			else if ( !_areAnimsPlaying && !AwaitingCutsceneInput )
			{
				_targetRenderer.Set( "Open", true );
				foreach ( var r in _additionalRenderers )
				{
					if ( r.IsValid() ) r.Set( "Open", true );
				}
				_areAnimsPlaying = true;
			}
		}

		if ( CutsceneDuration > 0f && TimeSinceCutsceneStart > CutsceneDuration )
			EndCutscene();
	}
}
