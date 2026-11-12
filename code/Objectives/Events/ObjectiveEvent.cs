namespace GvarJam.Objectives;

public struct ObjectiveEvent
{
	public EventType Type { get; set; }

	[ShowIf( nameof( Type ), EventType.PlaySound ), ResourceType( "sound" )]
	public string SoundName { get; set; }

	[ShowIf( nameof( Type ), EventType.SetLighting )]
	public bool LightsEnabled { get; set; }

	[ShowIf( nameof( Type ), EventType.SetDoorLockState )]
	public string DoorName { get; set; }

	[ShowIf( nameof( Type ), EventType.SetDoorLockState )]
	public bool DoorLocked { get; set; }

	[ShowIf( nameof( Type ), EventType.PlayCutscene )]
	public string TargetEntityName { get; set; }

	[ShowIf( nameof( Type ), EventType.PlayInstantiatedCutscene )]
	public string InfoTarget { get; set; }

	[ShowIf( nameof( Type ), EventType.PlayInstantiatedCutscene ), ResourceType( "vmdl" )]
	public string TargetModel { get; set; }

	[ShowIf( nameof( Type ), EventType.PlayInstantiatedCutscene ), ResourceType( "vmdl" )]
	public List<string> AdditionalSceneModels { get; set; }

	[ShowIf( nameof( Type ), EventType.PlayInstantiatedCutscene )]
	public string TargetAttachment { get; set; }

	[ShowIf( nameof( Type ), EventType.PlayInstantiatedCutscene )]
	public bool InputRequiredToStart { get; set; }

	[ShowIf( nameof( Type ), EventType.PlayInstantiatedCutscene )]
	public float Duration { get; set; }

	[ShowIf( nameof( Type ), EventType.PlayInstantiatedCutscene )]
	public bool HidePlayers { get; set; }

	[ShowIf( nameof( Type ), EventType.PlayMonsterIntroductionCutscene )]
	public string MonsterCameraToUse { get; set; }

	[ShowIf( nameof( Type ), EventType.PlayMonsterIntroductionCutscene )]
	public float MonsterCutsceneDuration { get; set; }

	public void Invoke( ColdOpenPlayer player )
	{
		var scene = player.Scene;
		switch ( Type )
		{
			case EventType.PlaySound:
				if ( !string.IsNullOrEmpty( SoundName ) )
					AudioDirector.Play2D( SoundName );
				break;

			case EventType.PlayCutscene:
			{
				var targetEntityName = TargetEntityName ?? "";
				var targetGo = scene.GetAllObjects( true ).FirstOrDefault( x => x.Name.Contains( targetEntityName, StringComparison.OrdinalIgnoreCase ) );
				var renderer = targetGo?.GetComponent<SkinnedModelRenderer>();
				if ( renderer is null )
				{
					Log.Warning( $"Failed to find target cutscene entity {TargetEntityName}" );
					return;
				}
				player.Cutscenes.StartCutscene( renderer, TargetAttachment, Duration, HidePlayers );
				break;
			}

			case EventType.PlayInstantiatedCutscene:
			{
				var infoTargetName = InfoTarget ?? "";
				var infoTarget = scene.GetAllObjects( true ).FirstOrDefault( x => x.Name.Contains( infoTargetName, StringComparison.OrdinalIgnoreCase ) );
				if ( infoTarget is null )
				{
					Log.Warning( $"Failed to find info_target {InfoTarget}" );
					return;
				}

				Log.Info( $"[Cutscene] Spawning {TargetModel} at {infoTarget.Name}" );
				var mainGo = new GameObject( false, $"Cutscene_{TargetModel}" );
				mainGo.WorldTransform = infoTarget.WorldTransform;
				var mainRenderer = mainGo.AddComponent<SkinnedModelRenderer>();
				mainRenderer.Model = Model.Load( TargetModel );
				mainRenderer.CreateAttachments = true;
				mainGo.Enabled = true;
				Log.Info( $"[Cutscene] Loaded main model {TargetModel}" );

				var sceneModels = new List<SkinnedModelRenderer>();
				if ( AdditionalSceneModels is not null )
				{
					foreach ( var modelPath in AdditionalSceneModels )
					{
						var extraGo = new GameObject( false, $"CutsceneExtra_{modelPath}" );
						extraGo.WorldTransform = infoTarget.WorldTransform;
						var extraRenderer = extraGo.AddComponent<SkinnedModelRenderer>();
						extraRenderer.Model = Model.Load( modelPath );
						extraGo.Enabled = true;
						sceneModels.Add( extraRenderer );
						Log.Info( $"[Cutscene] Loaded extra model {modelPath}" );
					}
				}

				player.Cutscenes.StartCutsceneWithPostCleanup( mainRenderer, sceneModels, TargetAttachment, Duration, InputRequiredToStart, HidePlayers );
				Log.Info( "[Cutscene] StartCutsceneWithPostCleanup done" );
				break;
			}

			case EventType.SetLighting:
				scene.GetSystem<PowerSystem>()?.SetLightState( LightsEnabled );
				break;

			case EventType.SetDoorLockState:
			{
				var doorName = DoorName ?? "";
				var door = scene.GetAllComponents<DoorControllerComponent>()
					.FirstOrDefault( x => x.GameObject.Name.Contains( doorName, StringComparison.OrdinalIgnoreCase ) );
				if ( door is null )
				{
					Log.Warning( $"SetDoorLockState: failed to find door {DoorName}" );
					return;
				}
				door.ObjectiveLocked = DoorLocked;
				if ( !DoorLocked )
				{
					door.Locked = false;
					door.BypassItemRequirement = true;
				}
				break;
			}

			case EventType.PlayMonsterIntroductionCutscene:
			{
				var camName = MonsterCameraToUse ?? "";
				var target = scene.GetAllObjects( true )
					.Where( x => x.Name.Contains( camName, StringComparison.OrdinalIgnoreCase ) )
					.OrderByDescending( x => Vector3.DistanceBetween( player.WorldPosition, x.WorldPosition ) )
					.LastOrDefault();

				if ( target is not null )
					player.Cutscenes.StartManualCutscene( target, MonsterCutsceneDuration, HidePlayers );
				break;
			}

			case EventType.ShowCredits:
				CreditsPanel.DisplayCredits();
				break;
		}
	}
}
