namespace GvarJam;

public sealed class ColdOpenMapInstance : MapInstance
{
	[Property] public List<ObjectiveResource> Objectives { get; set; } = new();

	protected override void OnEnabled()
	{
		OnMapLoaded += HandleMapLoaded;
		base.OnEnabled();
	}

	protected override void OnDisabled()
	{
		OnMapLoaded -= HandleMapLoaded;
		base.OnDisabled();
	}

	protected override void OnCreateObject( GameObject go, MapLoader.ObjectEntry kv )
	{
		if ( !string.IsNullOrEmpty( kv.TargetName ) )
			go.Name = kv.TargetName;
		else if ( !string.IsNullOrEmpty( kv.TypeName ) )
			go.Name = kv.TypeName;

		switch ( kv.TypeName )
		{
			case "ent_door_custom":
			{
				go.Tags.Add( "door" );
				var model = kv.GetResource<Model>( "model" );
				if ( model is not null )
				{
					var mr = go.GetOrAddComponent<ModelRenderer>();
					mr.Model = model;
					var col = go.GetOrAddComponent<ModelCollider>();
					col.Model = model;
				}

				var door = go.GetOrAddComponent<DoorControllerComponent>();
				var spawnFlags = kv.GetValue<int>( "spawnsettings", 1 );
				door.UseOpens = (spawnFlags & 1) != 0;
				door.StartLocked = (spawnFlags & 2) != 0;

				var itemStr = kv.GetString( "ItemRequiredToOpen", "" );
				if ( int.TryParse( itemStr, out var itemInt ) && Enum.IsDefined( typeof( ItemType ), itemInt ) )
					door.ItemRequiredToOpen = (ItemType)itemInt;
				else if ( Enum.TryParse<ItemType>( itemStr, true, out var itemParsed ) )
					door.ItemRequiredToOpen = itemParsed;

				door.MoveDir = kv.GetValue<Angles>( "movedir", Angles.Zero );
				door.MoveDirIsLocal = kv.GetValue<bool>( "movedir_islocal", true );
				door.MoveDirType = kv.GetValue<int>( "movedir_type", 0 );
				door.Distance = kv.GetValue<float>( "distance", 0f );
				door.InitialPosition = kv.GetValue<float>( "initial_position", 0f );
				door.OpenAwayFromPlayer = kv.GetValue<bool>( "open_away", false );
				door.Speed = kv.GetValue<float>( "speed", 100f );
				door.TimeBeforeReset = kv.GetValue<float>( "close_delay", 4f );
				door.OtherDoorsToOpen = kv.GetString( "other_doors_to_open", "" );
				door.OpenSound = kv.GetString( "open_sound", "" );
				door.FullyOpenSound = kv.GetString( "fully_open_sound", "" );
				door.CloseSound = kv.GetString( "close_sound", "" );
				door.FullyClosedSound = kv.GetString( "fully_closed_sound", "" );
				door.LockedSound = kv.GetString( "locked_sound", "" );
				door.MovingSound = kv.GetString( "moving_sound", "" );
				break;
			}

			case "ent_item":
			{
				var item = go.GetOrAddComponent<InventoryItemComponent>();
				var itemStr = kv.GetString( "ItemType", "KeycardLvl1" );
				if ( int.TryParse( itemStr, out var itemInt ) && Enum.IsDefined( typeof( ItemType ), itemInt ) )
					item.ItemType = (ItemType)itemInt;
				else if ( Enum.TryParse<ItemType>( itemStr, true, out var itemParsed ) )
					item.ItemType = itemParsed;

				item.Amount = kv.GetValue<int>( "Amount", 1 );
				break;
			}

			case "ent_helmet":
				go.GetOrAddComponent<HelmetPickupComponent>();
				break;

			case "ent_battery":
			{
				var bat = go.GetOrAddComponent<LampBatteryComponent>();
				bat.BatteryPower = kv.GetValue<float>( "BatteryPower", 100f );
				break;
			}

			case "ent_note":
			{
				var note = go.GetOrAddComponent<NoteComponent>();
				note.NoteContents = kv.GetString( "NoteContents", "this is a note" );
				break;
			}

			case "ent_generator":
				go.GetOrAddComponent<GeneratorComponent>();
				break;

			case "ent_console":
				go.GetOrAddComponent<ShipConsoleComponent>();
				break;

			case "ent_storagelocker":
			{
				var locker = go.GetOrAddComponent<StorageLockerComponent>();
				locker.IsLocked = kv.GetValue<bool>( "IsLocked", true );
				locker.RandomItem = kv.GetValue<bool>( "RandomItem", true );
				locker.ItemClass = kv.GetString( "ItemClass", "" );
				break;
			}

			case "ent_playerlocker":
				go.GetOrAddComponent<PlayerLockerComponent>();
				break;

			case "ent_mountedcamera":
			{
				var cam = go.GetOrAddComponent<MountedCameraComponent>();
				cam.IsViewable = kv.GetValue<bool>( "IsViewable", true );
				cam.ZoneName = kv.GetString( "ZoneName", "UNKNOWN" );
				cam.Fov = kv.GetValue<float>( "Fov", 120.0f );
				cam.ZNear = kv.GetValue<float>( "ZNear", 4.0f );
				cam.ZFar = kv.GetValue<float>( "ZFar", 10000.0f );
				cam.Aspect = kv.GetValue<float>( "Aspect", 1.0f );
				break;
			}

			case "ent_cameracontroller":
				go.GetOrAddComponent<CameraControllerComponent>();
				break;

			case "ent_splitizen":
				go.GetOrAddComponent<SplitizenComponent>();
				break;

			case "ObjectiveTrigger":
			{
				go.Tags.Add( "trigger" );
				var model = kv.GetResource<Model>( "model" );
				if ( model is not null )
				{
					var col = go.GetOrAddComponent<ModelCollider>();
					col.Model = model;
					col.Static = true;
					col.IsTrigger = true;
				}
				else
				{
					var box = go.GetOrAddComponent<BoxCollider>();
					box.Scale = new Vector3( 128, 128, 128 );
					box.Static = true;
					box.IsTrigger = true;
				}
				go.GetOrAddComponent<ObjectiveTriggerComponent>();
				break;
			}

			case "info_target":
				// Keep transform + name only
				break;

			default:
				base.OnCreateObject( go, kv );
				break;
		}
	}

	private void HandleMapLoaded()
	{
		// Remove any leftover static MapObjectComponent on custom/replaced entities
		foreach ( var go in GameObject.GetAllObjects( true ) )
		{
			if ( go == GameObject ) continue;
			if ( go.GetComponent<InteractableComponent>() is not null
				|| go.GetComponent<ObjectiveTriggerComponent>() is not null
				|| go.GetComponent<MountedCameraComponent>() is not null
				|| go.GetComponent<SplitizenComponent>() is not null
				|| go.Name.Contains( "introduction_campos", StringComparison.OrdinalIgnoreCase )
				|| go.Name.Contains( "cryo_sleep_pos", StringComparison.OrdinalIgnoreCase )
				|| go.Name.Contains( "selfdestruct_pos", StringComparison.OrdinalIgnoreCase )
				|| go.Name.Contains( "escapepod_pos", StringComparison.OrdinalIgnoreCase ) )
			{
				go.GetComponent<MapObjectComponent>()?.Destroy();
			}
		}

		// Register light cullers
		Scene?.GetSystem<PowerSystem>()?.AttachLightCullers();

		// Dress corpses (nosplit_blank props)
		DressCorpses();

		// Spawn local player if running in game mode
		if ( Scene is not null && !Scene.IsEditor )
		{
			if ( Scene.NavMesh.IsEnabled )
				_ = Scene.NavMesh.Generate( Scene.PhysicsWorld );
			Scene.GetSystem<ColdOpenGame>()?.OnMapReady();
		}
	}

	private void DressCorpses()
	{
		if ( Scene is null ) return;

		var jumpsuit = ResourceLibrary.Get<Clothing>( "models/citizen_clothes/shirt/jumpsuit/blue_jumpsuit.clothing" );
		ClothingContainer container = null;
		if ( jumpsuit is not null )
		{
			container = new ClothingContainer();
			container.Add( jumpsuit );
		}

		foreach ( var smr in Scene.GetAllComponents<SkinnedModelRenderer>() )
		{
			if ( smr.Model is null || !smr.Model.Name.Contains( "nosplit_blank", StringComparison.OrdinalIgnoreCase ) )
				continue;

			if ( container is not null )
				container.Apply( smr );
		}
	}
}
