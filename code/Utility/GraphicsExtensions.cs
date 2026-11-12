namespace GvarJam.Utility;

public static class GraphicsExtensions
{
	public static void CircleEx( in Vector2 center, float outer, float inner, Color color, int points = 32, float startAngle = 0f, float endAngle = 360f )
	{
		var vertices = new List<Vertex>();

		void AddVertex( Vector2 v, Color col, Vector2 uv2 )
		{
			vertices.Add( new Vertex( v, uv2, col ) );
		}

		float twoPi = MathF.PI * 2f;
		startAngle = startAngle.NormalizeDegrees().DegreeToRadian();
		for ( endAngle = endAngle.NormalizeDegrees().DegreeToRadian(); endAngle <= startAngle; endAngle += twoPi )
		{
		}

		float size = (endAngle - startAngle) % (twoPi + 0.01f);
		if ( size <= 0f ) return;

		float frac = twoPi / points;

		for ( float i = startAngle; i < endAngle; i += frac )
		{
			float angle = i;
			float angleFrac = i + frac;
			if ( angleFrac > endAngle ) angleFrac = endAngle;

			angle += MathF.PI;
			angleFrac += MathF.PI;

			Vector2 x = new( MathF.Sin( 0f - angle ), MathF.Cos( 0f - angle ) );
			Vector2 y = new( MathF.Sin( 0f - angleFrac ), MathF.Cos( 0f - angleFrac ) );

			Vector2 uv2 = x / 2f + 0.5f;
			Vector2 uv3 = y / 2f + 0.5f;
			Vector2 uv4 = x * inner / outer / 2f + 0.5f;
			Vector2 uv5 = y * inner / outer / 2f + 0.5f;

			AddVertex( center + x * outer, color, uv2 );
			AddVertex( center + y * outer, color, uv3 );
			AddVertex( center + x * inner, color, uv4 );

			if ( inner > 0f )
			{
				AddVertex( center + y * outer, color, uv3 );
				AddVertex( center + y * inner, color, uv5 );
				AddVertex( center + x * inner, color, uv4 );
			}
		}

		Graphics.Draw( vertices.ToArray(), vertices.Count, Material.UI.Box );
	}
}
