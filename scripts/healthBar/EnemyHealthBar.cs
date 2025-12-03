using Godot;

public partial class EnemyHealthBar : Node2D
{
    private TextureProgressBar _bar;

    public override void _Ready()
    {
        _bar = GetNode<TextureProgressBar>("TextureProgressBar");
    }

    public void Initialize(int maxValue)
    {
        _bar.MaxValue = maxValue;
        _bar.Value = maxValue;
    }

    public void SetValue(int value)
    {
        _bar.Value = value;
    }
}
