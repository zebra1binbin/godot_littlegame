using Godot;

public partial class Settings : Control
{
    private HSlider _musicSlider;
    private HSlider _sfxSlider;
    private Button _backButton;

    [Export] public string StartScenePath = "res://scenes/start.tscn";
    public bool IsOpenedFromPause = false;
    public override void _Ready()
    {
        _musicSlider = GetNode<HSlider>("MarginContainer/VBoxContainer/VBoxContainer_Volume/HBoxContainer_Music/HSlider");
        _sfxSlider = GetNode<HSlider>("MarginContainer/VBoxContainer/VBoxContainer_Volume/HBoxContainer_SFX/HSlider");
        _backButton = GetNode<Button>("MarginContainer/VBoxContainer/BackButton");
        InitializeVolumeSliders();

        _musicSlider.ValueChanged += OnMusicSliderValueChanged;
        _sfxSlider.ValueChanged += OnSfxSliderValueChanged;
        _backButton.Pressed += OnBackButtonPressed;
        ProcessMode = ProcessModeEnum.Always;
        Visible = true;
    }

    private void InitializeVolumeSliders()
    {
        int musicBusIndex = AudioServer.GetBusIndex("music");
        int sfxBusIndex = AudioServer.GetBusIndex("SFX");
        if (musicBusIndex != -1)
        {
            _musicSlider.Value = AudioServer.GetBusVolumeDb(musicBusIndex);
        }
        if (sfxBusIndex != -1)
        {
            _sfxSlider.Value = AudioServer.GetBusVolumeDb(sfxBusIndex);
        }
    }


    private void OnMusicSliderValueChanged(double value)
    {
        int musicBusIndex = AudioServer.GetBusIndex("music");
        if (musicBusIndex != -1)
        {
            AudioServer.SetBusVolumeDb(musicBusIndex, (float)value);
        }
    }

    private void OnSfxSliderValueChanged(double value)
    {
        int sfxBusIndex = AudioServer.GetBusIndex("SFX");
        if (sfxBusIndex != -1)
        {
            AudioServer.SetBusVolumeDb(sfxBusIndex, (float)value);
        }
    }

    private void OnBackButtonPressed()
    {
        if (IsOpenedFromPause)
        {
            QueueFree();
            if (GetTree().Root.GetNodeOrNull("Game/PauseMenu") is PauseMenu pauseMenu)
            {
                pauseMenu.Visible = true;
            }
        }
        else
        {
            GetTree().ChangeSceneToFile(StartScenePath);
        }
    }
}