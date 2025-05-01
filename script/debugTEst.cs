using Godot;
using System;

public partial class debugTEst : Node3D
{
    private Vector3 v1 = new Vector3(0, 0, 0);
    private Vector3 v2 = new Vector3(0, 0, 0);
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        DebugDraw3D.DrawArrow(v1,v2, Colors.Blue, 4f, false, 30f);
    }
}
