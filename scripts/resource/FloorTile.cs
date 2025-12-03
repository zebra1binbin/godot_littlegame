using Godot;

public partial class FloorTile : RigidBody2D
{
    [Export] public float BreakDelay = 0.8f;   // 踩上后多久开始掉落
    [Export] public bool AutoRespawn = true;   // 是否自动复原
    [Export] public float RespawnTime = 3f;    // 掉落后多久复原（精确计时）

    private Area2D _detectArea;
    private Sprite2D _sprite;
    private Vector2 _startPos;
    private float _startRotation;
    private bool _triggered = false;

    public override void _Ready()
    {
        _startPos = GlobalPosition;
        _startRotation = Rotation;
        _detectArea = GetNode<Area2D>("Area2D");
        _sprite = GetNode<Sprite2D>("Sprite2D");

        // 初始状态：冻结成静态地板
        Freeze = true;
        FreezeMode = FreezeModeEnum.Static;

        _detectArea.BodyEntered += body =>
        {
            if (_triggered || !body.IsInGroup("player")) return;
            _triggered = true;
            GetTree().CreateTimer(BreakDelay).Timeout += StartFalling;
        };
    }

    private async void StartFalling()
    {
        await ToSignal(GetTree(), "process_frame");
        Freeze = false;
        LinearVelocity = new Vector2(
             (float)GD.RandRange(-100f, 100f),
             (float)GD.RandRange(-50f, 80f)
         );
        AngularVelocity = (float)GD.RandRange(-15f, 15f);
        await ToSignal(GetTree().CreateTimer(RespawnTime), SceneTreeTimer.SignalName.Timeout);
        if (AutoRespawn)
        {
            GlobalPosition = _startPos;
            Rotation = _startRotation;
            LinearVelocity = Vector2.Zero;
            AngularVelocity = 0f;
            Freeze = true;
            FreezeMode = FreezeModeEnum.Static;
            _triggered = false;

            if (_sprite != null)
            {
                _sprite.Modulate = Colors.Transparent;
                var tween = CreateTween();
                tween.TweenProperty(_sprite, "modulate", Colors.White, 0.3f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quint);
            }
        }
        else
        {
            QueueFree(); 
        }
    }
}