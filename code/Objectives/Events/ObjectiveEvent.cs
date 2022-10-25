namespace GvarJam.Objectives;

public struct ObjectiveEvent
{
	public enum EventType
	{
		PlaySound,
		PlayCutscene
	}

	public EventType Type { get; set; }

	[ShowIf( nameof( Type ), EventType.PlaySound ), ResourceType( "sound" )]
	public string SoundName { get; set; }

	[ShowIf( nameof( Type ), EventType.PlayCutscene )]
	public string TargetEntityName { get; set; }

	[ShowIf( nameof( Type ), EventType.PlayCutscene )]
	public string TargetEntityAttachment { get; set; }

	[ShowIf( nameof( Type ), EventType.PlayCutscene )]
	public float Duration { get; set; }

	public void Invoke( Pawn pawn )
	{
		switch ( Type )
		{
			case EventType.PlaySound:
				Sound.FromScreen( SoundName );
				break;
			case EventType.PlayCutscene:
				var targetEntity = Entity.FindByName( TargetEntityName ) as ModelEntity;
				pawn.StartCutscene( targetEntity, TargetEntityAttachment, Duration );
				break;
		}
	}
}
