using Sandbox;

namespace GvarJam.Components;
public partial class LightCullComponent : EntityComponent
{
	public PointLightEntity light;

	float brightnessFade;

	Sound burningSound;

	protected override void OnActivate()
	{
		base.OnActivate();
		light = Entity as PointLightEntity;
		//light.Color = Color.Orange;
		//light.Brightness = 1f;
		//light.Range = 256;

		light.DynamicShadows = false;

		light.RenderDirty();
	}

	[Event.Tick.Client]
	public void OnTick()
	{
		if ( !(Game.Current as HorrorGame).areLightsOn )
		{
			if ( light.Enabled )
			{
				light.Enabled = false;
			}
			return;
		}
		if(light != null && Local.Pawn != null )
		{
			float dist = Vector3.DistanceBetween( light.Position, Local.Pawn.Position + Local.Pawn.Rotation.Forward * 128f );
			bool UnShadowed = Trace.Ray( light.Position, Local.Pawn.Position + Vector3.Up * 45f ).Ignore( Local.Pawn ).Run().Hit 
							&& Trace.Ray( light.Position, (Local.Pawn as Pawn).EyePosition ).Ignore( Local.Pawn ).Run().Hit;

			//DebugOverlay.Line( light.Position, Local.Pawn.Position + Vector3.Up * 45f, UnShadowed ? Color.Red : Color.Green );

			if ( UnShadowed )// && dist > 300
			{
				brightnessFade = 0f;
			}

			if( !UnShadowed || dist < 128 )
			{
				brightnessFade = 1f;
			}

			light.Brightness = MathX.Lerp( light.Brightness, brightnessFade, 0.5f * Time.Delta );

			if(light.Brightness <= 0.01f )
			{
				light.DynamicShadows = false;
				light.Enabled = false;
			}
			else
			{
				light.DynamicShadows = true;
				light.Enabled = true;
			}

			light.RenderDirty();
		}
	}
}
