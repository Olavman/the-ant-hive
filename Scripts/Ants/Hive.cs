using Godot;
using System;
using System.Data;

// Spawns and controls ants
public partial class Hive : Node2D
{
    private PheromoneGrid _grid;
    private Ant[] _ants;
    private float _updatePercentage = 0.5f; // 1 = 100%
    private int _updateIndex = 0;
    public void Init(Vector2 pos, PheromoneGrid grid)
    {
        GlobalPosition = pos;
        _grid = grid;

        _ants = new Ant[10000];
        for (int i = 0; i < 10000; i++)
        {
            Ant ant = new Ant();
            ant.Type = ANT_TYPE.WORKER;
            ant.State = ANT_STATE.SEARCHING;
            ant.Velocity = new Vector2(GD.Randf()*2-1, GD.Randf()*2-1);
            ant.Pos = new Vector2(GlobalPosition.X, GlobalPosition.Y);
            ant.Speed = 10f;

            _ants[i] = ant;
        }
    }

    public override void _Ready()
    {
        base._Ready();
    }
    public override void _Process(double delta)
    {
        _grid.AddPheromone((int)GlobalPosition.X, (int)GlobalPosition.Y, 1, PHEROMONE_TYPE.COLONY);
        UpdateAnts();


        int length = _ants.Length;
        float deltaFloat = (float)delta;
        for (int i = 0; i < length; i++)
        {
            ref Ant a = ref _ants[i];
            AntLogic.Move(ref a, _grid, deltaFloat);
        }
    }


    public void UpdateAnts()
    {
        int length = _ants.Length;
        int updateAmount = (int)(length * _updatePercentage) + 1;
        for (int i = 0; i < updateAmount; i++)
        {
            ref Ant a = ref _ants[_updateIndex];
            AntLogic.Update(ref a, ref _grid);

            _updateIndex = (_updateIndex + 1) % length;
        }
    }

}
