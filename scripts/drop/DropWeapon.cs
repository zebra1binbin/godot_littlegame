using Godot;
using System;
using System.Collections.Generic;

public partial class DropWeapon : Area2D
{
    private AnimatedSprite2D _animatedSprite;
    private CollisionShape2D _collisionShape;

    private struct WeaponInfo
    {
        public string FramesPath;
        public string ScenePath;
    }
    /// <summary>
    /// 武器类型
    /// </summary>
    private Dictionary<WeaponType, WeaponInfo> _weaponData = new()
    {
        { WeaponType.Sword, new WeaponInfo { FramesPath = "res://assets/resource/frames/SwordFrames.tres", ScenePath = "res://scenes/weapons/Sword.tscn" } },
        { WeaponType.Bow,   new WeaponInfo { FramesPath = "res://assets/resource/frames/BowFrames.tres",   ScenePath = "res://scenes/weapons/Bow.tscn" } }
    };

    private const float TARGET_WIDTH = 15f;  // 统一显示尺寸
    public WeaponType WeaponName { get; private set; }
    private bool _weaponSetManually = false;

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

        // 如果没有手动指定武器，则随机选择
        if (!_weaponSetManually)
        {
            WeaponType[] keys = new List<WeaponType>(_weaponData.Keys).ToArray();
            WeaponName = keys[GD.Randi() % keys.Length];
        }

        WeaponInfo info = _weaponData[WeaponName];
        var frames = GD.Load<SpriteFrames>(info.FramesPath);
        _animatedSprite.SpriteFrames = frames;
        _animatedSprite.Play("idle");
        AdjustSpriteScaleToTargetWidth(TARGET_WIDTH);
        AutoFitCollisionToSprite();

        BodyEntered += OnBodyEntered;
    }

    /// <summary>
    /// 设置掉落武器类型（运行时调用）
    /// </summary>
    public void SetWeapon(WeaponType weaponType)
    {
        if (!_weaponData.ContainsKey(weaponType))
        {
            GD.PrintErr($"武器类型 {weaponType} 不存在");
            return;
        }

        WeaponName = weaponType;
        _weaponSetManually = true;
        if (_animatedSprite != null)
        {
            var frames = GD.Load<SpriteFrames>(_weaponData[weaponType].FramesPath);
            _animatedSprite.SpriteFrames = frames;
            _animatedSprite.Play("idle");

            AdjustSpriteScaleToTargetWidth(TARGET_WIDTH);
            AutoFitCollisionToSprite();
        }
    }

    private void OnBodyEntered(Node body)
    {
        if (body is Player player)
        {
            player.CallDeferred("PickupWeapon", (int)WeaponName);
            CallDeferred("queue_free");
        }
    }

    private void AdjustSpriteScaleToTargetWidth(float targetWidth)
    {
        if (_animatedSprite == null || _animatedSprite.SpriteFrames == null)
            return;

        var tex = _animatedSprite.SpriteFrames.GetFrameTexture(_animatedSprite.Animation, 0);
        if (tex == null)
            return;

        float width = tex.GetSize().X;
        float scaleFactor = targetWidth / width;
        _animatedSprite.Scale = new Vector2(scaleFactor, scaleFactor);
    }

    private void AutoFitCollisionToSprite()
    {
        if (_animatedSprite == null || _collisionShape == null)
            return;

        var tex = _animatedSprite.SpriteFrames.GetFrameTexture(_animatedSprite.Animation, 0);
        if (tex == null)
            return;

        Vector2 texSize = tex.GetSize() * _animatedSprite.Scale;
        var shape = new RectangleShape2D { Size = texSize };
        _collisionShape.Shape = shape;
        _collisionShape.Position = Vector2.Zero;
    }
}
