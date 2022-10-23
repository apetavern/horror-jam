﻿namespace GvarJam;

public partial class Pawn
{
	/// <summary>
	/// Whether or not the pawn is interacting with something.
	/// </summary>
	[Net, Predicted]
	public bool IsInteracting { get; private set; }

	private InteractableEntity? LastInteractable { get; set; }

	/// <summary>
	/// The entity that the pawn is interacting with.
	/// </summary>
	public Entity? InteractedEntity
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
				camera.GoToThirdPerson();

				var rotation = Rotation.LookAt( (InteractedEntity.Position - Position).Normal );
				Rotation = new Rotation( 0, 0, rotation.z, rotation.w );
			}
			else
				camera.GoToFirstPerson();
		}
	}
	/// <summary>
	/// See <see cref="InteractedEntity"/>.
	/// </summary>
	[Net, Predicted]
	private Entity? interactedEntity { get; set; } = null!;

	/// <summary>
	/// Simulates the interaction system.
	/// </summary>
	private void SimulateInteraction()
	{
		var entity = FindInteractableEntity();

		if ( entity is InteractableEntity interactableEntity )
		{
			if ( LastInteractable != interactableEntity )
				LastInteractable?.ShowInteractionPrompt( false );

			interactableEntity.ShowInteractionPrompt( true );
			LastInteractable = interactableEntity;
		}
		else
			LastInteractable?.ShowInteractionPrompt( false );

		if ( InteractedEntity is not null && !InteractedEntity.IsValid )
			InteractedEntity = null;

		if( InteractedEntity is LockedUseItem lockedUseItem )
		{
			lockedUseItem.Simulate();
			return;
		}

		if ( Input.Down( InputButton.Use ) )
		{
			if ( entity is not null )
			{
				if ( entity is IInteractable interactable )
				{
					if ( interactable.OnUse( this ) )
						InteractedEntity = entity;
					else
						InteractedEntity = null;
				}
				else if ( IsServer && entity is IUse use )
					use.OnUse( this );
				else
					InteractedEntity = null;
			}
		}
		else if ( Input.Released( InputButton.Use ) )
		{
			if( InteractedEntity is not null )
			{
				if ( InteractedEntity.IsValid && InteractedEntity is IInteractable interactable )
					interactable.Reset();

				if( InteractedEntity is not LockedUseItem )
					InteractedEntity = null;
			}
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

	private bool IsValidInteractableEntity( Entity? entity )
	{
		if ( entity is null ) return false;
		if ( entity is not IUse use ) return false;
		if ( !use.IsUsable( this ) ) return false;

		return true;
	}
}
