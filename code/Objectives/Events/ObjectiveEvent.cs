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

				pawn.StartCutscene( animEntity, TargetAttachment, Duration );
				break;
		}
	}
}
