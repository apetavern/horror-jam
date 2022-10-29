﻿global using GvarJam.HammerEntities;
global using GvarJam.Interactions;
global using GvarJam.Inventory;
global using GvarJam.Managers;
global using GvarJam.Monster;
global using GvarJam.Objectives;
global using GvarJam.Player;
global using GvarJam.UI;
global using GvarJam.Utility;
global using GvarJam.Components;
global using Sandbox;
global using Sandbox.Component;
global using Sandbox.UI;
global using Sandbox.UI.Construct;
global using SandboxEditor;
global using System;
global using System.Collections.Generic;
global using System.Linq;

namespace GvarJam;

/// <summary>
/// The game class.
/// </summary>
public sealed partial class HorrorGame : Game
{
	public void DressEntity(ClothingContainer contain, ModelEntity citizen, bool hideInFirstPerson = true, bool castShadowsInFirstPerson = true )
	{
		//
		// Start with defaults
		//
		citizen.SetMaterialGroup( "default" );

		//
		// Remove old models
		//
		contain.ClearEntities();

		var SkinMaterial = contain.Clothing.Select( x => x.SkinMaterial ).Where( x => !string.IsNullOrWhiteSpace( x ) ).Select( x => Material.Load( x ) ).FirstOrDefault();
		var EyesMaterial = contain.Clothing.Select( x => x.EyesMaterial ).Where( x => !string.IsNullOrWhiteSpace( x ) ).Select( x => Material.Load( x ) ).FirstOrDefault();

		if ( SkinMaterial != null ) citizen.SetMaterialOverride( SkinMaterial, "skin" );
		if ( EyesMaterial != null ) citizen.SetMaterialOverride( EyesMaterial, "eyes" );

		//
		// Create clothes models
		//
		foreach ( var c in contain.Clothing )
		{
			if ( c.Model == "models/citizen/citizen.vmdl" )
			{
				citizen.SetMaterialGroup( c.MaterialGroup );
				continue;
			}

			var anim = new AnimatedEntity( c.Model, citizen );

			anim.Tags.Add( "clothes" );

			if ( SkinMaterial != null ) anim.SetMaterialOverride( SkinMaterial, "skin" );
			if ( EyesMaterial != null ) anim.SetMaterialOverride( EyesMaterial, "eyes" );

			anim.EnableHideInFirstPerson = hideInFirstPerson;
			anim.EnableShadowInFirstPerson = castShadowsInFirstPerson;

			if ( !string.IsNullOrEmpty( c.MaterialGroup ) )
				anim.SetMaterialGroup( c.MaterialGroup );

			//contain.ClothingModels.Add( anim );
		}

		//
		// Set body groups
		//
		foreach ( var group in contain.GetBodyGroups() )
		{
			citizen.SetBodyGroup( group.name, group.value );
		}
	}

	public override void PostLevelLoaded()
	{
		base.PostLevelLoaded();
		IEnumerable<Prop> props = Entity.All.OfType<Prop>();

		List<ModelEntity> corpses = new List<ModelEntity>();

		ClothingContainer clothes = new ClothingContainer();

		clothes.Clothing.Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/shirt/jumpsuit/blue_jumpsuit.clothing" ) );

		List<string> ClothesToAdd = new List<string>()
		{
			"models/citizen_clothes/shirt/jumpsuit/models/blue_jumpsuit.vmdl",
		};

		foreach ( Prop prop in props )
		{
			if ( prop.GetModelName().Contains( "nosplit_blank" ) )
			{
				corpses.Add( prop );
			}
		}

		foreach ( var corpse in corpses )
		{
			DressEntity(clothes, corpse );
		}
	}
}
