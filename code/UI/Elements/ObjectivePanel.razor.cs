namespace GvarJam.UI.Elements;

public partial class ObjectivePanel
{
	public static ObjectivePanel Instance { get; private set; }
	public List<ObjectiveResource> DisplayObjectives { get; } = new();
	private TimeSince _timeSinceLastObjective = 10f;

	public ObjectivePanel()
	{
		Instance = this;
		BindClass( "visible", () => DisplayObjectives.Count > 0 );

		var sys = Game.ActiveScene?.GetSystem<ObjectiveSystem>();
		if ( sys is not null )
		{
			sys.OnObjectiveAdded += AddObjective;
			sys.OnObjectiveRemoved += RemoveObjective;
			foreach ( var active in sys.ActiveObjectives )
				DisplayObjectives.Add( active.Resource );
		}
	}

	public override void OnDeleted()
	{
		var sys = Game.ActiveScene?.GetSystem<ObjectiveSystem>();
		if ( sys is not null )
		{
			sys.OnObjectiveAdded -= AddObjective;
			sys.OnObjectiveRemoved -= RemoveObjective;
		}
		base.OnDeleted();
	}

	public void AddObjective( ObjectiveResource resource )
	{
		if ( !DisplayObjectives.Contains( resource ) )
			DisplayObjectives.Add( resource );

		if ( _timeSinceLastObjective > 3f && Parent is not null )
		{
			_ = new NewObjectivePanel( Parent, resource );
			_timeSinceLastObjective = 0;
		}

		StateHasChanged();
	}

	public void RemoveObjective( ObjectiveResource resource )
	{
		DisplayObjectives.RemoveAll( x => x == resource );
		StateHasChanged();
	}

	private class NewObjectivePanel : Panel
	{
		private readonly TimeSince _timeSinceCreated;

		public NewObjectivePanel( Panel parent, ObjectiveResource objective )
		{
			Parent = parent;
			var inner = Add.Panel( "inner" );
			inner.Add.Label( "NEW OBJECTIVE", "title" );
			inner.Add.Label( objective.ObjectiveName, "name" );
			inner.Add.Label( objective.Description, "description" );
			_timeSinceCreated = 0;
		}

		public override void Tick()
		{
			base.Tick();
			if ( _timeSinceCreated > 5f )
				Delete();
		}
	}
}
