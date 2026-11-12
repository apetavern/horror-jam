namespace GvarJam.UI.Elements;

public sealed class InteractionProgress : Panel
{
	private const float Thickness = 4f;

	public InteractionProgress()
	{
		AddClass( "auto-hide" );
	}

	public override void Tick()
	{
		var player = Game.ActiveScene?.GetAllComponents<ColdOpenPlayer>().FirstOrDefault();
		if ( player is null ) return;

		SetClass( "visible", player.Interaction.IsInteracting && player.Interaction.InteractedEntity is DelayedUseComponent );
	}

	public override void DrawBackground( ref RenderState state )
	{
		base.DrawBackground( ref state );

		var player = Game.ActiveScene?.GetAllComponents<ColdOpenPlayer>().FirstOrDefault();
		if ( player is null || !player.Interaction.IsInteracting ) return;
		if ( player.Interaction.InteractedEntity is not DelayedUseComponent item ) return;

		var progress = item.GetInteractionProgress();
		var bounds = Box.Rect;
		var radius = bounds.Width / 2f;
		var alpha = ComputedStyle?.Opacity ?? 1f;
		var color = Color.White.WithAlpha( alpha );
		var endAngle = 360f * progress;

		GraphicsExtensions.CircleEx( bounds.Center, radius, radius - Thickness, color, endAngle: endAngle );
	}
}
