using Godot;
using System.Threading.Tasks;

public partial class Bow : BaseWeapon
{
    [Export] public PackedScene ArrowScene;
    private AnimatedSprite2D _animatedSprite;
    private Vector2 _rightHandOffset;
    private Vector2 _leftHandOffset;
    private bool _facingLeft = false;

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _rightHandOffset = Position;
        _leftHandOffset = new Vector2(Position.X - 15, Position.Y);
        _animatedSprite?.Play("idle");
        WeaponName = WeaponType.Bow;
    }

    public override void UpdateDirection(bool faceLeft)
    {
        _facingLeft = faceLeft;
        _animatedSprite.FlipH = faceLeft;
        Position = faceLeft ? _leftHandOffset : _rightHandOffset;
    }

    public override async Task Attack()
    {
        if (!_canAttack || ArrowScene == null) return;

        _canAttack = false;

        _animatedSprite?.Play("shoot");
        AudioManager.Instance.Play(SoundType.WeaponSound);

        Arrow arrow = ArrowScene.Instantiate<Arrow>();
        arrow.Damage = Damage;
        arrow.Scale = new Vector2(0.5f, 0.5f);
        GetTree().CurrentScene.AddChild(arrow);
        arrow.Position = GlobalPosition;
        arrow.Shoot(_facingLeft ? Vector2.Left : Vector2.Right);

        await Task.Delay((int)AttackCooldown);
        _canAttack = true;
        _animatedSprite?.Play("idle");
    }
}
