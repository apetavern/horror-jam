namespace GvarJam.Monster;

public class PathFinding
{
	public float Speed { get; set; } = 100f;
	public float Accel => 200f;
	private Vector3 CurrentPosition => Monster.IsValid()?Monster.Position:Vector3.Zero;

	// This is quite shit! Too bad
	public bool IsFinished => CurrentPath == null
		|| CurrentPath.Count <= 0
		|| Vector3.DistanceBetween( CurrentPosition, TargetPosition ) < 64;

	private NavPath CurrentPath { get; set; }
	private Vector3 TargetPosition { get; set; }
	private NavPathBuilder NavBuilder { get; set; }
	private AnimatedEntity Monster { get; set; }

	public PathFinding( AnimatedEntity monster )
	{
		Monster = monster;
	}

	public void SetPath( Vector3 target )
	{
		GenerateNewPath( target );
	}

	public void SetRandomPath()
	{
		SetPath( NavMesh.GetPointWithinRadius( CurrentPosition, 768, 2048f ) ?? Vector3.Zero );
	}

	private void GenerateNewPath( Vector3 target )
	{
		NavBuilder = new NavPathBuilder( CurrentPosition );
		NavBuilder.WithStepHeight( 16f );

		CurrentPath = NavBuilder.Build( target );
		TargetPosition = target;
	}

	/// <summary>
	/// Returns the direction and speed at which the monster should be moving.
	/// Useful for MoveHelpers.
	/// </summary>
	/// <param name="currentVelocity"></param>
	/// <param name="friction"></param>
	/// <returns></returns>
	public Vector3 GetWishVelocity( Vector3 currentVelocity, float friction )
	{
		if ( IsFinished )
		{
			SetRandomPath();
			return Vector3.Zero;
		}

		Vector3 newVelocity = currentVelocity;

		float speed = GetSpeed();
		Vector3 direction = GetDirection();
		float acceleration = GetAcceleration();

		if ( !IsFinished )
		{
			newVelocity += direction * acceleration * friction * Time.Delta;
			if ( newVelocity.Length > speed )
				newVelocity = newVelocity.Normal * speed;
		}

		return newVelocity;
	}

	/// <summary>
	/// Returns the speed at which the monster should be moving.
	/// </summary>
	/// <returns></returns>
	private float GetAcceleration()
	{
		// This just returns the Accel value. It's only really here
		// for consistency's sake, because GetSpeed exists.

		return Accel;
	}

	/// <summary>
	/// Returns the speed at which the monster should be moving.
	/// </summary>
	/// <returns></returns>
	private float GetSpeed()
	{
		// Slow down the closer we get to the final segment.
		// This prevents the monster from constantly shooting back and forth
		// when trying to reach the final point.

		var lastSegment = CurrentPath.Segments.Last();

		var lastSegmentTween = (lastSegment.Position - CurrentPosition).WithZ( 0 );
		float speedMul = lastSegmentTween.Length.LerpInverse( -64, 128 );

		return Speed * speedMul;
	}

	/// <summary>
	/// Get the direction the monster should move in.
	/// </summary>
	/// <returns></returns>
	private Vector3 GetDirection()
	{
		var segmentsCopy = CurrentPath.Segments.ToList();

		for ( int i = 0; i < segmentsCopy.Count - 1; i++ )
		{
			var segment = segmentsCopy[i];
			var nextSegment = segmentsCopy[i + 1];

			if ( Vector3.DistanceBetween( segment.Position, CurrentPosition ) < 32 )
			{
				if ( i < CurrentPath.Count - 1 && i >= 0 )
					CurrentPath.Segments.RemoveAt( i );
			}

			if ( MonsterEntity.DebugEnabled )
				DebugOverlay.Arrow( segment.Position, nextSegment.Position, Color.White );
		}

		var closestSegment = CurrentPath.Segments.First().Position;
		var closestSegmentTween = (closestSegment - CurrentPosition).WithZ( 0 );

		if ( closestSegmentTween.Length > 32 )
			closestSegmentTween = closestSegmentTween.Normal;

		return closestSegmentTween;
	}
}
