using Godot;
using System;
using System.Threading.Tasks;

public partial class Coin : Area2D
{
    private bool _collected = false; // 防止重复触发
    private const float PopUpHeight = 40.0f;
    private const float PopUpDuration = 0.2f;
    private const float WaitDuration = 0.1f;
    public override void _Ready()
    {
        // 碰撞信号（如果没在编辑器里连接，可以通过脚本控制）
        BodyEntered += _on_body_entered;
    }
    public async void PopUp()
    {
        Vector2 startPos = GlobalPosition;
        Vector2 endPos = startPos - new Vector2(0, PopUpHeight);
        startPos.Y = startPos.Y - 10;
        AudioManager.Instance.Play(SoundType.CoinPickupSound);
        var tween = CreateTween();
        //  向上弹出
        tween.TweenProperty(this, "global_position", endPos, PopUpDuration / 2.0f)
             .SetTrans(Tween.TransitionType.Quad)
             .SetEase(Tween.EaseType.Out);
        // 向下收回/停顿
        tween.TweenProperty(this, "global_position", startPos, PopUpDuration / 2.0f)
             .SetTrans(Tween.TransitionType.Quad)
             .SetEase(Tween.EaseType.In);
        await ToSignal(tween, Tween.SignalName.Finished);
        await Task.Delay(TimeSpan.FromSeconds(WaitDuration));
        QueueFree();
        // 建议使用层来控制
        if (GetTree().Root.GetNodeOrNull("Game/player") is Player playerNode) 
        {
            playerNode.AddCoin(1);
        }
    }

    private void _on_body_entered(Node body)
    {
        if (_collected)
            return;
        // 只响应玩家
        if (body is CharacterBody2D)
        {
            if (body is Player p)
            {
                _collected = true;
                p.AddCoin(1);
                AudioManager.Instance.Play(SoundType.CoinPickupSound);
                QueueFree();
            }
        }
    }
}
