using Sandbox.Internal.Globals;

namespace GvarJam.Utility;

/// <summary>
/// Extension class for <see cref="Sandbox.Internal.Globals.DebugOverlay"/>.
/// </summary>
public static class DebugOverlayExtensions
{
	/// <summary>
	/// Draws an arrow to the <see cref="Sandbox.Internal.Globals.DebugOverlay"/>.
	/// <remarks>From https://github.com/Facepunch/sbox-ai-lab/blob/master/code/Draw.cs#L62.</remarks>
	/// </summary>
	/// <param name="DebugOverlay">The debug overlay to draw to.</param>
	/// <param name="startPos">The starting position of the arrow.</param>
	/// <param name="endPos">The ending position of the arrow.</param>
	/// <param name="color">The color to draw the arrow in.</param>
	/// <param name="duration">The duration for the arrow to last.</param>
	public static void Arrow( this DebugOverlay DebugOverlay, Vector3 startPos, Vector3 endPos, Color color, float duration = 0f )
	{
		float width = 8.0f;
		Vector3 up = Vector3.Up;

		var lineDir = (endPos - startPos).Normal;
		var sideDir = lineDir.Cross( up );
		var radius = width * 0.5f;

		var p1 = startPos - sideDir * radius;
		var p2 = endPos - lineDir * width - sideDir * radius;
		var p3 = endPos - lineDir * width - sideDir * width;
		var p4 = endPos;
		var p5 = endPos - lineDir * width + sideDir * width;
		var p6 = endPos - lineDir * width + sideDir * radius;
		var p7 = startPos + sideDir * radius;

		DebugOverlay.Line( p1, p2, color, duration, depthTest: false );
		DebugOverlay.Line( p2, p3, color, duration, depthTest: false );
		DebugOverlay.Line( p3, p4, color, duration, depthTest: false );
		DebugOverlay.Line( p4, p5, color, duration, depthTest: false );
		DebugOverlay.Line( p5, p6, color, duration, depthTest: false );
		DebugOverlay.Line( p6, p7, color, duration, depthTest: false );
	}
}
