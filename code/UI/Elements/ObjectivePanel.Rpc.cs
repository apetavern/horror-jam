namespace GvarJam.UI.Elements;

/// <summary>
/// The panel to display current objectives to the player.
/// </summary>
partial class ObjectivePanel
{
	/// <summary>
	/// The only instance of <see cref="ObjectivePanel"/> in existance.
	/// </summary>
	public static ObjectivePanel Instance { get; private set; }

	/// <summary>
	/// A list containing all objectives to display on the screen.
	/// </summary>
	public List<ObjectiveResource> DisplayObjectives { get; set; } = new();

	/// <summary>
	/// The time since the last objective was received.
	/// </summary>
	TimeSince TimeSinceLastObjective = 0;

	/// <summary>
	/// Initializes a default instance of <see cref="ObjectivePanel"/>.
	/// </summary>
	public ObjectivePanel()
	{
		Instance = this;

		BindClass( "visible", () => DisplayObjectives.Count > 0 );
	}

	/// <summary>
	/// Adds an objective to display to the player.
	/// </summary>
	/// <param name="resource">The objective description.</param>
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

	/// <summary>
	/// Removes an objective from displaying to the player.
	/// </summary>
	/// <param name="resource">The objective description.</param>
	[ClientRpc]
	public static void RpcRemoveObjective( ObjectiveResource resource )
	{
		Instance.DisplayObjectives.RemoveAll( x => x == resource );

		Instance.StateHasChanged();
	}

	/// <summary>
	/// The panel to display when a new objective is received.
	/// </summary>
	private class NewObjectivePanel : Panel
	{
		/// <summary>
		/// The time since this panel was created.
		/// </summary>
		TimeSince TimeSinceCreated;

		/// <summary>
		/// Initializes a new instance of <see cref="NewObjectivePanel"/> with its parent and the objective that it is displaying.
		/// </summary>
		/// <param name="parent">The parent of this panel.</param>
		/// <param name="objective">The objective description that is new.</param>
		public NewObjectivePanel( Panel parent, ObjectiveResource objective )
		{
			Parent = parent;

			var inner = Add.Panel( "inner" );

			inner.Add.Label( "NEW OBJECTIVE", "title" );
			inner.Add.Label( objective.ObjectiveName, "name" );
			inner.Add.Label( objective.Description, "description" );

			TimeSinceCreated = 0;
		}

		/// <inheritdoc/>
		public override void Tick()
		{
			base.Tick();

			if ( TimeSinceCreated > 5 )
				Delete();
		}
	}
}
