namespace GvarJam.Utility;

/// <summary>
/// Extension class for <see cref="ModelEntity"/>.
/// </summary>
public static class ModelEntityExtensions
{
	/// <summary>
	/// Dress this citizen with clothes defined inside this class. We'll save the created entities in ClothingModels. All clothing entities are tagged with "clothes".
	/// </summary>
	/// <param name="container">The clothing container to dress from.</param>
	/// <param name="citizen">The citizen model to dress.</param>
	/// <param name="hideInFirstPerson">Whether or not the clothing model should be hidden in first person.</param>
	/// <param name="castShadowsInFirstPerson">Whether or not the clothing model should cast shadows in first person.</param>
	public static void DressEntity( this ClothingContainer container, ModelEntity citizen, bool hideInFirstPerson = true, bool castShadowsInFirstPerson = true )
	{
		//
		// Start with defaults
		//
		citizen.SetMaterialGroup( "default" );

		//
		// Remove old models
		//
		container.ClearEntities();

		var SkinMaterial = container.Clothing.Select( x => x.SkinMaterial ).Where( x => !string.IsNullOrWhiteSpace( x ) ).Select( x => Material.Load( x ) ).FirstOrDefault();
		if ( SkinMaterial is not null )
			citizen.SetMaterialOverride( SkinMaterial, "skin" );

		var EyesMaterial = container.Clothing.Select( x => x.EyesMaterial ).Where( x => !string.IsNullOrWhiteSpace( x ) ).Select( x => Material.Load( x ) ).FirstOrDefault();
		if ( EyesMaterial is not null )
			citizen.SetMaterialOverride( EyesMaterial, "eyes" );

		//
		// Create clothes models
		//
		foreach ( var c in container.Clothing )
		{
			if ( c.Model == "models/citizen/citizen.vmdl" )
			{
				citizen.SetMaterialGroup( c.MaterialGroup );
				continue;
			}

			var anim = new AnimatedEntity( c.Model, citizen );

			anim.Tags.Add( "clothes" );

			if ( SkinMaterial is not null )
				anim.SetMaterialOverride( SkinMaterial, "skin" );
			if ( EyesMaterial is not null )
				anim.SetMaterialOverride( EyesMaterial, "eyes" );

			anim.EnableHideInFirstPerson = hideInFirstPerson;
			anim.EnableShadowInFirstPerson = castShadowsInFirstPerson;

			if ( !string.IsNullOrEmpty( c.MaterialGroup ) )
				anim.SetMaterialGroup( c.MaterialGroup );
		}

		//
		// Set body groups
		//
		foreach ( var group in container.GetBodyGroups() )
			citizen.SetBodyGroup( group.name, group.value );
	}
}
