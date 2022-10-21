namespace GvarJam.Interaction;

public class GlowItem : ModelEntity
{
	public virtual Color GlowColor { get; set; } = Color.Orange;

	/// <summary>
	/// Enable or disable the glow component on this entity.
	/// </summary>
	/// <param name="shouldGlow"></param>
	public void ShouldGlow(bool shouldGlow)
	{
		var component = Components.GetOrCreate<Glow>();

		component.Color = GlowColor;
		component.ObscuredColor = Color.Transparent;

		component.Enabled = shouldGlow;
	}
}

