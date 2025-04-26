using Godot;
using System;

public partial class Player : CharacterBody3D
{
    private Camera3D camera;
    
    [Export]
    private float _mouseSensitivity = 1f;
    [Export]
    private float _minPitch = -90f;
    [Export]
    private float _maxPitch = 90f;

    private float _yaw = 0f;
    private float _pitch = 0f;
    
    public override void _Ready()
    {
        base._Ready();
        Input.MouseMode = Input.MouseModeEnum.Captured;
        camera = GetNode<Camera3D>("Camera3D");
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        
        var mouseMotion = @event as InputEventMouseMotion;
        if (mouseMotion == null) return;
      
        GD.Print(mouseMotion.Relative);
            
        _yaw -= mouseMotion.Relative.X * _mouseSensitivity;
        _pitch -= mouseMotion.Relative.Y * _mouseSensitivity;
        _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
        
        RotationDegrees = new Vector3(_pitch, _yaw, 0f);
    }
}
