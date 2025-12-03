using Godot;
using System.Threading.Tasks;

public partial class Weapon : BaseWeapon
{
    public Area2D HitBox;
    private AnimatedSprite2D _animatedSprite;
    private Vector2 _rightHandOffset;
    private Vector2 _leftHandOffset;
    private bool _isAttacking = false;
    private bool _facingLeft = false;

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite2D>("Weapon");
        HitBox = GetNode<Area2D>("HitBox");
        WeaponName = WeaponType.Sword;
        _rightHandOffset = Position;
        _leftHandOffset = new Vector2(Position.X - 12, Position.Y);
        Damage = 2;
        if (HitBox != null)
        {
            HitBox.Monitoring = false;
            HitBox.BodyEntered += OnBodyEntered;
        }

        _animatedSprite?.Play("idle");
    }

    public override async Task Attack()
    {
        if (!_canAttack || _isAttacking) return;

        _isAttacking = true;
        _canAttack = false;

        _animatedSprite?.Play("slash");
        AudioManager.Instance.Play(SoundType.WeaponSound);
        HitBox.Monitoring = true;

        if (_animatedSprite != null)
            await ToSignal(_animatedSprite, AnimatedSprite2D.SignalName.AnimationFinished);

        HitBox.Monitoring = false;
        _animatedSprite?.Play("idle");

        _isAttacking = false;
        await Task.Delay((int)(AttackCooldown ));
        _canAttack = true;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is IDamageable target)
            target.TakeDamage(Damage);
    }

    public override void UpdateDirection(bool faceLeft)
    {
        if (_facingLeft == faceLeft) return;
        _facingLeft = faceLeft;
        _animatedSprite.FlipH = faceLeft;
        Position = faceLeft ? _leftHandOffset : _rightHandOffset;
    }
}
