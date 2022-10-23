namespace GvarJam.Objectives;

internal class BaseObjective
{
	public virtual string ObjectiveName => "Objective Name";
	public virtual string ObjectiveDescription => "Objective Description. Lorem ipsum dolor sit amet";

	public virtual bool CheckObjectiveComplete()
	{
		return false;
	}
}
