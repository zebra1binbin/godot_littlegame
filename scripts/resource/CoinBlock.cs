using Godot;
using System;
using System.Threading.Tasks;

public partial class CoinBlock : StaticBody2D
{
    [Export] public int CoinCount { get; set; } = 10;
    [Export] public PackedScene CoinScene { get; set; }
    public delegate void CoinHitEventHandler(int amount);
    //public event CoinHitEventHandler CoinHit;
    private Marker2D _coinSpawnPoint;
    private bool _hasBeenHit = false;
    public override void _Ready()
    {
        _coinSpawnPoint = GetNode<Marker2D>("CoinSpawnPoint");
    }

    public async void OnPlayerHitFromBottom()
    {
        if (CoinCount <= 0)
        {
            return;
        }
        else
        {
            var tween = CreateTween();
            Vector2 startPos = GlobalPosition;
            Vector2 endPos = startPos - new Vector2(0, 10);
            tween.TweenProperty(this, "global_position", endPos, 0.2 / 2.0f)
                 .SetTrans(Tween.TransitionType.Quad)
                 .SetEase(Tween.EaseType.Out);
            tween.TweenProperty(this, "global_position", startPos, 0.2 / 2.0f)
                 .SetTrans(Tween.TransitionType.Quad)
                 .SetEase(Tween.EaseType.In);
            SpawnCoin();
            await ToSignal(tween, Tween.SignalName.Finished);
            await Task.Delay(TimeSpan.FromSeconds(0.1));
            CoinCount--;
        }
    }

    private async void SpawnCoinsAsync()
    {
        while (CoinCount > 0)
        {
            SpawnCoin();
            CoinCount--; 
            await Task.Delay(TimeSpan.FromSeconds(0.15f));
        }
    }

    private void SpawnCoin()
    {
        if (CoinScene == null)
        {
            return;
        }
        if (CoinScene.Instantiate() is Node2D newCoinNode)
        {
            if (GetParent() == null)
            {
                return;
            }
            GetParent().AddChild(newCoinNode);
            newCoinNode.GlobalPosition = _coinSpawnPoint.GlobalPosition;
            if (newCoinNode is Coin coinNode)
            {
                coinNode.PopUp();
            }
        }
    }
}