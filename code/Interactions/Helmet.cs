namespace GvarJam.Interactions;

[Library( "ent_helmet" )]
[HammerEntity]
[EditorModel( "models/cosmetics/spacehelmet.vmdl" )]
public sealed class Helmet : DelayedUseItem
{
	/// <inheritdoc/>
	public override float TimeToUse => 2.2f;
	/// <inheritdoc/>
	protected override bool DeleteOnUse => true;

	/// <summary>
	/// Whether or not the battery was picked up.
	/// </summary>
	private bool pickedUp;

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		Name = "Helmet";

		SetModel( "models/cosmetics/spacehelmet.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
	}

	/// <inheritdoc/>
	protected override void CreateActions()
	{
		base.CreateActions();

		Actions.Add( (0.6f, PickupHelmet) );
		Actions.Add( (1.6f, EquipHelmet) );
	}

	/// <inheritdoc/>
	protected override void OnUsed( Entity user )
	{
		base.OnUsed( user );

		if ( IsServer )
			(user as Pawn)!.EquipHelmet();
	}

	/// <inheritdoc/>
	public override bool IsUsable( Entity user )
	{
		return (user as Pawn)!.Helmet is null && base.IsUsable( user );
	}

	/// <inheritdoc/>
	public override void Reset()
	{
		if ( pickedUp )
		{
			SetParent( null );
			Position = Trace.Ray( Position, Position - Vector3.Up * 100f )
				.WorldOnly()
				.Run().EndPosition;
			Rotation = Rotation.Identity;
			pickedUp = false;
		}

		base.Reset();
	}

	/// <summary>
	/// The first animation, picks up the helmet.
	/// </summary>
	/// <param name="user">The entity that is doing the interaction.</param>
	/// <param name="firstTime">Whether or not this has been invoked for the first time.</param>
	/// <param name="timeInAnim">The time in seconds that this animation has been going for.</param>
	/// <returns>Whether or not to skip this animation.</returns>
	private bool PickupHelmet( Entity user, bool firstTime, float timeInAnim )
	{
		if ( IsServer && timeInAnim >= 0.55 && !pickedUp )
			Pickup( user );

		(user as Pawn)!.SetAnimParameter( "grabitem", true );
		return false;
	}

	/// <summary>
	/// Picks up the helmet.
	/// </summary>
	/// <param name="user">The entity that is doing the interaction.</param>
	private void Pickup( Entity user )
	{
		pickedUp = true;

		Tags.Add( "camignore" );
		SetParent( user, "hold_L", Transform.Zero.WithPosition( Vector3.Up * -64 ) );
	}

	/// <summary>
	/// The second animation, equips the helmet.
	/// </summary>
	/// <param name="user">The entity that is doing the interaction.</param>
	/// <param name="firstTime">Whether or not this has been invoked for the first time.</param>
	/// <param name="timeInAnim">The time in seconds that this animation has been going for.</param>
	/// <returns>Whether or not to skip this animation.</returns>
	private bool EquipHelmet( Entity user, bool firstTime, float timeInAnim )
	{
		(user as Pawn)!.SetAnimParameter( "puthelmet", true );
		return false;
	}
}
