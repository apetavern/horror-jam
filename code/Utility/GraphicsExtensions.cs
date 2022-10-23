namespace GvarJam.Utility;

/// <summary>
/// Extension class for <see cref="Graphics"/>.
/// </summary>
public static class GraphicsExtensions
{
	public static void CircleEx( in Vector2 center, float outer, float inner, Color color, int points = 32, float startAngle = 0f, float endAngle = 360f )
	{
		var vertices = new List<Vertex>();

		void AddVertex( Vector2 v, Color color, Vector2 uv2 )
		{
			var vert = new Vertex( v, uv2, color );
			vertices.Add( vert );
		}

		float twoPi = MathF.PI * 2f;
		startAngle = startAngle.NormalizeDegrees().DegreeToRadian();
		for ( endAngle = endAngle.NormalizeDegrees().DegreeToRadian(); endAngle <= startAngle; endAngle += twoPi )
		{
		}

		float size = (endAngle - startAngle) % (twoPi + 0.01f);
		if ( size <= 0f )
		{
			return;
		}

		float frac = twoPi / points;

		for ( float i = startAngle; i < endAngle; i += frac )
		{
			float angle = i;
			float angleFrac = i + frac;
			if ( angleFrac > endAngle )
			{
				angleFrac = endAngle;
			}

			angle += MathF.PI;
			angleFrac += MathF.PI;

			Vector2 x = new Vector2( MathF.Sin( 0f - angle ), MathF.Cos( 0f - angle ) );
			Vector2 y = new Vector2( MathF.Sin( 0f - angleFrac ), MathF.Cos( 0f - angleFrac ) );

			Vector2 uv2 = x / 2f + 0.5;
			Vector2 uv3 = y / 2f + 0.5;
			Vector2 uv4 = x * inner / outer / 2f + 0.5;
			Vector2 uv5 = y * inner / outer / 2f + 0.5;

			Vector2 v = center + x * outer;
			AddVertex( v, color, uv2 );

			v = center + y * outer;
			AddVertex( v, color, uv3 );

			v = center + x * inner;
			AddVertex( v, color, uv4 );

			if ( inner > 0f )
			{
				v = center + y * outer;
				AddVertex( v, color, uv3 );

				v = center + y * inner;
				AddVertex( v, color, uv5 );

				v = center + x * inner;
				AddVertex( v, color, uv4 );
			}
		}

		Graphics.Draw( vertices.ToArray(), vertices.Count, Material.UI.Box );
	}
}
