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
    
    [Export]
    private int _fallAcceleration { get; set; } = 0;
    [Export] 
    private int _speed { get; set; } = 10;
    
    private Vector3 _targetVelocity = Vector3.Zero;

    private float _cameraRotationY = 0f;
    private float _cameraRotationX = 0f;
    private Vector3 _rotatedDirection = Vector3.Zero;
    
    

    private float _yaw = 0f;
    private float _pitch = 0f;
    
    public override void _Ready()
    {
        base._Ready();
        Input.MouseMode = Input.MouseModeEnum.Captured;
        camera = GetNode<Camera3D>("Camera3D");
    }

    public override void _PhysicsProcess(double delta)
    {
        var direction = Vector3.Zero;
        var cameraYRotation = camera.Rotation.Y;
        if (Input.IsActionPressed("move_right"))
            direction.X += 1.0f;
        if (Input.IsActionPressed("move_left"))
            direction.X -= 1.0f;
        if (Input.IsActionPressed("move_back"))
            direction.Z += 1.0f;
        if (Input.IsActionPressed("move_forward"))
            direction.Z -= 1.0f;

        if (direction != Vector3.Zero)
            direction = direction.Normalized();

        // If you want movement relative to camera, rotate direction by camera's Y rotation here

        _cameraRotationY = camera.Rotation.Y;
        _cameraRotationX = camera.Rotation.X;
        
        //Rotated(Vector3.Up, _cameraRotationY);
       // _rotatedDirection = direction.X.Rotated(Vector3.Up, _cameraRotationY);
        _targetVelocity.X = direction.X * _speed;
        _targetVelocity.Z = direction.Z * _speed;

        // Apply gravity
        if (!IsOnFloor())
            _targetVelocity.Y -= _fallAcceleration * (float)delta;
        else
            _targetVelocity.Y = 0;

        Velocity = _targetVelocity;
        MoveAndSlide();
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
