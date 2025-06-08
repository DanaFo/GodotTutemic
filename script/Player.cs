using Godot;
using System;

public partial class Player : CharacterBody3D
{
    private Camera3D camera;
    private PhysicsDirectSpaceState3D spaceState;
    private RayCast3D groundRay;
    [Export] private Curve angleVsAccelCurve;
    [Export] private float _mouseSensitivityYaw = 0.6f;
    [Export] private float _mouseSensitivityPitch = 0.5f;
    [Export] private float _minPitch = -90f;
    [Export] private float _maxPitch = 90f;
    
    [Export] private int _fallAcceleration { get; set; } = 20500;
    [Export] private float _jumpForce { get; set; } = 23f;
    [Export] private int _speed { get; set; } = 7;
    [Export] private float _airControl { get; set; } = 0.2f;

    [Export] private float _MAX_SPEED = 7;
    [Export] private float _MAX_AIR_SPEED = 2;

    [Export] private float _MAX_ACCEL;

    [Export] private float _MAX_Z_VEL = 10;
    [Export] private float _MAX_X_VEL = 10;
    
    [Export]
    private float ACCELERATION = 1.99f;
    [Export]
    // FAIRLY SURE THIS IS A PERCENT OF 60FPS - lower more GROUND_DECEL applied
    private float GROUND_DECEL = 54f;
    
    private Vector3 _targetVelocity = Vector3.Zero;

    private float _cameraRotationY = 0f;
    private float _cameraRotationX = 0f;
    private Vector3 _rotatedDirection = Vector3.Zero;

    private Vector3 previousInputDirection = Vector3.Zero;
    private bool inputIsOppositeX = false;
    private bool inputIsOppositeZ = false;
    

    private float _yaw = 0f;
    private float _pitch = 0f;
    private bool wishJump = false;

    public override void _Ready()
    {
        base._Ready();
        Input.MouseMode = Input.MouseModeEnum.Captured;
        camera = GetNode<Camera3D>("Camera3D");
        groundRay = GetNode<RayCast3D>("GroundRay");
        _MAX_ACCEL = 10 * _speed;
    }

    public override void _PhysicsProcess(double delta)
    {
        spaceState = GetWorld3D().DirectSpaceState;
        Vector3 endCast = new Vector3( GlobalPosition.X, GlobalPosition.Y, GlobalPosition.Z - 10.5f);
        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(GlobalPosition, endCast);
        query.Exclude = [GetRid()];
        var raycastResults = spaceState.IntersectRay(query);
        if (raycastResults.Count > 0)
        {
            GD.Print("Hit at:" + raycastResults["position"]);
        }

        if (groundRay.IsColliding())
        {
            GD.Print("Colliding!");
        }
        Vector3 direction = Vector3.Zero;
        direction = CalcDirection();
        previousInputDirection = direction;
        
        RotateCamera(_yaw, _pitch);
        
        // If you want movement relative to camera, rotate direction by camera's Y rotation here
        _cameraRotationY = camera.Rotation.Y;
        _cameraRotationX = camera.Rotation.X;

        // direction relative to camera
        direction = direction.Rotated(Vector3.Up, _cameraRotationY);
        
       // GD.Print(direction);
     
       // calc velocity from direction and speed var
        _targetVelocity = CalcVelocity(Velocity, direction, delta, inputIsOppositeX, inputIsOppositeZ); 
     
        //velocity including gravity
        Velocity = _targetVelocity;
        
        MoveAndSlide();
    }
    
    public Vector3 CalcDirection()
    {
        Vector3 direction = Vector3.Zero;
        // build direction vector
        if (Input.IsActionPressed("move_right"))
            direction.X += 1.0f;
        if (Input.IsActionPressed("move_left"))
            direction.X -= 1.0f;
        if (Input.IsActionPressed("move_back"))
            direction.Z += 1.0f;
        if (Input.IsActionPressed("move_forward"))
            direction.Z -= 1.0f;

        //normalize direction vector
        if (direction != Vector3.Zero)
            direction = direction.Normalized();
        else direction = Vector3.Zero;
        
        // did inputs flip
        if (direction.X != previousInputDirection.X)
        {
            inputIsOppositeX = true;
        }
        
        if (direction.Z != previousInputDirection.Z)
        {
            inputIsOppositeZ = true;
        }
        return direction;

    }
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

       var mouseMotion = @event as InputEventMouseMotion;
        if (mouseMotion == null) return;
      
       // GD.Print(mouseMotion.Relative);
       if (Input.IsActionPressed("input_jump"))
       {
           wishJump = true;
       }
        _yaw -= mouseMotion.Relative.X * _mouseSensitivityYaw;
        _pitch -= mouseMotion.Relative.Y * _mouseSensitivityPitch;
        _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
        
        //RotationDegrees = new Vector3(_pitch, _yaw, 0f);
    }

    private void RotateCamera(float yaw, float pitch)
    {
        camera.RotationDegrees = new Vector3(pitch, yaw, 0f);
    }

    private Vector3 UpdateGroundVel(Vector3 dir, Vector3 vel, float delta)
    {
        vel = vel * GROUND_DECEL * delta;
        float currentSpeed = vel.Dot(dir);
                
        float addSpeed = float.Clamp(_MAX_SPEED - currentSpeed, 0, _MAX_ACCEL * delta);

        return vel + (addSpeed * dir);

    }
    
    private Vector3 UpdateAirVel(Vector3 dir, Vector3 vel, float delta)
    {
        float currentSpeed = vel.Dot(dir);
                
        float addSpeed = float.Clamp(_MAX_AIR_SPEED - currentSpeed, 0, _MAX_ACCEL * delta);

        return vel + (addSpeed * dir);

    }



    private Vector3 CalcVelocity(Vector3 vel, Vector3 dir, double deltaTime, bool inputOppositeXdir, bool inputOppositeZdir)
    {
        float singleDelta = (float)deltaTime;

        if (GROUND_DECEL > 0)
        {
            if (IsOnFloor())
            {
                return UpdateGroundVel(dir, vel, singleDelta);
                
            }
            else
            {
                return UpdateAirVel(dir, vel, singleDelta);

            }
        }

        return Vector3.Zero;






    }
}
