using Godot;
using System;
using System.Data;

public partial class Worker : Node2D
{
    
    private ANT_STATE _state = ANT_STATE.SEARCHING;
    public bool HasFood = false;
    private PheromoneGrid _grid;
    public ANT_TYPE AntType = ANT_TYPE.WORKER;
    private Vector2 _velocity = Vector2.Right;
    private float _speed = 0.05f;

    public void Init(PheromoneGrid grid)
    {
        _grid = grid;
    }

    public void Update()
    {
        switch (_state)
        {
            case ANT_STATE.SEARCHING:
                DetectPheromone(PHEROMONE_TYPE.RETURNING);
                break;
            case ANT_STATE.RETURNING:
                break;
            case ANT_STATE.FLEEING:
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
        //Update();
    }


    public void DetectPheromone(PHEROMONE_TYPE pheromoneType)
    {
        int x = (int)GlobalPosition.X;
        int y = (int)GlobalPosition.Y;

        Vector2 up = Vector2.Up * _grid.GetPheromoneLevel(x, y - 1, pheromoneType);
        //GD.Print("Up: " + up.ToString());
        Vector2 down = Vector2.Down * _grid.GetPheromoneLevel(x, y+1,pheromoneType);
        //GD.Print("Down: " + down.ToString());
        Vector2 left = Vector2.Left * _grid.GetPheromoneLevel(x-1, y,pheromoneType);
        //GD.Print("Left: " + left.ToString());
        Vector2 right = Vector2.Right * _grid.GetPheromoneLevel(x + 1, y, pheromoneType);
        //GD.Print("Right: " + right.ToString());
        _velocity = (_velocity +up + down + left + right).Normalized();
        //GD.Print("New velocity " +_velocity.ToString());
        
        _grid.AddPheromone(x, y, 1, PHEROMONE_TYPE.SEARCHING);
    }

}
