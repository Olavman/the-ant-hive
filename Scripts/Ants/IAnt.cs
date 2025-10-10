using Godot;
using System;
public enum ANT_TYPE
{
    WORKER,
    SOLDIER,
    SCOUT,
    QUEEN
}
public interface IAnt
{
    public ANT_TYPE AntType { get; set; }
    
}
