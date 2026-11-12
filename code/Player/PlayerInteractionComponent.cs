namespace GvarJam.Player;

public sealed class PlayerInteractionComponent : Component
{
	private ColdOpenPlayer _player;

	private InteractableComponent _interactedEntity;
	public InteractableComponent InteractedEntity
	{
		get => _interactedEntity;
		set
		{
			if ( _interactedEntity == value ) return;
			_interactedEntity = value;
			IsInteracting = value is not null;

			if ( IsInteracting && (value is DelayedUseComponent || value is LockedUseComponent) )
			{
				var dir = (value.WorldPosition - _player.WorldPosition).WithZ( 0 );
				if ( !dir.IsNearZeroLength )
					_player.WorldRotation = Rotation.LookAt( dir.Normal, Vector3.Up );
				_player.Camera.GoToThirdPerson();
			}
			else
			{
				_player.Camera.GoToFirstPerson();
			}
		}
	}

	[Sync] public bool IsInteracting { get; private set; }

	protected override void OnStart()
	{
		_player = GetComponent<ColdOpenPlayer>();
	}

	protected override void OnUpdate()
	{
		if ( _player is null || _player.IsDead ) return;

		if ( InteractedEntity is not null && InteractedEntity.IsValid() )
		{
			if ( Input.Down( "Use" ) && InteractedEntity is not LockedUseComponent )
			{
				if ( !InteractedEntity.OnUse( _player ) )
					InteractedEntity = null;
			}
			else if ( InteractedEntity is LockedUseComponent locked )
			{
				locked.SimulateLocked( _player );
			}
			else if ( Input.Released( "Use" ) )
			{
				InteractedEntity.ResetInteraction();
				InteractedEntity = null;
			}
		}
		else
		{
			InteractedEntity = null;
		}

		if ( InteractedEntity is not null )
			return;

		var entity = FindInteractable();
		if ( entity is not null )
		{
			if ( Input.Down( "Use" ) && _player.IsGrounded && _player.Velocity.IsNearZeroLength
				&& entity.IsUsable( _player ) && entity.OnUse( _player ) )
			{
				InteractedEntity = entity;
			}
			return;
		}

		if ( Input.Pressed( "Use" ) )
		{
			AudioDirector.PlayAt( "player_use_fail", _player.WorldPosition );
		}
	}

	public InteractableComponent FindInteractable()
	{
		var tr = Scene.Trace.Ray( _player.EyePosition, _player.EyePosition + _player.EyeRotation.Forward * 100f )
			.WithoutTags( "trigger" )
			.IgnoreGameObjectHierarchy( _player.GameObject )
			.Run();

		var comp = tr.GameObject?.GetComponentInParent<InteractableComponent>();
		if ( comp is not null && comp.IsValid() )
			return comp;

		tr = Scene.Trace.Ray( _player.EyePosition, _player.EyePosition + _player.EyeRotation.Forward * 100f )
			.WithoutTags( "trigger" )
			.Radius( 2f )
			.IgnoreGameObjectHierarchy( _player.GameObject )
			.Run();

		comp = tr.GameObject?.GetComponentInParent<InteractableComponent>();
		return comp is not null && comp.IsValid() ? comp : null;
	}
}
