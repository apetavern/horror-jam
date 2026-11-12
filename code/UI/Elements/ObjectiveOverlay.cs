namespace GvarJam.UI.Elements;

public sealed class ObjectiveOverlay : Panel
{
	private const float Size = 32f;
	private const float MaxDistance = 512f;
	private const float Margin = 64f;

	private static readonly Color ObjectiveColor = new( 0xFFFFFFFF );
	private static readonly Color NoteColor = new( 0xFFFFE900 );

	public override void DrawBackground( ref RenderState state )
	{
		base.DrawBackground( ref state );
		DrawObjectives();
		DrawNotes();
	}

	private void DrawIcon( string icon, GameObject go, Color? color = null )
	{
		const float inchesToMetres = 0.0254f;
		var scene = Game.ActiveScene;
		var cam = scene?.Camera;
		if ( cam is null || !go.IsValid() ) return;

		var mr = go.GetComponent<ModelRenderer>();
		var position = mr is not null ? mr.Bounds.Center : go.WorldPosition;
		var distance = cam.WorldPosition.Distance( position );
		var alpha = distance.LerpInverse( MaxDistance, 128f );

		var screenPos = cam.PointToScreenPixels( position, out bool isBehind );
		if ( isBehind )
		{
			screenPos.x = Screen.Width - screenPos.x;
			screenPos.y = Screen.Height - Margin;
		}
		screenPos = screenPos.Clamp( Margin, Screen.Size - Margin );
		screenPos -= Size / 2.0f;

		var rect = new Rect( screenPos, Size );
		var col = (color ?? Color.White).WithAlpha( alpha );
		Graphics.DrawIcon( rect, icon, col, Size );

		rect.Position += new Vector2( 0, 32 );
		rect.Position -= new Vector2( 64, 0 );
		rect.Width = 160f;

		string displayDistance = (distance * inchesToMetres).CeilToInt().ToString();
		Graphics.DrawText( rect, $"{displayDistance}m", col, "Chakra Petch", 14, 400, TextFlag.Center );
	}

	private void DrawObjectives()
	{
		var scene = Game.ActiveScene;
		var player = scene?.GetAllComponents<ColdOpenPlayer>().FirstOrDefault();
		if ( scene is null || ObjectivePanel.Instance is null ) return;

		foreach ( var objective in ObjectivePanel.Instance.DisplayObjectives )
		{
			foreach ( var endCondition in objective.ObjectiveEndConditions )
			{
				GameObject target = endCondition.Type switch
				{
					ObjectiveEndCondition.ConditionType.InteractWithEntity =>
						scene.GetAllObjects( true ).FirstOrDefault( x => x.Name.Contains( endCondition.InteractableName ?? "", StringComparison.OrdinalIgnoreCase ) ),

					ObjectiveEndCondition.ConditionType.InteractedWithEntity =>
						scene.GetAllObjects( true ).FirstOrDefault( x => x.Name.Contains( endCondition.InteractedName ?? "", StringComparison.OrdinalIgnoreCase ) ),

					ObjectiveEndCondition.ConditionType.InteractWithType =>
						scene.GetAllComponents<InteractableComponent>().Where( x => ObjectiveEndCondition.MatchesTypeName( x, endCondition.TypeName ) ).GetClosestOrDefault( player?.GameObject )?.GameObject,

					ObjectiveEndCondition.ConditionType.InteractedWithType =>
						scene.GetAllComponents<InteractableComponent>().Where( x => ObjectiveEndCondition.MatchesTypeName( x, endCondition.InteractedTypeName ) ).GetClosestOrDefault( player?.GameObject )?.GameObject,

					ObjectiveEndCondition.ConditionType.PlayerEnteredTrigger =>
						scene.GetAllObjects( true ).FirstOrDefault( x => x.Name.Contains( endCondition.TriggerName ?? "", StringComparison.OrdinalIgnoreCase ) ),

					ObjectiveEndCondition.ConditionType.PlayerHasItem =>
						scene.GetAllComponents<InventoryItemComponent>().Where( x => x.ItemType.ToString() == endCondition.ItemType ).GetClosestOrDefault( player?.GameObject )?.GameObject,

					_ => null
				};

				if ( target is not null && target.IsValid() )
					DrawIcon( "radio_button_checked", target, ObjectiveColor );
			}
		}
	}

	private void DrawNotes()
	{
		var scene = Game.ActiveScene;
		var cam = scene?.Camera;
		if ( scene is null || cam is null ) return;

		foreach ( var note in scene.GetAllComponents<NoteComponent>() )
		{
			if ( note.WorldPosition.Distance( cam.WorldPosition ) < MaxDistance )
				DrawIcon( "description", note.GameObject, NoteColor );
		}
	}
}
