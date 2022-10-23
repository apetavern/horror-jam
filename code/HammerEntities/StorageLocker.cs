namespace GvarJam.HammerEntities;

[Category( "Environment" )]
[Library( "ent_storagelocker" )]
[HammerEntity]
[EditorModel( "models/scifilocker/scifilocker.vmdl" )]
public sealed partial class StorageLocker : InstantUseItem
{
	/// <summary>
	/// Whether or not this locker is locked
	/// </summary>
	[Net, Property]
	public bool IsLocked { get; set; } = true;

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/scifilocker/scifilocker.vmdl" );

		Transmit = TransmitType.Always;
	}
}

