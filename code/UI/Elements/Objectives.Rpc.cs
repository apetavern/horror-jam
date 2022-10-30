namespace GvarJam.UI.Elements;

partial class Objectives
{
	public static Objectives Instance { get; private set; }
	public List<ObjectiveResource> DisplayObjectives { get; set; } = new();

	TimeSince TimeSinceLastObjective = 0;

	public Objectives()
	{
		Instance = this;

		BindClass( "visible", () => DisplayObjectives.Count > 0 );
	}

	[ClientRpc]
	public static void RpcAddObjective( ObjectiveResource resource )
	{
		Instance.DisplayObjectives.Add( resource );

		if ( Instance.TimeSinceLastObjective > 3 )
		{
			_ = new NewObjectivePanel( Local.Hud, resource );
			Instance.TimeSinceLastObjective = 0;
		}

		Instance.StateHasChanged();
	}

	[ClientRpc]
	public static void RpcRemoveObjective( ObjectiveResource resource )
	{
		Instance.DisplayObjectives.RemoveAll( x => x == resource );

		Instance.StateHasChanged();
	}

	class NewObjectivePanel : Panel
	{
		TimeSince TimeSinceCreated;

		public NewObjectivePanel( Panel parent, ObjectiveResource objective )
		{
			Parent = parent;

			var inner = Add.Panel( "inner" );

			inner.Add.Label( "NEW OBJECTIVE", "title" );
			inner.Add.Label( objective.ObjectiveName, "name" );
			inner.Add.Label( objective.Description, "description" );

			TimeSinceCreated = 0;
		}

		public override void Tick()
		{
			base.Tick();

			if ( TimeSinceCreated > 5 )
			{
				Delete();
			}
		}
	}
}
