namespace GvarJam.UI.Elements;

/// <summary>
/// The circular progress meter for interactions.
/// </summary>
public sealed class InteractionProgress : Panel
{
	/// <summary>
	/// The thickness of the outer circle.
	/// </summary>
	private const float Thickness = 4f;

	/// <summary>
	/// Initializes a default instance of <see cref="InteractionProgress"/>.
	/// </summary>
	public InteractionProgress()
	{
		AddClass( "auto-hide" );
	}

	/// <inheritdoc/>
	public override void Tick()
	{
		if ( Game.LocalPawn is not Pawn player )
			return;

		SetClass( "visible", player.IsInteracting && player.InteractedEntity is DelayedUseItem item );
	}

	/// <inheritdoc/>
	public override void DrawBackground( ref RenderState state )
	{
		base.DrawBackground( ref state );

		if ( Game.LocalPawn is not Pawn player )
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
