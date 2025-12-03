using Godot;
using System;

/// <summary>
/// 暂时未使用
/// </summary>
[GlobalClass]
public partial class EnemyConfig : Resource
{
    [Export] public float Speed = 60f;
    [Export] public int Damage = 1;
    [Export] public int MaxHP = 1;
    [Export] public float EdgeCheckDistance = 8f;
    [Export] public float DamageCooldown = 0.3f;
    [Export] public PackedScene DropWeaponScene;
}