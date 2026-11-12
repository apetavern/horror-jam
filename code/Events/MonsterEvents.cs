namespace GvarJam.Events;

public interface IMonsterSound : ISceneEvent<IMonsterSound>
{
	void OnMonsterSound( Vector3 position, float volume );
}
