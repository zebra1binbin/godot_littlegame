using Godot;

public partial class Game : Node2D
{
    private PauseMenu _pauseMenu;

    public override void _Ready()
    {
        Node pauseMenuNode = GetNodeOrNull("PauseMenu");
        if (pauseMenuNode == null)
        {
            return;
        }
        _pauseMenu = pauseMenuNode as PauseMenu;
        if (_pauseMenu != null)
        {
            GetTree().Paused = false;
        }
        else
        {
            return;
        }
       
    }

    public override void _Input(InputEvent @event)
    {
        if (_pauseMenu != null && @event.IsActionPressed("pause"))
        {
            bool shouldPause = !GetTree().Paused;
            _pauseMenu.TogglePause(shouldPause); 
        }
    }
}