using Godot;
using System;

public partial class Killzone : Area2D
{
    private Timer _timer;
    private Player _playerRef;

    public override void _Ready()
    {
        _timer = GetNode<Timer>("Timer");
        _timer.Timeout += OnTimerTimeout;
        BodyEntered += OnBodyEntered;
    }

    public void OnBodyEntered(Node body)
    {
        if (body is Player player)
        {
            _playerRef = player;

            // 慢动作效果
            Engine.TimeScale = 0.5;
            HandlePlayerDeath(player);
            _timer.Start();
        }
    }


    public void OnTimerTimeout()
    {
        Engine.TimeScale = 1.0;
        GetTree().ReloadCurrentScene();
    }

    private async void HandlePlayerDeath(Player player)
    {
        await player.Die();
    }



}
