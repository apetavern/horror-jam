namespace GvarJam.UI.Elements;

public sealed class InteractionPromptComponent : PanelComponent
{
	private InteractableComponent _currentTarget;
	private Label _label;

	protected override void OnTreeFirstBuilt()
	{
		base.OnTreeFirstBuilt();
		Panel.AddClass( "world-panel" );
		Panel.StyleSheet.Load( "/UI/Hud.scss" );
		_label = Panel.Add.Label( "Name" );
		Panel.AddClass( "hide" );
	}

	protected override void OnUpdate()
	{
		var player = Scene.GetAllComponents<ColdOpenPlayer>().FirstOrDefault();
		if ( player is null || Panel is null ) return;

		var entity = player.Interaction.FindInteractable();
		if ( entity != _currentTarget )
		{
			Panel.AddClass( "hide" );
			if ( _currentTarget is not null && _currentTarget.IsValid() )
			{
				_currentTarget.ShowInteractionPrompt( player, false );
				_currentTarget = null;
			}
		}

		if ( entity is null || !entity.IsValid() )
			return;

		if ( entity != _currentTarget )
		{
			if ( _label is not null )
				_label.Text = entity.DisplayName;

			var mr = entity.GetComponent<ModelRenderer>();
			var center = mr is not null ? mr.Bounds.Center : entity.WorldPosition;
			var height = mr?.Model?.RenderBounds.Size.z ?? 32f;
			WorldPosition = center + Vector3.Up * height * 0.6f;

			Panel.RemoveClass( "hide" );
		}

		entity.ShowInteractionPrompt( player, true );
		_currentTarget = entity;
	}
}
