using Godot;
using System.Threading.Tasks;


public enum WeaponType
{
    Sword,
    Bow
}

public abstract partial class BaseWeapon : Node2D
{
    [Export] public int Damage = 1;
    [Export] public float AttackCooldown = 0.1f; // 秒或毫秒自行约定
    public WeaponType WeaponName { get; set; }
    public bool Available { get; set; }

    protected bool _canAttack = true;


    /// <summary>
    /// 攻击方法，子类实现具体逻辑
    /// </summary>
    public abstract Task Attack();

    /// <summary>
    /// 更新武器方向
    /// </summary>
    public abstract void UpdateDirection(bool faceLeft);
}