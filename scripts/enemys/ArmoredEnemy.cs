using Godot;
using System;

public partial class ArmoredEnemy : BaseEnemy
{
    public ArmoredEnemy()
    {
        MaxHP = 5; // 比普通敌人高
        _HP = MaxHP;  // 初始化血量
    }
    public override void TakeDamage(int dmg)
    {
        base.TakeDamage(dmg); 
    }
}