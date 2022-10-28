namespace GvarJam.Player;

partial class Pawn
{
	/// <summary>
	/// Whether or not the pawn is interacting with something.
	/// </summary>
	[Net, Predicted]
	public bool IsInteracting { get; private set; }

	/// <summary>
	/// The entity that the pawn is interacting with.
	/// </summary>
	public InteractableEntity? InteractedEntity
	{
		get => interactedEntity;
		set
		{
			if ( InteractedEntity == value )
				return;

			interactedEntity = value;
			IsInteracting = value is not null;

			if ( Camera is not PawnCamera camera )
				return;

			if ( IsInteracting && (InteractedEntity is DelayedUseItem || InteractedEntity is LockedUseItem))
			{
				var rotation = Rotation.LookAt( (InteractedEntity.Position - Position).Normal );
				Rotation = new Rotation( 0, 0, rotation.z, rotation.w );
				camera.GoToThirdPerson();
			}
			else
				camera.GoToFirstPerson();
		}
	}
	/// <summary>
	/// See <see cref="InteractedEntity"/>.
	/// </summary>
	[Net, Predicted]
	private InteractableEntity? interactedEntity { get; set; } = null!;

	/// <summary>
	/// The name of the entity we last successfully interacted with.
	/// </summary>
	[Net, Predicted]
	public string? LastInteractedEntityName { get; set; }

	/// <summary>
	/// The type of the entity we last successfully interacted with.
	/// </summary>
	[Net, Predicted]
	public string? LastInteractedEntityTypeName { get; set; }

	/// <summary>
	/// Simulates the interaction system.
	/// </summary>
	private void SimulateInteraction( Client cl )
	{
		if ( InteractedEntity is not null && InteractedEntity.IsValid )
		{
			if ( Input.Down( InputButton.Use ) && InteractedEntity is not LockedUseItem )
			{
				if ( !InteractedEntity.OnUse( this ) )
					InteractedEntity = null;
			}
			else if ( InteractedEntity is LockedUseItem )
			{
				InteractedEntity.Simulate( cl );
			}
			else if ( Input.Released( InputButton.Use ) )
			{
				InteractedEntity.Reset();
				InteractedEntity = null;
			}
		}
		else
			InteractedEntity = null;

		if ( InteractedEntity is not null )
			return;

		var entity = FindInteractableEntity();
		if ( entity is InteractableEntity interactableEntity )
		{
			if ( Input.Down( InputButton.Use ) &&
				GroundEntity is not null && Velocity.IsNearZeroLength &&
				interactableEntity.IsUsable( this ) && interactableEntity.OnUse( this ) )
				InteractedEntity = interactableEntity;

			return;
		}

		if ( Input.Pressed( InputButton.Use ) )
		{
			if ( IsServer && entity is IUse use && Input.Pressed( InputButton.Use ) )
				use.OnUse( this );
			else
				PlaySound( "player_use_fail" );
		}
	}

	/// <summary>
	/// Finds an interactable entity.
	/// </summary>
	/// <returns>An interactable entity. Null if none were found.</returns>
	public Entity? FindInteractableEntity()
	{
		var tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 100 )
			.WithoutTags( "trigger" )
			.Ignore( this )
			.Run();

		var entity = tr.Entity;
		while ( entity is not null && entity.IsValid && !IsValidInteractableEntity( entity ) )
			entity = entity.Parent;

		if ( IsValidInteractableEntity( entity ) )
			return entity;

		tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 100 )
			.WithoutTags( "trigger" )
			.Radius( 2 )
			.Ignore( this )
			.Run();

		entity = tr.Entity;
		while ( entity is not null && entity.IsValid && !IsValidInteractableEntity( entity ) )
			entity = entity.Parent;

		return IsValidInteractableEntity( entity ) ? entity : null;
	}

	/// <summary>
	/// Returns whether or not the entity is a valid interactable entity.
	/// </summary>
	/// <param name="entity">The entity to check if it is interactable.</param>
	/// <returns>Whether or not the entity is a valid interactable entity.</returns>
	private bool IsValidInteractableEntity( Entity? entity )
	{
		if ( entity is null ) return false;
		if ( !entity.IsValid ) return false;
		if ( entity is not IUse use ) return false;

		return true;
	}
}
