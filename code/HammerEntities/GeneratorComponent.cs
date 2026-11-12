namespace GvarJam.HammerEntities;

public sealed class GeneratorComponent : DelayedUseComponent
{
	public override string LegacyTypeName => "Generator";
	public override float TimeToUse => 1.1f;
	protected override bool DeleteOnUse => false;
	protected override bool ResetOnUse => false;

	protected override void OnStart()
	{
		base.OnStart();
		DisplayName = "Generator";
		_requiredItems.Clear();
		_requiredItems[ItemType.GeneratorFuse] = 1;

		var model = Model.Load( "models/generator/fusebox_01a.vmdl" );
		var mr = GameObject.GetOrAddComponent<ModelRenderer>();
		mr.Model = model;

		var col = GameObject.GetOrAddComponent<ModelCollider>();
		col.Model = model;
	}

	protected override void CreateActions()
	{
		Actions.Add( (0.6f, (_, _, _) => false) );
		Actions.Add( (0.5f, (_, _, _) => false) );
	}

	protected override void OnUsed( ColdOpenPlayer user )
	{
		base.OnUsed( user );
		Scene.GetSystem<PowerSystem>()?.FlickerLighting( false, 0f );
	}
}
