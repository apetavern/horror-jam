namespace GvarJam.Cameras;

public partial class ControllableCamera : CameraMode
{
	private Vector3 StartPosition { get; set; }
	private Rotation StartRotation { get; set; }

	public ControllableCamera ( Entity controlFrom )
	{
		StartPosition = controlFrom.Position;
		StartRotation = controlFrom.Rotation;
	}

	public override void Activated()
	{
		var pawn = Local.Pawn;
		if ( pawn == null ) return;

		Position = StartPosition;
		Rotation = StartRotation;
	}

	public override void Update()
	{
		var pawn = Local.Pawn;

		if ( pawn is null ) 
			return;

		//Position = Position.LerpTo( targetPos, ModeSwitchProgress * ModeSwitchSpeed );
		//Rotation = pawn.EyeRotation;

		Viewer = null;
	}
}
