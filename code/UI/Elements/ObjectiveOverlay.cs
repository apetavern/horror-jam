namespace GvarJam.UI.Elements;

public class ObjectiveOverlay : Panel
{
	private const float Size = 32f;
	private const float MaxDistance = 512f;

	private Color ObjectiveColor => new Color( 0xFFFFFFFF );
	private Color NoteColor => new Color( 0xFFFFE900 );

	private float Margin = 64f;

	private void DrawIcon( string icon, Entity entity, Color? _color = null )
	{
		const float inchesToMetres = 0.0254f;

		var position = entity.WorldSpaceBounds.Center;
		var distance = CurrentView.Position.Distance( position );
		var alpha = distance.LerpInverse( MaxDistance, 128f );

		var size = Size;

		var screenPos = ((Vector2)position.ToScreen()) * Screen.Size;
		screenPos = screenPos.Clamp( Margin, Screen.Size - Margin );
		screenPos -= size / 2.0f;

		var rect = new Rect( screenPos, size );

		var color = _color ?? Color.White;
		color = color.WithAlpha( alpha );

		Graphics.DrawIcon( rect, icon, color, size );

		rect.Position += new Vector2( 0, 32 );
		rect.Position -= new Vector2( 64, 0 );
		rect.Width = 160f;

		string displayDistance = (distance * inchesToMetres).CeilToInt().ToString();
		Graphics.DrawText( rect, $"{displayDistance}m", color, "Chakra Petch", 16, 400, TextFlag.Center );
	}

	public override void DrawBackground( ref RenderState state )
	{
		base.DrawBackground( ref state );
		DrawObjectives();
		DrawNotes();
	}

	private void DrawObjectives()
	{

		foreach ( var objective in Objectives.Instance.DisplayObjectives )
		{
			foreach ( var endCondition in objective.ObjectiveEndConditions )
			{
				//
				// This is awful. :D
				//
				Entity? entity = endCondition.Type switch
				{
					ObjectiveEndCondition.ConditionType.InteractWithEntity =>
						Entity.FindByName( endCondition.InteractableName ),

					ObjectiveEndCondition.ConditionType.InteractedWithEntity =>
						Entity.FindByName( endCondition.InteractedName ),

					ObjectiveEndCondition.ConditionType.InteractWithType =>
						Entity.All.Where( x => x.GetType().Name == endCondition.TypeName ).FirstOrDefault(),

					ObjectiveEndCondition.ConditionType.InteractedWithType =>
						Entity.All.Where( x => x.GetType().Name == endCondition.TypeName ).FirstOrDefault(),

					ObjectiveEndCondition.ConditionType.PlayerEnteredTrigger =>
						Entity.FindByName( endCondition.TriggerName ),

					ObjectiveEndCondition.ConditionType.PlayerHasItem =>
						Entity.All.OfType<InventoryItem>().Where( x => x.ItemType.ToString() == endCondition.ItemType ).FirstOrDefault(),

					ObjectiveEndCondition.ConditionType.Timer =>
						null,

					_ =>
						null
				};

				if ( !entity.IsValid() || entity == null )
					continue;

				DrawIcon( "radio_button_checked", entity, ObjectiveColor );
			}
		}
	}

	private void DrawNotes()
	{
		foreach ( var note in Entity.All.OfType<Note>()
			.Where( x => x.WorldSpaceBounds.Center.Distance( CurrentView.Position ) < MaxDistance ) )
		{
			DrawIcon( "description", note, NoteColor );
		}
	}
}
