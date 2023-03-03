namespace GvarJam.UI.Elements;

/// <summary>
/// The overlay for displaying icons over objective entities.
/// </summary>
public sealed class ObjectiveOverlay : Panel
{
	/// <summary>
	/// The size of the material icon.
	/// </summary>
	private const float Size = 32f;
	/// <summary>
	/// The maximum distance to display an icon on an objective.
	/// </summary>
	private const float MaxDistance = 512f;
	/// <summary>
	/// The margin to keep objective icons from within the bounds of the screen.
	/// </summary>
	private const float Margin = 64f;

	/// <summary>
	/// The color to highlight objective markers in.
	/// </summary>
	private static readonly Color ObjectiveColor = new( 0xFFFFFFFF );
	/// <summary>
	/// The color to highlight note markers in.
	/// </summary>
	private static readonly Color NoteColor = new( 0xFFFFE900 );

	/// <inheritdoc/>
	public override void DrawBackground( ref RenderState state )
	{
		base.DrawBackground( ref state );
		DrawObjectives();
		DrawNotes();
	}

	/// <summary>
	/// Draws an icon over an entity.
	/// </summary>
	/// <param name="icon">The icon to draw.</param>
	/// <param name="entity">The entity to draw the icon over.</param>
	/// <param name="color">The color to draw the icon in.</param>
	private void DrawIcon( string icon, Entity entity, Color? color = null )
	{
		const float inchesToMetres = 0.0254f;
		var col = color ?? Color.White;

		var position = entity.WorldSpaceBounds.Center;
		var distance = Camera.Position.Distance( position );
		var alpha = distance.LerpInverse( MaxDistance, 128f );

		var screenPos = ((Vector2)position.ToScreen()) * Screen.Size;
		screenPos = screenPos.Clamp( Margin, Screen.Size - Margin );
		screenPos -= Size / 2.0f;

		var rect = new Rect( screenPos, Size );

		col = col.WithAlpha( alpha );
		Graphics.DrawIcon( rect, icon, col, Size );

		rect.Position += new Vector2( 0, 32 );
		rect.Position -= new Vector2( 64, 0 );
		rect.Width = 160f;

		string displayDistance = (distance * inchesToMetres).CeilToInt().ToString();
		Graphics.DrawText( rect, $"{displayDistance}m", col, "Chakra Petch", 14, 400, TextFlag.Center );
	}

	/// <summary>
	/// Draws objective markers.
	/// </summary>
	private void DrawObjectives()
	{
		foreach ( var objective in ObjectivePanel.Instance.DisplayObjectives )
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
						Entity.All.Where( x => x.GetType().Name == endCondition.TypeName ).GetClosestOrDefault( Game.LocalPawn ),

					ObjectiveEndCondition.ConditionType.InteractedWithType =>
						Entity.All.Where( x => x.GetType().Name == endCondition.TypeName ).GetClosestOrDefault( Game.LocalPawn ),

					ObjectiveEndCondition.ConditionType.PlayerEnteredTrigger =>
						Entity.FindByName( endCondition.TriggerName ),

					ObjectiveEndCondition.ConditionType.PlayerHasItem =>
						Entity.All.OfType<InventoryItem>().Where( x => x.ItemType.ToString() == endCondition.ItemType ).GetClosestOrDefault( Game.LocalPawn ),

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

	/// <summary>
	/// Draws note markers.
	/// </summary>
	private void DrawNotes()
	{
		foreach ( var note in Entity.All.OfType<Note>()
			.Where( x => x.WorldSpaceBounds.Center.Distance( Camera.Position ) < MaxDistance ) )
		{
			DrawIcon( "description", note, NoteColor );
		}
	}
}
