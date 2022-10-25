using System.Runtime.CompilerServices;

namespace GvarJam.Objectives;

public struct ObjectiveEvent
{
	public enum EventType
	{
		PlaySound,
		PlayCutscene,
		PlayInstantiatedCutscene
	}

	public EventType Type { get; set; }

	[ShowIf( nameof( Type ), EventType.PlaySound ), ResourceType( "sound" )]
	public string SoundName { get; set; }

	[ShowIf( nameof( Type ), EventType.PlayCutscene )]
	public string TargetEntityName { get; set; }

	[ShowIf( nameof( Type ), EventType.PlayInstantiatedCutscene ), ResourceType( "vmdl" )]
	public string TargetModel { get; set; }

	[ShowIf( nameof( Type ), EventType.PlayInstantiatedCutscene )]
	public string InfoTarget { get; set; }

	[HideIf( nameof( Type ), EventType.PlaySound )]
	public string TargetAttachment { get; set; }

	[HideIf( nameof( Type ), EventType.PlaySound )]
	public float Duration { get; set; }

	[ShowIf( nameof( Type ), EventType.PlayInstantiatedCutscene ), ResourceType( "vmdl" )]
	public List<string> AdditionalSceneModels { get; set; }

	public void Invoke( Pawn pawn )
	{
		switch ( Type )
		{
			case EventType.PlaySound:
				Sound.FromScreen( SoundName );
				break;
			case EventType.PlayCutscene:
				var targetEntity = Entity.FindByName( TargetEntityName ) as AnimatedEntity;
				pawn.StartCutscene( targetEntity, TargetAttachment, Duration );
				break;
			case EventType.PlayInstantiatedCutscene:
				var infoTarget = Entity.FindByName( InfoTarget );

				var animEntity = new AnimatedEntity( TargetModel );
				animEntity.Transform = infoTarget.Transform;

				// Spawn additional models
				var sceneModels = new List<AnimatedEntity>();

				if( AdditionalSceneModels is not null )
				{
					foreach ( var model in AdditionalSceneModels )
					{
						var animEnt = new AnimatedEntity( model );
						animEnt.Transform = infoTarget.Transform;
						sceneModels.Add( animEnt );
					}
				}
				
				pawn.StartCutsceneWithPostCleanup( animEntity, sceneModels, TargetAttachment, Duration );
				break;
		}
	}
}
