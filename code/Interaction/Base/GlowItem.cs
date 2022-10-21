namespace GvarJam.Interactions;

public class GlowItem : ModelEntity
{
	public virtual Color GlowColor { get; set; } = Color.Orange;

	/// <summary>
	/// Enable or disable the glow component on this entity.
	/// </summary>
	/// <param name="shouldGlow">Whether or not to enable the glow component on the entity.</param>
	public void ShouldGlow( bool shouldGlow )
	{
		var component = Components.GetOrCreate<Glow>();

		component.Color = GlowColor;
		component.ObscuredColor = Color.Transparent;

		component.Enabled = shouldGlow;
	}
}

