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
				Vector3 position = 0;

				//
				// This is awful. :D
				//
				switch ( endCondition.Type )
				{
					case ObjectiveEndCondition.ConditionType.InteractWithEntity:
						{
							var entity = Entity.FindByName( endCondition.InteractableName );
							position = entity.WorldSpaceBounds.Center;
							break;
						}
					case ObjectiveEndCondition.ConditionType.InteractedWithEntity:
						{
							var entity = Entity.FindByName( endCondition.InteractedName );
							position = entity.WorldSpaceBounds.Center;
							break;
						}
					case ObjectiveEndCondition.ConditionType.InteractWithType:
						{
							var entity = Entity.All.Where( x => x.GetType().Name == endCondition.TypeName ).First();
							position = entity.WorldSpaceBounds.Center;
							break;
						}
					case ObjectiveEndCondition.ConditionType.InteractedWithType:
						{
							var entity = Entity.All.Where( x => x.GetType().Name == endCondition.TypeName ).First();
							position = entity.WorldSpaceBounds.Center;
							break;
						}
					case ObjectiveEndCondition.ConditionType.PlayerEnteredTrigger:
						{
							var entity = Entity.FindByName( endCondition.TriggerName );
							position = entity.WorldSpaceBounds.Center;
							break;
						}
					case ObjectiveEndCondition.ConditionType.PlayerHasItem:
						{
							var entity = Entity.All.OfType<InventoryItem>().Where( x => x.ItemType.ToString() == endCondition.ItemType ).First();
							position = entity.WorldSpaceBounds.Center;
							break;
						}
					case ObjectiveEndCondition.ConditionType.Timer:
						continue;
				}

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
