using Godot;
using System;
public enum ANT_TYPE
{
    WORKER,
    SOLDIER,
    SCOUT,
    QUEEN
}
public enum ANT_STATE
{
    NONE,
    IDLE,
    SEARCHING,
    RETURNING,
    FLEEING,
    FOUND_HOME
}

public struct Ant
{
    public ANT_TYPE Type;
    public ANT_STATE State;
    public Vector2 Pos;
    public Vector2 Velocity;
    public Vector2 HomeVector;
    public float Speed;
    public bool HasFood;
    public int WanderTimer;
    
}
