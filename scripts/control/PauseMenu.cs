using Godot;

public partial class PauseMenu : CanvasLayer
{
    [Export] public string StartScenePath = "res://scenes/start.tscn";
    [Export] public string SettingsScenePath = "res://scenes/settings.tscn";

    private Button _resumeButton;
    private Button _quitButton;

    public override void _Ready()
    {
        _resumeButton = GetNode<Button>("CenterContainer/VBoxContainer/ResumeButton");
        _quitButton = GetNode<Button>("CenterContainer/VBoxContainer/QuitButton");

        _resumeButton.Pressed += OnResumeButtonPressed;
        GetNode<Button>("CenterContainer/VBoxContainer/SettingsButton").Pressed += OnSettingsButtonPressed;
        _quitButton.Pressed += OnQuitButtonPressed;
        Visible = false;
        ProcessMode = ProcessModeEnum.Always;
    }

    private void OnResumeButtonPressed()
    {
        TogglePause(false); 
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile(StartScenePath);
    }

    private void OnSettingsButtonPressed()
    {
        if (SettingsScenePath != null)
        {
            var settingsScene = GD.Load<PackedScene>(SettingsScenePath);
            Settings _settingsPanel = settingsScene.Instantiate<Settings>();
            AddChild(_settingsPanel);
            _settingsPanel.IsOpenedFromPause = true;
            _settingsPanel.Visible = true;
        }

      
    }

    public void TogglePause(bool isPaused)
    {
        GetTree().Paused = isPaused;
        Visible = isPaused;
        if (isPaused)
        {
            GD.Print("游戏暂停.");
        }
        else
        {
            GD.Print("游戏恢复.");
        }
    }
}