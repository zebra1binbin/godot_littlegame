using Godot;
using System.Threading.Tasks;

public abstract partial class BaseEnemy : CharacterBody2D, IDamageable
{
    [Export] public float Speed = 60f;
    [Export] public int Damage = 1;
    [Export] public int MaxHP = 1;
    [Export] public float EdgeCheckDistance = 8f;
    [Export] public float DamageCooldown = 0.3f;
    [Export] public string MonsterName;
    [Export] public PackedScene DropWeaponScene;

    protected int _direction = 1;
    protected int _HP;
    protected bool _isStunned = false;
    protected AnimatedSprite2D _animatedSprite;
    public bool IsDead { get; protected set; } = false;
    public enum EnemyState { Idle, Patrol, Run, Attack, Die }
    protected EnemyState _state = EnemyState.Idle;
    private bool _canTakeDamage = true;
    private EnemyInfoPanel _enemyInfo;

    public override void _Ready()
    {
        _HP = MaxHP;
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _enemyInfo = GetNodeOrNull<EnemyInfoPanel>("EnemyInfoPanel");
        if (_enemyInfo != null) 
        {
            _enemyInfo.SetLableName(MonsterName);
            _enemyInfo.InitializeHealth(MaxHP);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsDead || _isStunned)
            return;

        Vector2 motion = new Vector2((float)(Speed * _direction * delta), 0);
        var collision = MoveAndCollide(motion);

        if (collision != null)
        {
            if (collision.GetCollider() is Player player)
            {
                player.TakeDamage(Damage);
            }
            else
            {
                TurnAround();
            }
        }
        else if (!IsGroundAhead())
        {
            TurnAround();
        }
    }

    /// <summary>
    /// 转身逻辑
    /// </summary>
    protected void TurnAround()
    {
        _direction *= -1;
        if (_animatedSprite != null)
            _animatedSprite.FlipH = _direction < 0;
    }

    protected virtual bool IsGroundAhead()
    {
        Transform2D transform = GlobalTransform;
        Vector2 offset = new Vector2(_direction * 6, 4 + EdgeCheckDistance);
        Transform2D testTransform = transform.Translated(offset);

        PhysicsTestMotionParameters2D testParams = new PhysicsTestMotionParameters2D()
        {
            From = testTransform,
            Motion = Vector2.Zero
        };

        PhysicsTestMotionResult2D result = new PhysicsTestMotionResult2D();
        return PhysicsServer2D.BodyTestMotion(GetRid(), testParams, result);

    }

    /// <summary>
    /// 受伤
    /// </summary>
    /// <param name="dmg"></param>
    public virtual async void TakeDamage(int dmg)
    {
        if (!_canTakeDamage || IsDead) return;

        _canTakeDamage = false;
        _HP -= dmg;
        if (_enemyInfo != null)
        {
            _enemyInfo.SetHealth(_HP);
        }

        AudioManager.Instance.Play(SoundType.EnemyHitSound);
        _animatedSprite?.Play("hit");
        _isStunned = true;

        // 僵直 0.5 秒
        await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
        _isStunned = false;
        _animatedSprite?.Play("idle");

        if (_HP <= 0)
        {
            await Die();
            return;
        }

        // 受伤冷却
        await ToSignal(GetTree().CreateTimer(DamageCooldown), "timeout");
        _canTakeDamage = true;
    }

    protected virtual async Task Die()
    {
        if (IsDead) return;
        IsDead = true;

        var collision = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        if (collision != null)
            collision.SetDeferred("disabled", true);

        _animatedSprite?.Play("die");
        CallDeferred(nameof(SpawnDropWeapon));

        await ToSignal(_animatedSprite, AnimatedSprite2D.SignalName.AnimationFinished);
        CallDeferred("queue_free");
    }

    /// <summary>
    /// 掉落武器
    /// </summary>
    private void SpawnDropWeapon()
    {
        if (DropWeaponScene == null)
        {
            GD.PrintErr("⚠️ 未设置掉落武器场景");
            return;
        }
        var drop = DropWeaponScene.Instantiate<Area2D>();
        if (drop == null)
        {
            return;
        }

        // 把掉落物放在敌人原地
        GetParent().AddChild(drop);
        drop.GlobalPosition = GlobalPosition + new Vector2(0, 5);
    }
}
