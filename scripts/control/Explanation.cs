using Godot;
using System;

public partial class Explanation : Control
{
 
    [Export] public string StartScenePath = "res://scenes/start.tscn";
    private Button _backButton;
    public override void _Ready()
    {
        _backButton = GetNode<Button>("MarginContainer/VBoxContainer/BackButton");
        _backButton.Pressed += OnBackButtonPressed;
        Visible = true;
    }

    private void OnBackButtonPressed()
    {
        GetTree().ChangeSceneToFile(StartScenePath);
    }
}
