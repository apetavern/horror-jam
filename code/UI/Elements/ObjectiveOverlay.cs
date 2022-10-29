namespace GvarJam.UI.Elements;

public class ObjectiveOverlay : Panel
{
	private const float Size = 32f;
	private const float MaxDistance = 512f;

	public override void DrawBackground( ref RenderState state )
	{
		base.DrawBackground( ref state );

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

				var position = entity.WorldSpaceBounds.Center;
				var distance = CurrentView.Position.Distance( position );
				var alpha = distance.LerpInverse( MaxDistance, 128f );

				var size = Size;
				var screenPos = position.ToScreen( Screen.Size ) ?? new Vector2( -128f );
				screenPos -= size / 2.0f;

				var rect = new Rect( screenPos, size );
				var icon = "radio_button_checked";
				var color = Color.White.WithAlpha( alpha );

				Graphics.DrawIcon( rect, icon, color, size );
			}
		}
	}
}
