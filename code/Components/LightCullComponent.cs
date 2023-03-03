namespace GvarJam.Components;

/// <summary>
/// A component to cull the light source of its owner when not needed.
/// </summary>
public sealed class LightCullComponent : EntityComponent
{
	/// <summary>
	/// The light this component is on.
	/// </summary>
	public PointLightEntity? Light;

	/// <summary>
	/// The distance at which the lights will fade.
	/// </summary>
	private float brightnessFade;

	/// <summary>
	/// An array of all cameras in the map.
	/// </summary>
	private MountedCamera[] cams;
	/// <summary>
	/// The currently viewed camera.
	/// </summary>
	private MountedCamera? viewcam;

	/// <inheritdoc/>
	protected override void OnActivate()
	{
		base.OnActivate();

		Light = (PointLightEntity)Entity;

		cams = Entity.All.OfType<MountedCamera>().ToArray();

		Light.DynamicShadows = false;
		Light.RenderDirty();
	}

	/// <summary>
	/// Called for every tick on the client.
	/// </summary>
	[Event.Tick.Client]
	public void OnTick()
	{
		if ( !HorrorGame.Current.LightsEnabled )
		{
			if ( Light.Enabled )
				Light.Enabled = false;

			return;
		}

		if ( Light != null && Game.LocalPawn != null ) 
		{
			viewcam = null;
			foreach ( var cam in cams )
			{
				if ( cam.IsBeingViewed )
					viewcam = cam;
			}

			float dist = Vector3.DistanceBetween( Light.Position, Game.LocalPawn.Position + Game.LocalPawn.Rotation.Forward * 128f );
			bool UnShadowed = Trace.Ray( Light.Position, Game.LocalPawn.Position + Vector3.Up * 45f ).Ignore( Game.LocalPawn ).Run().Hit 
							&& Trace.Ray( Light.Position, (Game.LocalPawn as Pawn)!.EyePosition ).Ignore( Game.LocalPawn ).Run().Hit;

			if ( viewcam != null )
			{
				UnShadowed = Trace.Ray( Light.Position, Game.LocalPawn.Position + Vector3.Up * 45f ).Ignore( Game.LocalPawn ).Run().Hit
							&& Trace.Ray( Light.Position, (Game.LocalPawn as Pawn)!.EyePosition ).Ignore( Game.LocalPawn ).Run().Hit && 
							Trace.Ray( Light.Position, viewcam.Position - Vector3.Up * 45f ).Ignore( Game.LocalPawn ).Run().Hit;
			}

			if ( UnShadowed )
				brightnessFade = 0f;

			if( !UnShadowed || dist < 256 )
				brightnessFade = 1f;

			Light.Brightness = MathX.Lerp( Light.Brightness, brightnessFade, 0.5f * Time.Delta );

			if(Light.Brightness <= 0.01f )
			{
				Light.DynamicShadows = false;
				Light.Enabled = false;
			}
			else
			{
				Light.DynamicShadows = true;
				Light.Enabled = true;
			}

			Light.RenderDirty();
		}
	}
}
