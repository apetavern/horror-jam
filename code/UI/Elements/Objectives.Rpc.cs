namespace GvarJam.UI.Elements;

partial class Objectives
{
	public static Objectives Instance { get; private set; }
	public List<ObjectiveResource> DisplayObjectives { get; set; } = new();

	public Objectives()
	{
		Instance = this;

		BindClass( "visible", () => DisplayObjectives.Count > 0 );
	}

	[ClientRpc]
	public static void RpcAddObjective( ObjectiveResource resource )
	{
		Instance.DisplayObjectives.Add( resource );

		Instance.StateHasChanged();
	}

	[ClientRpc]
	public static void RpcRemoveObjective( ObjectiveResource resource )
	{
		Instance.DisplayObjectives.RemoveAll( x => x == resource );

		Instance.StateHasChanged();
	}
}
