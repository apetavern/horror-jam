namespace GvarJam.Player;

public partial class Pawn
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
			InteractedEntity?.ShowInteractionPrompt( false );
			IsInteracting = value is not null;

			if ( Camera is not PawnCamera camera )
				return;

			if ( IsInteracting && (InteractedEntity is DelayedUseItem || InteractedEntity is LockedUseItem))
				camera.GoToThirdPerson();
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
	/// The last interactable entity that was looked at.
	/// </summary>
	[Net, Predicted]
	private InteractableEntity? LastInteractable { get; set; }

	/// <summary>
	/// Simulates the interaction system.
	/// </summary>
	private void SimulateInteraction( Client cl )
	{
		if ( InteractedEntity is not null && InteractedEntity.IsValid )
		{
			InteractedEntity.ShowInteractionPrompt( false );

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
			if ( Input.Down( InputButton.Use ) && interactableEntity.OnUse( this ) )
				InteractedEntity = interactableEntity;

			if ( LastInteractable != interactableEntity )
				LastInteractable?.ShowInteractionPrompt( false );

			interactableEntity.ShowInteractionPrompt( true );
			LastInteractable = interactableEntity;
		}
		else
			LastInteractable?.ShowInteractionPrompt( false );

		if ( Input.Pressed( InputButton.Use ) )
		{
			if ( IsServer && entity is IUse use && Input.Pressed( InputButton.Use ) )
				use.OnUse( this );
		}
	}

	/// <summary>
	/// Finds an interactable entity.
	/// </summary>
	/// <returns>An interactable entity. Null if none were found.</returns>
	private Entity? FindInteractableEntity()
	{
		if ( InteractedEntity is not null && (InteractedEntity as IUse)!.IsUsable( this ) )
			return InteractedEntity;

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
		if ( entity is not IUse use ) return false;
		if ( !use.IsUsable( this ) ) return false;

		return true;
	}
}
