﻿namespace GvarJam.Objectives;

/// <summary>
/// An event to be invoked on pawns when an objective starts or ends.
/// </summary>
public struct ObjectiveEvent
{
	/// <summary>
	/// The type of the event.
	/// </summary>
	public EventType Type { get; set; }

	/// <summary>
	/// The name of the sound to play when <see cref="EventType"/> is <see cref="EventType.PlaySound"/>.
	/// </summary>
	[ShowIf( nameof( Type ), EventType.PlaySound ), ResourceType( "sound" )]
	public string SoundName { get; set; }

	/// <summary>
	/// The name of the entity to start the cutscene in the perspective of.
	/// </summary>
	[ShowIf( nameof( Type ), EventType.PlayCutscene )]
	public string TargetEntityName { get; set; }

	/// <summary>
	/// The name of the entity toget the transform of for the start of the cutscene.
	/// </summary>
	[ShowIf( nameof( Type ), EventType.PlayInstantiatedCutscene )]
	public string InfoTarget { get; set; }

	/// <summary>
	/// The model of the entity to start the cutscene in the perspective of.
	/// </summary>
	[ShowIf( nameof( Type ), EventType.PlayInstantiatedCutscene ), ResourceType( "vmdl" )]
	public string TargetModel { get; set; }

	/// <summary>
	/// A list of all scene models to create for the objective.
	/// </summary>
	[ShowIf( nameof( Type ), EventType.PlayInstantiatedCutscene ), ResourceType( "vmdl" )]
	public List<string> AdditionalSceneModels { get; set; }

	/// <summary>
	/// The attachment of the target entity in the cutscene.
	/// </summary>
	[HideIf( nameof( Type ), EventType.PlaySound )]
	public string TargetAttachment { get; set; }

	/// <summary>
	/// The duration of the cutscene.
	/// </summary>
	[HideIf( nameof( Type ), EventType.PlaySound )]
	public float Duration { get; set; }

	/// <summary>
	/// Invokes the event.
	/// </summary>
	/// <param name="pawn">The pawn to invoke the event for.</param>
	public void Invoke( Pawn pawn )
	{
		switch ( Type )
		{
			case EventType.PlaySound:
				Sound.FromScreen( SoundName );
				break;
			case EventType.PlayCutscene:
				if ( Entity.FindByName( TargetEntityName ) is not AnimatedEntity targetEntity )
				{
					Log.Warning( $"Failed to find target entity {TargetEntityName}" );
					return;
				}

				pawn.StartCutscene( targetEntity, TargetAttachment, Duration );
				break;
			case EventType.PlayInstantiatedCutscene:
				var infoTarget = Entity.FindByName( InfoTarget );

				var animEntity = new AnimatedEntity( TargetModel )
				{
					Transform = infoTarget.Transform
				};

				// Spawn additional models
				var sceneModels = new List<AnimatedEntity>();

				if( AdditionalSceneModels is not null )
				{
					foreach ( var model in AdditionalSceneModels )
					{
						var animEnt = new AnimatedEntity( model )
						{
							Transform = infoTarget.Transform
						};
						sceneModels.Add( animEnt );
					}
				}
				
				pawn.StartCutsceneWithPostCleanup( animEntity, sceneModels, TargetAttachment, Duration );
				break;
		}
	}
}
