namespace GvarJam.HammerEntities;

public sealed class MountedCameraComponent : Component
{
	[Sync] public bool IsBeingViewed { get; set; }
	[Property, Sync] public bool IsViewable { get; set; } = true;
	[Property, Sync] public string ZoneName { get; set; } = "UNKNOWN";
	[Property] public float Fov { get; set; } = 120.0f;
	[Property] public float ZNear { get; set; } = 4.0f;
	[Property] public float ZFar { get; set; } = 10000.0f;
	[Property] public float Aspect { get; set; } = 1.0f;

	private const float CameraTrackingReachUnits = 300f;

	private Vector3 _lookPos;
	private Rotation _lookRot = Rotation.Identity;
	private Rotation _flatLookRot = Rotation.Identity;
	private Rotation _startRot = Rotation.Identity;
	private SkinnedModelRenderer _renderer;

	public GameObject LensAttachment => _renderer?.GetAttachmentObject( "lens_position" );

	protected override void OnStart()
	{
		_renderer = GameObject.GetOrAddComponent<SkinnedModelRenderer>();
		_renderer.Model = Model.Load( "models/mountedcamera/mountedcamera.vmdl" );
		_renderer.CreateAttachments = true;
		_startRot = WorldRotation;
		_flatLookRot = WorldRotation;
	}

	protected override void OnFixedUpdate()
	{
		if ( _renderer is null ) return;

		var player = Scene.GetAllComponents<ColdOpenPlayer>().GetClosestOrDefault( GameObject );
		if ( player is null ) return;

		bool shouldTrack = Vector3.DistanceBetween( player.WorldPosition, WorldPosition ) < CameraTrackingReachUnits
			&& player.WorldPosition.z < WorldPosition.z;

		_renderer.Set( "tracking", shouldTrack );

		if ( !shouldTrack )
		{
			_flatLookRot = Rotation.Slerp( _flatLookRot, _startRot, 10f * Time.Delta );
			WorldRotation = _flatLookRot;
			return;
		}

		float playerDist = Vector3.DistanceBetween( WorldPosition - Vector3.Up * 15f, player.EyePosition ) / 200f;
		var targetPos = WorldPosition - Vector3.Up * 15f + (player.EyePosition - (WorldPosition - Vector3.Up * 15f)).Normal * 25f * playerDist;
		_lookPos = Vector3.Lerp( _lookPos, targetPos, 0.5f );

		var targetRot = Rotation.LookAt( player.EyePosition - Vector3.Up * 10f - (WorldPosition - Vector3.Up * 15f), Vector3.Up )
			* new Angles( 0, 90, 90 ).ToRotation();
		_lookRot = Rotation.Slerp( _lookRot, targetRot, 10f * Time.Delta );

		_renderer.Set( "position", _lookPos );
		_renderer.Set( "rotation", _lookRot );

		var flatDir = (player.WorldPosition - WorldPosition).WithZ( 0 );
		if ( !flatDir.IsNearZeroLength )
		{
			_flatLookRot = Rotation.Slerp( _flatLookRot, Rotation.LookAt( flatDir, Vector3.Up ), 10f * Time.Delta );
			WorldRotation = _flatLookRot;
		}
	}
}
