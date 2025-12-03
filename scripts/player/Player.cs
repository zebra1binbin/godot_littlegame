using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class Player : CharacterBody2D
{
    [Export] public int MaxHP = 3;
    [Export] public Node2D WeaponMount;   // 挂载所有武器的节点
    [Export] public float DamageCooldown = 1.0f; // 受伤后1秒无敌

    public int CurrentHP { get; private set; }
    public const float Speed = 130.0f;
    public const float JumpVelocity = -300.0f;
    private AnimatedSprite2D _animatedSprite;
    private int _lastFootstepFrame = -1;
    private bool _canTakeDamage = true;
    private bool _isDead = false;  // 死亡锁定
    private bool _hasPlayedJumpSound = false;
    public bool IsDead => _isDead;
    private bool _isHurt = false;
    private bool _isFlashing = false;
    private readonly Color _hurtColor = new Color(1, 0.3f, 0.3f); 
    private readonly Color _normalColor = Colors.White;      


    [Signal] 
    public delegate void HpChangedEventHandler(int currentHP, int maxHP);
    [Signal]
    public delegate void CoinsChangedEventHandler(int newCoinCount);

    private int _coinCount = 0;
    private BaseWeapon _currentWeapon;
    private Dictionary<WeaponType, BaseWeapon> _weapons = new();
    private RayCast2D _ceilingChecker;

    /// <summary>
    /// 状态枚举
    /// </summary>
    private enum PlayerState
    {
        Idle,
        Run,
        Jump,
        Attack
    }

    private PlayerState _state = PlayerState.Idle;

    public override void _Ready()
    {
        //GD.Print(GetTree().Root.GetPath());
        //GD.Print(GetTree().CurrentScene.GetPath()); // 打印当前场景节点路径
        //GD.Print("所有 player 组节点数量: " + GetTree().GetNodesInGroup("player").Count);
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _ceilingChecker = GetNode<RayCast2D>("CeilingChecker");
        CurrentHP = MaxHP;
        EmitSignal(SignalName.HpChanged, CurrentHP, MaxHP);
        // 自动初始化挂载节点下的武器
        foreach (Node child in WeaponMount.GetChildren())
        {
            if (child is BaseWeapon weapon)
            {
                weapon.Visible = false;
                _weapons[weapon.WeaponName] = weapon;
            }
        }
        _currentWeapon = null; // 初始没有武器
     
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_isDead) return;
        Vector2 velocity = Velocity;
        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        }

        if (velocity.Y < 0)
        {
            CheckCoinBlockHit(ref velocity);
        }

        switch (_state)
        {
            case PlayerState.Idle:
                StateIdle(ref velocity);
                break;

            case PlayerState.Run:
                StateRun(ref velocity);
                break;

            case PlayerState.Jump:
                StateJump(ref velocity);
                break;

            case PlayerState.Attack:
                StateAttack(ref velocity);
                break;
        }

        //在 MoveAndSlide 前检测踩敌人
        if (Velocity.Y > 0)
        {
            CheckStomp(ref velocity);
        }
        Velocity = velocity;
        MoveAndSlide();
        UpdateStateTransitions(velocity);
        if (Input.IsActionJustPressed("switch_weapon"))
        {
            SwitchWeapon();
        }
    }

    /// <summary>
    /// 静止状态
    /// </summary>
    /// <param name="velocity"></param>
    private void StateIdle(ref Vector2 velocity)
    {
        if (_isDead) return;
        Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        // 攻击
        if (Input.IsActionJustPressed("attack"))
        {
            ChangeState(PlayerState.Attack);
            return;
        }

        // 跳跃
        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
            ChangeState(PlayerState.Jump);
            return;
        }

        // 移动
        if (direction.X != 0)
        {
            ChangeState(PlayerState.Run);
            return;
        }
        // 静止
        if (_animatedSprite.Animation != "idle")
        {
            _animatedSprite.Play("idle");
            return;
        }
    }

    /// <summary>
    /// 移动状态
    /// </summary>
    /// <param name="velocity"></param>
    private void StateRun(ref Vector2 velocity)
    {
        if (_isDead) return;
        Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");

        // 攻击
        if (Input.IsActionJustPressed("attack"))
        {
            ChangeState(PlayerState.Attack);
            return;
        }

        // 跳跃
        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
            ChangeState(PlayerState.Jump);
            return;
        }

        // 移动
        if (direction.X != 0)
        {
            velocity.X = direction.X * Speed;

            _animatedSprite.FlipH = direction.X < 0;
            _currentWeapon?.UpdateDirection(_animatedSprite.FlipH);

            if ( _animatedSprite.Animation != "run")
                _animatedSprite.Play("run");

            PlayFootstepSound();
        }
        else
        {
            // 停止移动
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            if (Mathf.Abs(velocity.X) < 1f)
                ChangeState(PlayerState.Idle);
        }
    }

    /// <summary>
    /// 跳跃状态
    /// </summary>
    /// <param name="velocity"></param>
    private void StateJump(ref Vector2 velocity)
    {
        if (_isDead) return;
        Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");

        velocity.X = direction.X * Speed;

        if (_animatedSprite.Animation != "jump")
        {
            _animatedSprite.Play("jump");
        }

        // 攻击
        if (Input.IsActionJustPressed("attack"))
        {
            ChangeState(PlayerState.Attack);
            return;
        }

        // 落地自动切回 Idle/Run
        if (IsOnFloor())
        {
            ChangeState(Mathf.Abs(direction.X) > 0 ? PlayerState.Run : PlayerState.Idle);
        }
    }

    /// <summary>
    /// 攻击状态
    /// </summary>
    /// <param name="velocity"></param>
    private void StateAttack(ref Vector2 velocity)
    {
        if (_isDead) return;
        // 攻击时：锁定水平速度，保留重力
        velocity.X = 0;
    }

    /// <summary>
    /// 踩踏攻击
    /// </summary>
    /// <param name="velocity"></param>
    private void CheckStomp(ref Vector2 velocity)
    {
        // 玩家脚下位置
        Transform2D transform = GlobalTransform;
        Vector2 offset = new Vector2(0, -3); // 脚下偏移
        Transform2D testTransform = transform.Translated(offset);

        // 检测向下
        PhysicsTestMotionParameters2D testParams = new PhysicsTestMotionParameters2D()
        {
            From = testTransform,
            Motion = new Vector2(0, 8)        // 向下检测距离
        };

        PhysicsTestMotionResult2D result = new PhysicsTestMotionResult2D();
        bool hit = PhysicsServer2D.BodyTestMotion(GetRid(), testParams, result);

        if (hit)
        {
            Node colliderNode = result.GetCollider() as Node;
            if (colliderNode is IDamageable enemy)
            {
                enemy.TakeDamage(1);
                velocity.Y = JumpVelocity * 1.2f; // 弹起
            }
        }
    }

    /// <summary>
    /// 状态切换
    /// </summary>
    /// <param name="newState"></param>
    private async void ChangeState(PlayerState newState)
    {
        if (_state == newState)
            return;

        _state = newState;

        switch (newState)
        {
            case PlayerState.Idle:
                _animatedSprite.Play("idle");
                break;

            case PlayerState.Run:
                _animatedSprite.Play("run");
                break;

            case PlayerState.Jump:
                _animatedSprite.Play("jump");
                if (!_hasPlayedJumpSound)
                {
                    AudioManager.Instance.Play(SoundType.JumpSound);
                    _hasPlayedJumpSound = true;
                }
                break;

            case PlayerState.Attack:
                _animatedSprite.Play("attack");
                if (_currentWeapon != null)
                    await _currentWeapon.Attack(); 
                // 攻击结束：落地回 idle，否则跳跃
                if (IsOnFloor())
                {
                    ChangeState(PlayerState.Idle);
                    _hasPlayedJumpSound = false;
                }
                else
                {
                    ChangeState(PlayerState.Jump);
                }
                break;
        }
    }

    private void UpdateStateTransitions(Vector2 velocity)
    {
        if (_state == PlayerState.Attack && !IsOnFloor())
        {
            // 不强制切回，允许空中攻击继续
        }
        // 落地时重置跳跃音效标记
        if (IsOnFloor() && _hasPlayedJumpSound)
        {
            _hasPlayedJumpSound = false;
        }
    }

    /// <summary>
    /// 走路声效
    /// </summary>
    private void PlayFootstepSound()
    {
        if (!IsOnFloor())
            return;

        int currentFrame = _animatedSprite.Frame;
        //第0帧和第4帧发去哒哒哒的走路声
        if ((currentFrame == 0 || currentFrame == 4) && currentFrame != _lastFootstepFrame)
        {
            AudioManager.Instance.Play(SoundType.FootstepSound);
            _lastFootstepFrame = currentFrame;
        }
        if (currentFrame != _lastFootstepFrame)
            _lastFootstepFrame = -1;
    }


    /// <summary>
    /// 受伤
    /// </summary>
    /// <param name="dmg"></param>
    public async void TakeDamage(int dmg)
    {
        if (!_canTakeDamage || CurrentHP <= 0)
            return;
        _canTakeDamage = false;
        CurrentHP = Math.Max(CurrentHP - dmg, 0);
        EmitSignal(SignalName.HpChanged, CurrentHP, MaxHP);
        AudioManager.Instance.Play(SoundType.PlayerHurtSound);
        _ = FlashRed();
        if (CurrentHP <= 0)
        {
            await Die();
            GetTree().ReloadCurrentScene();
            return;
        }
        // 受伤冷却
        await ToSignal(GetTree().CreateTimer(DamageCooldown), "timeout");
        _canTakeDamage = true;
    }
    public void Heal(int amount)
    {
        CurrentHP = Math.Min(CurrentHP + amount, MaxHP);
        EmitSignal(SignalName.HpChanged, CurrentHP, MaxHP);
    }


    public async Task Die()
    {
        if (_isDead) return;

        _isDead = true;
        //// 停用武器检测
        //Weapon.HitBox.Monitoring = false;

        // 播放死亡动画
        _animatedSprite?.Play("die");

        //// 播放死亡音效
        //weaponSound?.Play();
        await ToSignal(_animatedSprite, AnimatedSprite2D.SignalName.AnimationFinished);

        CallDeferred("queue_free");
        //// 重置场景或者其他处理
        //GetTree().ReloadCurrentScene();
    }

    /// <summary>
    /// 复活机制
    /// </summary>
    public void Respawn()
    {
        GD.Print("Player respawned!");
    }


    private bool InitializationCheck()
    {
        if (_currentWeapon == null)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 捡起武器
    /// </summary>
    /// <param name="weaponint"></param>
    public void PickupWeapon(int weaponint)
    {
        var weapon = (WeaponType)weaponint;
        if (_weapons.TryGetValue(weapon, out BaseWeapon weaponnode))
        {
            weaponnode.Visible = false; // 拾起后先隐藏
            weaponnode.Available = true;
        }
        // 自动装备新武器
        EquipWeapon(weapon);
    }


    private void EquipWeapon(WeaponType weaponName)
    {
        if (!_weapons.TryGetValue(weaponName, out BaseWeapon weapon) || !weapon.Available)
            return;
        foreach (var kv in _weapons.Values)
            kv.Visible = false;
        weapon.Visible = true;
        _currentWeapon = weapon;
        weapon.UpdateDirection(_animatedSprite.FlipH);
    }

    /// <summary>
    /// 切换武器
    /// </summary>
    private void SwitchWeapon()
    {
        if (_currentWeapon == null || _weapons.Count <= 1)
            return;

        var keys = new List<WeaponType>(_weapons.Keys);
        int index = keys.IndexOf(_currentWeapon.WeaponName);
        index = (index + 1) % keys.Count;
        if (_weapons.TryGetValue(keys[index], out BaseWeapon weapon))
        {
            if (weapon.Available) 
            {
                EquipWeapon(keys[index]);
            }
        }
    }


   
    /// <summary>
    /// 顶金币石头
    /// </summary>
    /// <param name="velocity"></param>
    private void CheckCoinBlockHit(ref Vector2 velocity)
    {
        if (velocity.Y >= 0)
        {
            return;
        }
        if (_ceilingChecker.IsColliding())
        {
            Node colliderNode = _ceilingChecker.GetCollider() as Node;
            if (colliderNode is CoinBlock coinBlock)
            {
                coinBlock.OnPlayerHitFromBottom();
                velocity.Y = 0;
                velocity.Y = -JumpVelocity * 0.1f;
                return;
            }
        }
    }

  
    /// <summary>
    /// 金币
    /// </summary>
    /// <param name="amount"></param>
    public void AddCoin(int amount)
    {
        _coinCount += amount;
        Global.PassedCoins = _coinCount;
        EmitSignal(SignalName.CoinsChanged, _coinCount);
    }

    /// <summary>
    /// 受伤闪烁
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    private async Task FlashRed(float time = 0.3f)
    {
        if (_isFlashing) return;
        _isFlashing = true;
        _animatedSprite.Modulate = _hurtColor;  
        await ToSignal(GetTree().CreateTimer(time), "timeout");
        _animatedSprite.Modulate = _normalColor; //回复颜色
        _isFlashing = false;
    }

}
