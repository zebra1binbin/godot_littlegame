using Godot;
using System.Reflection.Emit;

// =====================
//   怪物血条
// =====================
public partial class EnemyInfoPanel : Node2D
{
    private TextureProgressBar _bar;
    private Godot.Label _nameLabel;

    public override void _Ready()
    {
        _nameLabel = GetNode<Godot.Label>("NameLabel");
        _bar = GetNode<TextureProgressBar>("HealthBar");
    }

    public void SetLableName(string name)
    {
        _nameLabel.Text = name;
    }

    public void InitializeHealth(int maxHp)
    {
        _bar.MaxValue = maxHp;
        _bar.Value = maxHp;
    }

    public void SetHealth(int hp)
    {
        _bar.Value = hp;
    }
}
