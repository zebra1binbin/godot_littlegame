using Godot;
using System;


// =====================
//   金币UI界面
// =====================
public partial class CoinIcon : Node2D
{
    private Label _coinCountLabel;
    private Player _player;
    public override void _Ready()
	{
        _coinCountLabel = GetNode<Label>("HBoxContainer/Label");
        _player = GetTree().Root.GetNode<Player>("Game/player"); 
        if (_coinCountLabel == null)
        {
            return;
        }
        if (_player != null)
        {
            _player.CoinsChanged += OnCoinsChanged;
            OnCoinsChanged(0); 
        }
    }

    private void OnCoinsChanged(int newCoinCount)
    {
        _coinCountLabel.Text = $"  {newCoinCount}";
    }

}
