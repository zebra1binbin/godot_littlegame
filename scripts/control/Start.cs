using Godot;

public partial class Start : Control
{
    [Export] public string GameScenePath = "res://scenes/game.tscn";
    [Export] public string SettingScenePath = "res://scenes/settings.tscn";
    [Export] public string ExplanationScenePath = "res://scenes/explanation.tscn";

    private Button _startButton;
    private Button _settingsButton;
    private Button _explanationButton;
    private Button _quitButton;

    public override void _Ready()
    {
        _startButton = GetNode<Button>("CenterContainer/VBoxContainer/StartButton");
        _settingsButton = GetNode<Button>("CenterContainer/VBoxContainer/SettingsButton");
        _explanationButton = GetNode<Button>("CenterContainer/VBoxContainer/ExplanationButton");
        _quitButton = GetNode<Button>("CenterContainer/VBoxContainer/QuitButton");
        _startButton.Pressed += OnStartButtonPressed;
        _settingsButton.Pressed += OnSettingsButtonPressed;
        _explanationButton.Pressed += OnExplanationButtonPressed;
        _quitButton.Pressed += OnQuitButtonPressed;
    }

    private void OnStartButtonPressed()
    {
        if (!ResourceLoader.Exists(GameScenePath))
        {
            GD.PrintErr($"FATAL ERROR: {GameScenePath}");
            return;
        }

        Error error = GetTree().ChangeSceneToFile(GameScenePath);

        if (error != Error.Ok)
        {
            GD.PrintErr($"Error: {error}");
        }
    }


    private void OnSettingsButtonPressed()
    {
        if (!ResourceLoader.Exists(SettingScenePath))
        {
            return;
        }
        Error error = GetTree().ChangeSceneToFile(SettingScenePath);
        if (error != Error.Ok)
        {
            GD.PrintErr($"Error: {error}");
        }
    }


    private void OnExplanationButtonPressed()
    {
        if (!ResourceLoader.Exists(ExplanationScenePath))
        {
            return;
        }
        Error error = GetTree().ChangeSceneToFile(ExplanationScenePath);
        if (error != Error.Ok)
        {
            GD.PrintErr($"Error: {error}");
        }
    }


    private void OnQuitButtonPressed()
    {
        GD.Print("退出游戏...");
        GetTree().Quit();
    }
}