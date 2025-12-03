using Godot;
using System.Threading.Tasks;

public partial class Arrow : Area2D
{
    [Export] public float Speed = 200f;       // 箭矢飞行速度
    [Export] public int Damage = 1;
    [Export] public float MaxRange = 1000f;    // 最大射程

    private Vector2 _direction = Vector2.Right;
    private Sprite2D _sprite;

    public override void _Ready()
    {
        _sprite = GetNode<Sprite2D>("Sprite2D");
        BodyEntered += OnBodyEntered;
        _ = AutoDestroyAfterTime(1.0f);
    }

    public void Shoot(Vector2 direction)
    {
        _direction = direction.Normalized();
        Rotation = _direction.Angle();
    }

    public override void _PhysicsProcess(double delta)
    {
        //箭飞行
        Position += _direction * Speed * (float)delta;
    }

    private void OnBodyEntered(Node body)
    {
        if (body is IDamageable target)
        {
            target.TakeDamage(Damage);
        }
        if (body is not Player player)
        {
            // 碰撞后销毁箭矢
            CallDeferred("queue_free");
        }
    }

    /// <summary>
    /// 异步定时销毁逻辑
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    private async Task AutoDestroyAfterTime(double seconds)
    {
        await ToSignal(GetTree().CreateTimer(seconds), "timeout");
        CallDeferred("queue_free");
    }
}
