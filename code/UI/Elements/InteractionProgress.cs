using GvarJam.Utility;

namespace GvarJam.UI.Elements;

internal class InteractionProgress : Panel
{
	private float Thickness => 4f;

	public InteractionProgress()
	{
		AddClass( "auto-hide" );
	}

	public override void Tick()
	{
		if ( Local.Pawn is not Pawn player )
			return;

		SetClass( "visible", player.IsInteracting && player.InteractedEntity is DelayedUseItem item );
	}

	public override void DrawBackground( ref RenderState state )
	{
		base.DrawBackground( ref state );

		if ( Local.Pawn is not Pawn player )
			return;

		if ( !player.IsInteracting )
			return;

		if ( player.InteractedEntity is not DelayedUseItem item )
			return;

		var progress = item.GetInteractionProgress();
		var bounds = Box.Rect;

		var radius = bounds.Width / 2f;
		var alpha = ComputedStyle?.Opacity ?? 1;
		var color = Color.White.WithAlpha( alpha );
		var endAngle = 360f * progress;

		GraphicsExtensions.CircleEx( bounds.Center, radius, radius - Thickness, color, endAngle: endAngle );
	}
}
