namespace GvarJam.Monster;

public enum States
{
	/// <summary>
	/// In this state, the monster is effectively moved out of the map.
	/// It is 'hiding' from the player, invoking an air of uncertainty
	/// and tension.
	/// </summary>
	Hiding,

	/// <summary>
	/// In this state, the monster is slowly making its way towards the
	/// player. If the monster gets close to the player, it will stop
	/// and wait for the player to get close before proceeding. The
	/// monster will attempt to be 'visible' to the player in some way.
	/// </summary>
	Stalking,

	/// <summary>
	/// In this state, the monster is actively chasing down the player.
	/// This state has a maximum duration of 30 seconds, before the
	/// monster runs off and goes back into a hiding state.
	/// </summary>
	Hunting
}
