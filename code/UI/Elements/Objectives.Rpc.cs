namespace GvarJam.UI.Elements;

partial class Objectives
{
	private static Objectives Instance { get; set; }
	private List<(string Name, string Description)> DisplayObjectives { get; set; } = new();

	public Objectives()
	{
		Instance = this;

		BindClass( "visible", () => DisplayObjectives.Count > 0 );
	}

	[ClientRpc]
	public static void RpcAddObjective( string name, string description )
	{
		Log.Trace( $"RPC: Add objective {name}" );
		Instance.DisplayObjectives.Add( (name, description) );

		Instance.StateHasChanged();
	}

	[ClientRpc]
	public static void RpcRemoveObjective( string name, string description )
	{
		Log.Trace( $"RPC: Remove objective {name}" );
		Instance.DisplayObjectives.RemoveAll( x => x.Name == name );

		Instance.StateHasChanged();
	}
}
