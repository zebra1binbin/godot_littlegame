using Godot;
using System.Collections.Generic;

public partial class HeartDisplay : Node2D
{
    [Export] public PackedScene HeartPrefab; // 每颗心的预制体
    [Export] public int Spacing = 72;        // 心之间间距
    private Player _player;
    private List<Sprite2D> _hearts = new();

    public override void _Ready()
    {
        if (HeartPrefab == null)
        {
            return;
        }
        _player = GetTree().Root.GetNode<Player>("Game/player");
        _player.HpChanged += OnHpChanged;
        GenerateHearts(_player.MaxHP);
        UpdateHearts(_player.CurrentHP);
    }

    private void GenerateHearts(int maxHP)
    {
        foreach (var heart in _hearts)
            heart.QueueFree();
        _hearts.Clear();

        for (int i = 0; i < maxHP; i++)
        {
            var heartInstance = (Sprite2D)HeartPrefab.Instantiate();
            heartInstance.Position = new Vector2(i * Spacing, 0);
            AddChild(heartInstance);
            _hearts.Add(heartInstance);
        }
    }

    private void OnHpChanged(int currentHP, int maxHP)
    {
        UpdateHearts(currentHP);
    }

    private void UpdateHearts(int currentHP)
    {
        for (int i = 0; i < _hearts.Count; i++)
        {
            _hearts[i].Visible = i < currentHP; // 血量以内显示心，其他隐藏
        }
    }
}
