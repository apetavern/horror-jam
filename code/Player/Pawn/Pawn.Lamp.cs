namespace GvarJam.Player;

partial class Pawn
{
	/// <summary>
	/// The maximum amount of power that the lamp can hold.
	/// </summary>
	private const float LampMaxPower = 100;

	/// <summary>
	/// The amount of power that is discharged per tick.
	/// </summary>
	private const float LampDischargePerTick = 0.1f;

	/// <summary>
	/// Gets the 0-1 percentage of the batteries power left.
	/// </summary>
	public float BatteryPercentage => LampPower / LampMaxPower;

	/// <summary>
	/// Gets or sets whether or not the lamp is enabled.
	/// </summary>
	public bool LampEnabled
	{
		get
		{
			if ( Lamp is null )
				return false;

			return Lamp.Enabled;
		}
		private set
		{
			if ( Lamp is null || LampEnabled == value )
				return;

			if ( Lamp == null )
				return;

			Lamp.Enabled = value;

			if ( !value )
			{
				TimeSinceLampOff = 0;
				Sound.FromEntity( "flashlight_click_off", this );
			}
			else
				Sound.FromEntity( "flashlight_click_on", this );
		}
	}

	/// <summary>
	/// Gets or sets whether or not a battery is inserted to the pawns helmet.
	/// </summary>
	public bool BatteryInserted
	{
		get => batteryInserted;
		set
		{
			if ( Helmet is null || BatteryInserted == value )
				return;

			batteryInserted = value;
			if ( value )
			{
				LampPower = LampMaxPower;
				Helmet.SetBodyGroup( 0, 0 );
			}
			else
			{
				LampEnabled = false;
				LampPower = 0;
				Helmet.SetBodyGroup( 0, 1 );
			}
		}
	}
	/// <summary>
	/// See <see cref="BatteryInserted"/>.
	/// </summary>
	[Net, Predicted]
	private bool batteryInserted { get; set; } = true;

	/// <summary>
	/// The current amount of power in the lamp.
	/// </summary>
	[Net, Predicted]
	public float LampPower { get; set; } = 0;

	/// <summary>
	/// The time since the lamp was turned off.
	/// </summary>
	[Net, Predicted]
	public TimeSince TimeSinceLampOff { get; private set; }

	/// <summary>
	/// The pawns lamp.
	/// </summary>
	[Net]
	private SpotLightEntity? Lamp { get; set; } = null;

	/// <summary>
	/// Simulates the lamp system.
	/// </summary>
	private void SimulateLamp()
	{
		if ( Helmet is null || Lamp is null )
			return;

		if ( Input.Pressed( InputButton.Flashlight ) && LampPower > 0 )
			LampEnabled = !LampEnabled;

		if ( LampEnabled )
			LampPower -= LampDischargePerTick;

		if ( LampPower <= 0 && LampEnabled )
			LampEnabled = false;

		Lamp.Brightness = BatteryPercentage * 1.4f;
		Lamp.Color = Color.FromBytes( 181, 177, 255 );
		Helmet.RenderColor = Color.FromBytes( 181, 177, 255 ).WithAlpha( BatteryPercentage );
	}
}
