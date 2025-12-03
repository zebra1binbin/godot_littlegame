using Godot;
using System;
using System.Threading.Tasks;
using static Godot.HttpRequest;

public partial class GuardEnemy : BaseEnemy
{
    [Export] public float DetectionRange = 100f; //守卫区域
    [Export] public float ChaseSpeed = 50f; // 追击速度
    [Export] public float ReturnSpeed = 80f; // 回出生点速度
    [Export] public RayCast2D GroundCheckRayCastLeft; //空地检测左侧
    [Export] public RayCast2D GroundCheckRayCastRight; //空地检测右侧
    [Export] public RayCast2D PlayerCheckRayCastLeft; //袭击玩家检测左侧
    [Export] public RayCast2D PlayerCheckRayCastRight; //袭击玩家检测右侧

    private Player _player;
    private Vector2 _spawnPosition;
    private bool _needBack = false;
    private bool _isAttacking = false;
    private int _attackFrame = 3;              // 攻击触发帧（从 3 开始）
    private string _attackAnimation = "attack";
    private Area2D _attackAreaLeft;
    private Area2D _attackAreaRight;

    public override void _Ready()
    {
        base._Ready();
        _spawnPosition = GlobalPosition;
        _player = GetTree().GetFirstNodeInGroup("player") as Player;
        ChangeState(EnemyState.Idle);
        _attackAreaLeft = GetNode<Area2D>("HitboxLeft");
        _attackAreaLeft.Monitoring = false;
        _attackAreaLeft.BodyEntered += OnAttackAreaBodyEntered;
        _attackAreaRight = GetNode<Area2D>("HitboxRight");
        _attackAreaRight.Monitoring = false;
        _attackAreaRight.BodyEntered += OnAttackAreaBodyEntered;
        _animatedSprite.FrameChanged += OnFrameChanged;
        _animatedSprite.AnimationFinished += OnAnimationFinished;
        //GD.Print(_animatedSprite.FlipH); //false-》左边；true-》右边
    }

    public async override void _PhysicsProcess(double delta)
    {
        if (IsDead || _isStunned || _player == null || !IsInstanceValid(_player))
            return;

        float distanceToPlayer = GlobalPosition.DistanceTo(_player.GlobalPosition);
        switch (_state)
        {
            case EnemyState.Idle:
                StateIdle(distanceToPlayer);
                break;
            case EnemyState.Run:
                StateRun(distanceToPlayer, delta);
                break;
            case EnemyState.Attack:
                await StateAttack(distanceToPlayer);
                break;
        }
    }

    private void ChangeState(EnemyState newState)
    {
        if (_state == newState) return;
        _state = newState;

        switch (_state)
        {
            case EnemyState.Idle:
                _animatedSprite?.Play("idle");
                break;
            case EnemyState.Run:
                _animatedSprite?.Play("run");
                break;
            case EnemyState.Attack:
                _animatedSprite?.Play("attack");
                break;
        }
    }

    /// <summary>
    /// 追击玩家
    /// </summary>
    /// <param name="distanceToPlayer"></param>
    /// <param name="delta"></param>
    private void StateRun(float distanceToPlayer, double delta)
    {
        if (distanceToPlayer > DetectionRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }
        Vector2 direction = (_player.GlobalPosition - GlobalPosition).Normalized();
        float moveX = direction.X * ChaseSpeed;
        if (IsGroundAhead())
        {
            if (IsPlayer())
            {
                ChangeState(EnemyState.Attack);
            }

            Velocity = new Vector2(moveX, Velocity.Y);
            MoveAndSlide();

            if (_animatedSprite != null)
            {
                _animatedSprite.FlipH = moveX > 0;
            }

            if (_animatedSprite.Animation != "run")
                _animatedSprite.Play("run");
        }
        else
        {
            // 没有地面，回到出生点
            ChangeState(EnemyState.Idle);
        }
    }

    // 回出生点 / 待机
    private void StateIdle(float distanceToPlayer)
    {
        if (distanceToPlayer <= DetectionRange && _needBack == false)
        {
            GD.Print("进入攻击范围");
            ChangeState(EnemyState.Run);
            return;
        }
        Vector2 toSpawn = _spawnPosition - GlobalPosition;
        float distanceToSpawn = toSpawn.Length();

        if (distanceToSpawn > 3f)
        {
            // 移动回出生点
            float moveX = (toSpawn.X >= 0 ? 1 : -1) * ReturnSpeed;
            Velocity = new Vector2(moveX, Velocity.Y);
            MoveAndSlide();

            // 翻转朝向，面向移动方向
            if (_animatedSprite != null)
            {
                _animatedSprite.FlipH = moveX > 0;
            }

            if (_animatedSprite.Animation != "run")
                _animatedSprite.Play("run");
            if (distanceToPlayer <= DetectionRange)
            {
                ChangeState(EnemyState.Run);
                _needBack = false;
            }
        }
        else
        {
            // 已经回到出生点
            Velocity = Vector2.Zero;
            MoveAndSlide();
            if (_animatedSprite.Animation != "idle")
                _animatedSprite.Play("idle");
            _needBack = false;
        }
    }

    private async Task StateAttack(float distanceToPlayer)
    {
        if (_isAttacking) return; // 防止重复执行攻击逻辑

        if (IsPlayer())
        {
            Node colliderNode = null;
            if (_animatedSprite.FlipH == false)
                colliderNode = PlayerCheckRayCastLeft.GetCollider() as Node;
            else
                colliderNode = PlayerCheckRayCastRight.GetCollider() as Node;

            if (colliderNode is Player player)
            {
                _isAttacking = true; // 标记攻击中
                _animatedSprite?.Play("attack");
                await ToSignal(_animatedSprite, AnimatedSprite2D.SignalName.AnimationFinished);
                // 攻击动画完成后执行下一步逻辑
                _isAttacking = false;

                // 再次检查距离（因为动画播放过程中玩家可能已跑远）
                float newDistance = GlobalPosition.DistanceTo(player.GlobalPosition);
                if (newDistance > DetectionRange)
                {
                    ChangeState(EnemyState.Idle);
                }
                else
                {
                    ChangeState(EnemyState.Run);
                }
                return;
            }
        }
        // 没检测到玩家时恢复 Idle 或 Run
        if (distanceToPlayer > DetectionRange)
        {
            ChangeState(EnemyState.Idle);
        }
        else
        {
            ChangeState(EnemyState.Run);
        }
    }

    private void OnFrameChanged()
    {
        if (_animatedSprite.Animation == _attackAnimation && _animatedSprite.Frame == _attackFrame)
        {
            if (_animatedSprite.FlipH == false)
                _attackAreaLeft.Monitoring = true;
            else
                _attackAreaRight.Monitoring = true;
        }
    }

    /// <summary>
    /// 动画必须播放完整
    /// </summary>
    private void OnAnimationFinished()
    {
        if (_animatedSprite.Animation == _attackAnimation)
        {
            _attackAreaLeft.Monitoring = false;
            _attackAreaRight.Monitoring = false;
        }
    }

    private void OnAttackAreaBodyEntered(Node2D body)
    {
        if (!_isAttacking) return;
        if (body is Player player)
        {
            player.TakeDamage(Damage);
        }
    }

    /// <summary>
    /// 检测地面
    /// </summary>
    /// <returns></returns>
    protected override bool IsGroundAhead()
    {
        RayCast2D ray = _animatedSprite.FlipH ? GroundCheckRayCastRight : GroundCheckRayCastLeft;
        if (ray == null) return false;
        bool result = ray.IsColliding();
        if (!result) _needBack = true;
        return result;
    }

    /// <summary>
    /// 检测玩家
    /// </summary>
    /// <returns></returns>
    private bool IsPlayer()
    {
        RayCast2D ray = _animatedSprite.FlipH ? PlayerCheckRayCastRight : PlayerCheckRayCastLeft;
        if (ray == null) return false;

        Node colliderNode = ray.GetCollider() as Node;
        return colliderNode is Player;
    }

    protected override async Task Die()
    {
        await base.Die();
        GetTree().ChangeSceneToFile("res://scenes/victoryscene.tscn");
    }
}
