using Godot;
using System;
using System.Data;

public partial class Worker : Node2D
{
    public enum STATE
    {
        SEARCHING,
        RETURNING,
        FLEEING,

    }
    private STATE _state = STATE.SEARCHING;
    public bool HasFood = false;
    private PheromoneGrid _grid;
    public ANT_TYPE AntType = ANT_TYPE.WORKER;
    private Vector2 _velocity = Vector2.Right;
    private float _speed = 1;

    public void Init(PheromoneGrid grid)
    {
        _grid = grid;
    }

    public void Update()
    {
        switch (_state)
        {
            case STATE.SEARCHING:
                DetectPheromone(PHEROMONE_TYPE.RETURNING);
                break;
            case STATE.RETURNING:
                break;
            case STATE.FLEEING:
                break;
        }
    }

    private void Move()
    {
        GlobalPosition += _velocity * _speed;
    }
    public override void _PhysicsProcess(double delta)
    {
        Move();
        Update();
    }


    public void DetectPheromone(PHEROMONE_TYPE pheromoneType)
    {
        int x = (int)GlobalPosition.X;
        int y = (int)GlobalPosition.Y;

        Vector2 up = Vector2.Up * _grid.GetPheromoneLevel(x, y-1,pheromoneType);
        Vector2 down = Vector2.Down * _grid.GetPheromoneLevel(x, y+1,pheromoneType);
        Vector2 left = Vector2.Left * _grid.GetPheromoneLevel(x-1, y,pheromoneType);
        Vector2 right = Vector2.Right * _grid.GetPheromoneLevel(x + 1, y, pheromoneType);
        _velocity = (_velocity +up + down + left + right).Normalized();
        GD.Print("New velocity " +_velocity.ToString());
        
        _grid.AddPheromone(x, y, 1, pheromoneType);
    }

}
