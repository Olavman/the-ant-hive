using Godot;
using System;

public enum PHEROMONE_TYPE
{
    COLONY,
    SEARCHING,
    RETURNING,
    ALARM
}
public struct PheromoneCell
{
    public float colony;
    public float searching;
    public float returning;
    public float alarm;
    //public Vector4 PheromoneValues; // X=colony, Y=searching, Z=returning, W=alarm

} 
