using Godot;
using System;

public partial class Player : CharacterBody3D
{
    private Camera3D camera;
    
    [Export]
    private float _mouseSensitivityYaw = 0.6f;
    [Export]
    private float _mouseSensitivityPitch = 0.5f;
    [Export]
    private float _minPitch = -90f;
    [Export]
    private float _maxPitch = 90f;
    
    [Export]
    private int _fallAcceleration { get; set; } = 0;
    [Export] 
    private int _speed { get; set; } = 7;

    [Export] private float MaxZVel = 3;
    [Export] private float MaxXVel = 0;
    
    [Export]
    private float ACCELERATION = 1.99f;
    [Export]
    private float DECELERATION = 1.9999f;
    
    private Vector3 _targetVelocity = Vector3.Zero;

    private float _cameraRotationY = 0f;
    private float _cameraRotationX = 0f;
    private Vector3 _rotatedDirection = Vector3.Zero;

    private Vector3 previousInputDirection = Vector3.Zero;
    private bool inputIsOppositeX = false;
    private bool inputIsOppositeZ = false;
    

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
        inputIsOppositeX = false;
        inputIsOppositeZ = false;
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
        
        // did inputs flip
        if (direction.X != previousInputDirection.X)
        {
            inputIsOppositeX = true;
        }
        
        if (direction.Z != previousInputDirection.Z)
        {
            inputIsOppositeZ = true;
        }
        
        previousInputDirection = direction;
        
        RotateCamera(_yaw, _pitch);
        
        // If you want movement relative to camera, rotate direction by camera's Y rotation here
        _cameraRotationY = camera.Rotation.Y;
        _cameraRotationX = camera.Rotation.X;

        direction = direction.Rotated(Vector3.Up, _cameraRotationY);
        
        GD.Print(direction);
        
        //Rotated(Vector3.Up, _cameraRotationY);
       // _rotatedDirection = direction.X.Rotated(Vector3.Up, _cameraRotationY);
       
       
       _targetVelocity = AccelOrDecelerate(Velocity, direction, delta, inputIsOppositeX, inputIsOppositeZ); 
       
       // this works
       // _targetVelocity.X = direction.X * _speed;
       // _targetVelocity.Z = direction.Z * _speed;

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
      
       // GD.Print(mouseMotion.Relative);
            
        _yaw -= mouseMotion.Relative.X * _mouseSensitivityYaw;
        _pitch -= mouseMotion.Relative.Y * _mouseSensitivityPitch;
        _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
        
        //RotationDegrees = new Vector3(_pitch, _yaw, 0f);
    }

    private void RotateCamera(float yaw, float pitch)
    {
        camera.RotationDegrees = new Vector3(pitch, yaw, 0f);
    }

    private Vector3 AccelOrDecelerate(Vector3 vel, Vector3 dir, double deltaTime, bool inputOppositeXdir, bool inputOppositeZdir)
    {
        float singleDelta = (float)deltaTime;
        /*if (dir != Vector3.Zero)
        {
            var target_velocity = new Vector3();
            target_velocity.X = dir.X * _speed;
            target_velocity.Z = dir.Z * _speed;
           // vel.X = Single.Lerp(vel.X, target_velocity.X, ACCELERATION * singleDelta);
           // vel.Z = Single.Lerp(vel.Z, target_velocity.Z, ACCELERATION * singleDelta);
            vel.X = Mathf.MoveToward(vel.X, target_velocity.X, ACCELERATION * singleDelta);
            vel.Z = Mathf.MoveToward(vel.Z, target_velocity.Z, ACCELERATION * singleDelta);
        }
        else
        {
            vel.X = Mathf.MoveToward(vel.X, 0f, DECELERATION * singleDelta);
            vel.Z = Mathf.MoveToward(vel.Z, 0f, DECELERATION * singleDelta);
           // GD.Print(vel.Z);
            
        }*/
        
        if (dir.X != 0f)
        {
            var target_velocity = new Vector3();
            target_velocity.X = dir.X * _speed;
            
            // vel.X = Single.Lerp(vel.X, target_velocity.X, ACCELERATION * singleDelta);
            // vel.Z = Single.Lerp(vel.Z, target_velocity.Z, ACCELERATION * singleDelta);
            vel.X = Mathf.MoveToward(vel.X, target_velocity.X, ACCELERATION * singleDelta);
           
        }
        else
        {
            vel.X = Mathf.MoveToward(vel.X, 0f, DECELERATION * singleDelta);
           // GD.Print(vel.Z);
        }
        
        if (dir.Z != 0f)
        {
            var target_velocity = new Vector3();
            target_velocity.Z = dir.Z * _speed;
            // vel.Z = Single.Lerp(vel.Z, target_velocity.Z, ACCELERATION * singleDelta);
            vel.Z = Mathf.MoveToward(vel.Z, target_velocity.Z, ACCELERATION * singleDelta);
           
        }
        else
        {
            vel.Z = Mathf.MoveToward(vel.Z, 0f, DECELERATION * singleDelta);
            // GD.Print(vel.Z);
            
        }
        // Vector2 absVel = new Vector2(vel.X, vel.Z);
        // absVel.X = Mathf.Abs(absVel.X);
        // absVel.Y = Mathf.Abs(absVel.Y);
        // //CAREFUL absVel.Y is going to standin for Z
        // vel.Z = (absVel.Y > MaxZVel) ? MaxZVel : vel.Z;
        // vel.X = (absVel.X > MaxXVel) ? MaxXVel : vel.X;

        GD.Print(vel.X);
        return vel;
    }
}
