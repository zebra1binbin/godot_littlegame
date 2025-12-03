using Godot;
using System;
using System.Collections.Generic;

public enum SoundType
{
    CoinPickupSound,
    FootstepSound,
    JumpSound,
    WeaponSound,
    EnemyHitSound,
    PlayerHurtSound

}

public partial class AudioManager : Node
{
    private static AudioManager _instance;
    public static AudioManager Instance => _instance;

    private Dictionary<SoundType, AudioStreamPlayer2D> _soundPlayers = new();

    public override void _Ready()
    {
        if (_instance != null)
        {
            QueueFree();
            return;
        }

        _instance = this;
        //GetTree().Root.AddChild(this);
        Name = "GlobalAudio";
        foreach (Node child in GetChildren())
        {
            if (child is AudioStreamPlayer2D player)
            {
                if (Enum.TryParse<SoundType>(player.Name, out var type))
                {
                    _soundPlayers[type] = player;
                }
                else
                {
                    GD.PrintErr($"⚠️ 未识别的音效节点名：{player.Name}");
                }
            }
        }
        GD.Print("🎵 全局音效初始化完成");
    }

    /// <summary>
    /// 播放指定音效
    /// </summary>
    public void Play(SoundType type)
    {
        if (_soundPlayers.TryGetValue(type, out var player))
        {
            player.Play();
        }
        else
        {
            GD.PrintErr($"❌ 未找到音效：{type}");
        }
    }

    /// <summary>
    /// 停止播放音效
    /// </summary>
    public void Stop(SoundType type)
    {
        if (_soundPlayers.TryGetValue(type, out var player))
        {
            player.Stop();
        }
    }

    /// <summary>
    /// 设置音量（单位 dB）
    /// </summary>
    public void SetVolume(SoundType type, float db)
    {
        if (_soundPlayers.TryGetValue(type, out var player))
        {
            player.VolumeDb = db;
        }
    }
}
