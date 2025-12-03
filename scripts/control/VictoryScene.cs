using Godot;
using System;

public partial class VictoryScene : CanvasLayer
{
    [Export] public string MainMenuScene = "res://scenes/start.tscn";
    private Label _cointitle;
    private Button _menuButton;

    public override void _Ready()
	{
        _cointitle = GetNode<Label>("CenterContainer/VBoxContainer/Coin");
        _menuButton = GetNode<Button>("CenterContainer/VBoxContainer/QuitButton");
        _cointitle.Text = $" 金币： {Global.PassedCoins}";
        _menuButton.Pressed += OnMenuButtonPressed;
    }


    private void OnMenuButtonPressed()
    {
        if (!ResourceLoader.Exists(MainMenuScene))
        {
            GD.PrintErr($"FATAL ERROR: {MainMenuScene}");
            return;
        }

        Error error = GetTree().ChangeSceneToFile(MainMenuScene);

        if (error != Error.Ok)
        {
            GD.PrintErr($"Error: {error}");
        }
    }

}
