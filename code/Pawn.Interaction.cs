using GvarJam.Interactions;

namespace GvarJam;

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
	[Net, Predicted]
	public Entity? InteractedEntity { get; private set; } = null!;

	/// <summary>
	/// Simulates the interaction system.
	/// </summary>
	private void SimulateInteraction()
	{
		if ( InteractedEntity is not null && !InteractedEntity.IsValid )
			InteractedEntity = null;

		if ( Input.Down( InputButton.Use ) )
		{
			var entity = FindInteractableEntity();
			if ( entity is not null )
			{
				if ( (entity as IUse)!.OnUse( this ) )
				{
					InteractedEntity = entity;
					IsInteracting = true;
					Camera.GoToThirdPerson();
				}
				else
				{
					InteractedEntity = null;
					IsInteracting = false;
					Camera.GoToFirstPerson();
				}
			}
		}
		else if ( Input.Released( InputButton.Use ) )
		{
			if ( InteractedEntity is not null && InteractedEntity.IsValid && InteractedEntity is IInteractable interactable )
				interactable.Reset();

			InteractedEntity = null;
			IsInteracting = false;
			Camera.GoToFirstPerson();
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

		var tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward.Normal * 10000 )
			.WithTag( "usable" )
			.Ignore( this )
			.Run();

		return tr.Entity;
	}
}
