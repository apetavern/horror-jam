﻿namespace GvarJam.Objectives;

/// <summary>
/// Represents a type of an event to invoke.
/// </summary>
public enum EventType
{
	/// <summary>
	/// Play a sound.
	/// </summary>
	PlaySound,
	/// <summary>
	/// Play a new cutscene.
	/// </summary>
	PlayCutscene,
	/// <summary>
	/// Play an existing cutscene.
	/// </summary>
	PlayInstantiatedCutscene,
	/// <summary>
	/// Alter the ships lighting
	/// </summary>
	SetLighting
}
