namespace GvarJam.Objectives;

public struct ObjectiveEvent
{
	public enum EventType
	{
		PlaySound,
	}

	public EventType Type { get; set; }

	[ShowIf( nameof( Type ), EventType.PlaySound ), ResourceType( "sound" )]
	public string SoundName { get; set; }

	public void Invoke( Pawn pawn )
	{
		switch ( Type )
		{
			case EventType.PlaySound:
				Sound.FromScreen( SoundName );
				break;
		}
	}
}
