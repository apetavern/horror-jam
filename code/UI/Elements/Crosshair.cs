namespace GvarJam.UI.Elements;

internal class Crosshair : Panel
{
	public const float Size = 4f;
	public const float JiggleAmount = 0.05f;

	public override void DrawBackground( ref RenderState state )
	{
		base.DrawBackground( ref state );

		if ( Local.Pawn is not Pawn player )
			return;

		if ( player.IsInteracting )
			return;

		var targetPos = CurrentView.Position + CurrentView.Rotation.Forward * 32f;

		//
		// Try to preserve some camera motion, makes things look more natural
		//
		var cameraOffset = player.EyePosition - CurrentView.Position;
		targetPos += cameraOffset * JiggleAmount;

		var screenPos = targetPos.ToScreen( Screen.Size ) ?? 0;
		var rect = new Rect( screenPos, Size );
		Graphics.DrawRoundedRectangle( rect, Color.White, new( Size ) );
	}

	public override void Tick()
	{
		base.Tick();
	}
}
