namespace GvarJam;

public partial class Pawn
{
	/// <summary>
	/// The pawns lamp.
	/// </summary>
	[Net]
	private SpotLightEntity Lamp { get; set; } = null!;
	/// <summary>
	/// The delay in seconds before the lamps power starts recharging.
	/// </summary>
	private const float LampRechargeDelay = 2;
	/// <summary>
	/// The amount of power that is recharged per tick.
	/// </summary>
	private const float LampRechargePerTick = 1;
	/// <summary>
	/// The amount of power that is discharged per tick.
	/// </summary>
	private const float LampDischargePerTick = 0.1f;
	/// <summary>
	/// The maximum amount of power that the lamp can hold.
	/// </summary>
	private const float LampMaxPower = 100;

	/// <summary>
	/// Gets or sets whether or not the lamp is enabled.
	/// </summary>
	public bool LampEnabled
	{
		get => Lamp.Enabled;
		private set
		{
			if ( BatteryInserted )
			{
				Lamp.Enabled = value;
			}
			else
			{
				Lamp.Enabled = false;
			}
			if ( !value )
			{
				TimeSinceLampOff = 0;
				Sound.FromEntity( "flashlight_click_off", this );
			}
			else
			{
				Sound.FromEntity( "flashlight_click_on", this );
			}
		}
	}

	/// <summary>
	/// The current amount of power in the lamp.
	/// </summary>
	[Net, Predicted]
	public float LampPower { get; private set; } = LampMaxPower;

	/// <summary>
	/// The time since the lamp was turned off.
	/// </summary>
	[Net, Predicted]
	public TimeSince TimeSinceLampOff { get; private set; }

	/// <summary>
	/// Simulates the lamp system.
	/// </summary>
	private void SimulateLamp()
	{
		if ( Input.Pressed( InputButton.Flashlight ) && LampPower > 0 )
			LampEnabled = !LampEnabled;

		if ( !LampEnabled && TimeSinceLampOff > LampRechargeDelay )
			LampPower = MathX.Clamp( LampPower + LampRechargePerTick, 0, LampMaxPower );

		if ( LampEnabled )
			LampPower -= LampDischargePerTick;

		if ( LampPower <= 0 && LampEnabled )
			LampEnabled = false;

		Lamp.Brightness = LampPower / 100;
	}

	/// <summary>
	/// Resets the lamps battery level to full.
	/// </summary>
	public void InsertNewLampBattery()
	{
		LampPower = LampMaxPower;
	}
}
