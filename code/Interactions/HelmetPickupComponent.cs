namespace GvarJam.Interactions;

public sealed class HelmetPickupComponent : DelayedUseComponent
{
	public override string LegacyTypeName => "Helmet";
	public override float TimeToUse => 2.2f;
	protected override bool DeleteOnUse => true;

	private bool _pickedUp;

	protected override void OnStart()
	{
		base.OnStart();
		DisplayName = "Helmet";

		var model = Model.Load( "models/cosmetics/spacehelmet.vmdl" );
		var mr = GameObject.GetOrAddComponent<ModelRenderer>();
		mr.Model = model;

		var col = GameObject.GetOrAddComponent<ModelCollider>();
		col.Model = model;
	}

	protected override void CreateActions()
	{
		Actions.Add( (0.6f, PickupHelmet) );
		Actions.Add( (1.6f, EquipHelmet) );
	}

	protected override void OnUsed( ColdOpenPlayer user )
	{
		base.OnUsed( user );
		user.Lamp.EquipHelmet();
	}

	public override bool IsUsable( ColdOpenPlayer user )
	{
		return !user.Lamp.HasHelmet && base.IsUsable( user );
	}

	public override void ResetInteraction()
	{
		if ( _pickedUp )
		{
			GameObject.SetParent( null, true );
			var tr = Scene.Trace.Ray( WorldPosition, WorldPosition - Vector3.Up * 100f )
				.WithoutTags( "player", "trigger" )
				.Run();
			WorldPosition = tr.EndPosition;
			WorldRotation = Rotation.Identity;
			_pickedUp = false;
		}

		base.ResetInteraction();
	}

	private bool PickupHelmet( ColdOpenPlayer user, bool firstTime, float timeInAnim )
	{
		if ( timeInAnim >= 0.55f && !_pickedUp )
		{
			_pickedUp = true;
			GameObject.Tags.Add( "camignore" );
			var bone = user.BodyRenderer?.GetBoneObject( "hold_L" ) ?? user.GameObject;
			GameObject.SetParent( bone, false );
			LocalPosition = Vector3.Up * -64f;
		}

		user.BodyRenderer?.Set( "grabitem", true );
		return false;
	}

	private bool EquipHelmet( ColdOpenPlayer user, bool firstTime, float timeInAnim )
	{
		user.BodyRenderer?.Set( "puthelmet", true );
		return false;
	}
}
