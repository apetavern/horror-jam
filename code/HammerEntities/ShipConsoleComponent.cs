namespace GvarJam.HammerEntities;

public sealed class ShipConsoleComponent : DelayedUseComponent
{
	public override string LegacyTypeName => "ShipConsole";
	public override float TimeToUse => 1.0f;
	protected override bool DeleteOnUse => false;

	[Sync] private bool Used { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		DisplayName = "Control panel";
		_requiredItems.Clear();
		_requiredItems[ItemType.KeycardLvl3] = 1;

		var model = Model.Load( "models/ctrlpanel/ctrlpanel_01a.vmdl" );
		var mr = GameObject.GetOrAddComponent<ModelRenderer>();
		mr.Model = model;

		var col = GameObject.GetOrAddComponent<ModelCollider>();
		col.Model = model;
	}

	protected override void CreateActions()
	{
		Actions.Add( (1.0f, (_, _, _) => false) );
	}

	public override bool IsUsable( ColdOpenPlayer user )
	{
		return !Used && base.IsUsable( user );
	}
}
