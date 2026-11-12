namespace GvarJam.Interactions;

public abstract class DelayedUseComponent : InteractableComponent
{
	public virtual float TimeToUse => 1f;
	protected virtual bool DeleteOnUse => true;
	protected virtual bool ResetOnUse => true;

	protected readonly List<(float Duration, Func<ColdOpenPlayer, bool, float, bool> Action)> Actions = new();

	[Sync] protected float CurrentUseTime { get; set; }
	[Sync] private int CurrentActionIndex { get; set; }

	private readonly HashSet<int> _firstTimeActions = new();

	protected override void OnStart()
	{
		base.OnStart();
		CreateActions();
	}

	public float GetInteractionProgress() => TimeToUse > 0f ? (CurrentUseTime / TimeToUse).Clamp( 0f, 1f ) : 1f;

	protected virtual void CreateActions()
	{
	}

	protected virtual void OnUseTick( ColdOpenPlayer user )
	{
		if ( CurrentActionIndex >= Actions.Count )
			return;

		var previousTotalTime = 0f;
		for ( var i = 0; i < CurrentActionIndex; i++ )
			previousTotalTime += Actions[i].Duration;

		if ( previousTotalTime + Actions[CurrentActionIndex].Duration < CurrentUseTime )
			CurrentActionIndex++;

		if ( CurrentActionIndex >= Actions.Count )
			return;

		bool firstTime = !_firstTimeActions.Contains( CurrentActionIndex );
		_firstTimeActions.Add( CurrentActionIndex );

		var skip = Actions[CurrentActionIndex].Action( user, firstTime, CurrentUseTime - previousTotalTime );
		if ( !skip )
			return;

		CurrentUseTime = previousTotalTime + Actions[CurrentActionIndex].Duration;
		CurrentActionIndex++;
	}

	public override bool IsUsable( ColdOpenPlayer user )
	{
		if ( User is not null && User != user )
			return false;

		return base.IsUsable( user );
	}

	public override bool OnUse( ColdOpenPlayer user )
	{
		User = user;
		CurrentUseTime += Time.Delta;
		OnUseTick( user );

		var used = CurrentUseTime >= TimeToUse;
		if ( used )
		{
			OnUsed( user );
			if ( DeleteOnUse && GameObject.IsValid() )
				GameObject.Destroy();
			else if ( ResetOnUse )
				ResetInteraction();
		}

		return !used;
	}

	public override void ResetInteraction()
	{
		CurrentUseTime = 0;
		CurrentActionIndex = 0;
		_firstTimeActions.Clear();
		User = null;
	}
}
