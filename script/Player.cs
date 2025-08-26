using Godot;
using System;

public partial class Player : CharacterBody3D
{
    private Camera3D camera;
    private PhysicsDirectSpaceState3D spaceState;
    private RayCast3D groundRay;
    private Timer _coyoteTimer;
    private Timer _jumpBufferTimer;
    [Export] private Curve angleVsAccelCurve;
    [Export] private float _mouseSensitivityYaw = 0.6f;
    [Export] private float _mouseSensitivityPitch = 0.5f;
    [Export] private float _minPitch = -90f;
    [Export] private float _maxPitch = 90f;
    
    [Export] private float _fallAcceleration { get; set; } = 20500f;
    [Export] private float _jumpingFallAcceleration { get; set; } = 20500f;
    [Export] private float _maxJumpHeight { get; set; } = 8;
    [Export] private float _timeToJumpApex { get; set; } = 1f;
    [Export] private float _fallVelocity { get; set; } = -1.2f;
    [Export] private float _jumpVelocity { get; set; } = 3f;
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
    private float _verticalVelocity = 0f;
    private Vector3 _rotatedDirection = Vector3.Zero;

    private Vector3 previousInputDirection = Vector3.Zero;
    private bool inputIsOppositeX = false;
    private bool inputIsOppositeZ = false;
    

    private float _yaw = 0f;
    private float _pitch = 0f;
    private bool wishJump = false;
    private bool _coyoteTimerStarted = false;
    private float fromJumpStartMaxHeight = 0f;

    private float _currentGroundVelocity = 0f;
    private double jumpTimer = 0f;
    private float _verticalDisplacementCounter = 0f;
    private bool wasOnFloorLastFrame = false;

    public override void _Ready()
    {
        base._Ready();
        Input.MouseMode = Input.MouseModeEnum.Captured;
        camera = GetNode<Camera3D>("Camera3D");
        groundRay = GetNode<RayCast3D>("GroundRay");
        _coyoteTimer = GetNode<Timer>("CoyoteTimer");
        _coyoteTimer.Timeout += () => CoyoteTimerEnd();
        _jumpBufferTimer = GetNode<Timer>("JumpBufferTimer");
       // _jumpBufferTimer.Timeout += () => wishJump = false;
        _MAX_ACCEL = 10 * _speed;
        _fallVelocity = CalcFallVelocity(_maxJumpHeight, _timeToJumpApex);
        _jumpVelocity = CalcJumpVelocity(_maxJumpHeight, _timeToJumpApex);
    }

    public override void _PhysicsProcess(double delta)
    {
        spaceState = GetWorld3D().DirectSpaceState;
        Vector3 endCastPosition = new Vector3( GlobalPosition.X, GlobalPosition.Y, GlobalPosition.Z - 10.5f);
        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(GlobalPosition, endCastPosition);
        // filter self from query
        query.Exclude = [GetRid()];
        var raycastResults = spaceState.IntersectRay(query);
        var verticalDisplacementThisFrame = 0f;
        if (wishJump == false)
        {
            fromJumpStartMaxHeight = GetGlobalPosition().Y + _maxJumpHeight;    
        }
        
        //GD.Print("jump max height global pos: " + fromJumpStartMaxHeight);
        //GD.Print(_currentGroundVelocity + " : current ground velocity");
        
        if (raycastResults.Count > 0)
        {
            GD.Print("Hit at:" + raycastResults["position"]);
        }
        // Set default gravity
        _fallVelocity = -1.2f;
        
        if (_coyoteTimerStarted == true)
        {
            GD.Print("time left " + _coyoteTimer.GetTimeLeft());
        }
        
        // Landing from not the ground
        if (IsOnFloor() && wasOnFloorLastFrame == false)
        {
            GD.Print("Landing!");
            wasOnFloorLastFrame = true;   // so we don't retrigger 
            CoyoteTimerEnd(); // don't need this anymore go back to regular gravity
            _currentGroundVelocity = GetVelocity().Length();
            if (wishJump == true)
            {
                wishJump = false;
            }
        }

        // Maintain 0 _fallVelocity while CoyoteTimer is ticking down
        if (!IsOnFloor() && _coyoteTimerStarted == true && _coyoteTimer.GetTimeLeft() >= 0)
        {
            _fallVelocity = 0;
            wasOnFloorLastFrame = false;

        }
        // START COYOTE TIME IF RUNNING OFF EDGE, optional use _currentGroundVelocity to make it more selective
        if (!IsOnFloor() && wishJump == false) 
        {
            GD.Print("not colliding");
           // if (_coyoteTimerStarted == false && GetGlobalPosition().Y < (GetGlobalPosition().Y + (_maxJumpHeight - 0.5)))
            if (_coyoteTimerStarted == false && wasOnFloorLastFrame == true)
                
            {
                _coyoteTimer.Start();
                GD.Print("coyote timer started");
                //GD.Print(wishJump + " wish jump value");
                _coyoteTimerStarted = true;
                _fallVelocity = 0;
            }
            
            // coyote time check
        }
        
        // JUMPING ON GROUND, wishJump is true
        UpdateJumpTimer(delta);
        
        if (Input.IsActionPressed("input_jump") && IsOnFloor())
        {
            wishJump = true;
            _jumpBufferTimer.Start();
            GD.Print("Jump");
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
        _verticalDisplacementCounter += verticalDisplacementThisFrame;
    }



    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        
        // DEBUG close window input
        if (Input.IsActionJustPressed("CloseDebugWindow") && OS.IsDebugBuild() )
        {
            GetTree().Quit();
        }
        var mouseMotion = @event as InputEventMouseMotion;
        if (mouseMotion == null) return;
      
       // GD.Print(mouseMotion.Relative);
//       if (Input.IsActionJustPressed("input_jump") && groundRay.IsColliding())
     
        _yaw -= mouseMotion.Relative.X * _mouseSensitivityYaw;
        _pitch -= mouseMotion.Relative.Y * _mouseSensitivityPitch;
        _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
        
        //RotationDegrees = new Vector3(_pitch, _yaw, 0f);
    }
    private Vector3 UpdateGroundVel(Vector3 dir, Vector3 vel, float delta)
    {
        vel = vel * GROUND_DECEL * delta;
        float currentSpeed = vel.Dot(dir);
                
        float addSpeed = float.Clamp(_MAX_SPEED - currentSpeed, 0, _MAX_ACCEL * delta);
        if ( addSpeed <= 0 )
        {
            addSpeed = 0;
        }

        if ( wishJump == true )
        {
            GD.Print("global pos y :" + GetGlobalPosition().Y);
            vel += new Vector3(0f, _jumpVelocity, 0f );
        }
       
        
        _currentGroundVelocity = GetVelocity().Length();
        
        return vel + ( addSpeed * dir );

    }
    private Vector3 CalcVelocity(Vector3 vel, Vector3 dir, double deltaTime, bool inputOppositeXdir, bool inputOppositeZdir)
    {
        float singleDelta = (float)deltaTime;

        if (GROUND_DECEL > 0)
        {
            // note IsOnFloor() uses move and slide
            if ( IsOnFloor() || _fallVelocity == 0)
            {
            //    GD.Print("Colliding!");
               
                _currentGroundVelocity = GetVelocity().Length();
                return UpdateGroundVel(dir, vel, singleDelta);
                
            }
            else
            {
                vel += new Vector3(0f, _fallVelocity, 0f );
                GD.Print("update air velocity " + "fall vel : " + _fallVelocity);
                return UpdateAirVel(dir, vel, singleDelta);

            }
        }

        return Vector3.Zero;
    }
    private void CoyoteTimerEnd()
    {
        _fallVelocity = -1.2f;
        _coyoteTimerStarted = false;
    //    GD.Print("coyote timer ended");
    }
    private Vector3 UpdateAirVel(Vector3 dir, Vector3 vel, float delta)
    {
        wasOnFloorLastFrame = false;
        float currentSpeed = vel.Dot(dir);
                
        float addSpeed = float.Clamp(_MAX_AIR_SPEED - currentSpeed, 0, _MAX_ACCEL * delta);
        _currentGroundVelocity = 0f;
        
        // if ( wishJump == true && ( GetGlobalPosition().Y < (fromJumpStartMaxHeight - 1f) ) )
        // {
        //
        //     GD.Print("global pos y :" + GetGlobalPosition().Y);
        //     vel += new Vector3(0f, _jumpVelocity/delta, 0f );
        // }
        // else if ( wishJump == true && ( GetGlobalPosition().Y >= (fromJumpStartMaxHeight -1f) ) ) 
        // {
        //     wishJump = false;
        //     vel += new Vector3(0f, 0f, 0f );
        // }
        
        return vel + (addSpeed * dir);

    }
    private void RotateCamera(float yaw, float pitch)
    {
        camera.RotationDegrees = new Vector3(pitch, yaw, 0f);
    }
    
    private float CalcJumpVelocity(float maxJumpHeight, float timeToJumpApex)
    {
        return 2f * maxJumpHeight / timeToJumpApex;
    }

    private float CalcFallVelocity(float jumpMaxHeight, float timeToJumpApex)
    {
        return (-2f * jumpMaxHeight) / timeToJumpApex;
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
    private void UpdateJumpTimer(double delta)
    {
        if (Input.IsActionPressed("input_jump"))
        {
            jumpTimer += delta;
        }
        else
        {
            // GD.Print("jump timer: "+ jumpTimer);
            jumpTimer = 0f;
        }
    }
}
