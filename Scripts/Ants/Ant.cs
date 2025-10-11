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
        SEARCHING,
        RETURNING,
        FLEEING,

    }
public struct Ant
{
    public ANT_TYPE Type;
    public ANT_STATE State;
    public Vector2 Pos;
    public Vector2 Velocity;
    public float Speed;
    public bool HasFood;
    
}
