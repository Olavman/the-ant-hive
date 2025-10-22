using Godot;
using System;
using System.Data;

// Spawns and controls ants
public partial class Hive : Node2D
{
    private PheromoneGrid _pheromoneGrid;
    private FoodGrid _foodGrid;
    private Ant[] _ants;

    private float _foodCount = 100;
    private float _updateTimer = 0f;
    private const float _updateInterval = 0.0005f;
    private float _updatePercentage = 0.1f; // 1 = 100%
    private int _updateIndex = 0;
    public void Init(Vector2 pos, PheromoneGrid pheromoneGrid, FoodGrid foodGrid)
    {
        GlobalPosition = pos;
        _pheromoneGrid = pheromoneGrid;
        _pheromoneGrid.SetHivePos(pos);
        _foodGrid = foodGrid;
        AntLogic.OnFoodReturned += OnFoodReturned;

        int antCount = 10000;
        _ants = new Ant[antCount];
    }

    private void OnFoodReturned(int amount)
    {
        _foodCount += amount;
        if (_foodCount > 10) // Spawn ant
        {
            _foodCount -= 10;
            SpawnAnt();
        }
    }

    private void OnAntReturned(ref Ant ant)
    {
        
    }
    
    private void SpawnAnt()
    {
        Ant ant = new Ant();
        ant.Type = ANT_TYPE.SCOUT;
        ant.State = ANT_STATE.SEARCHING;
        ant.Velocity = new Vector2(GD.Randf() * 2 - 1, GD.Randf() * 2 - 1);
        ant.Pos = new Vector2(GlobalPosition.X, GlobalPosition.Y);
        ant.Speed = 10 + GD.Randf() * 2;

        for (int i = 0; i < _ants.Length; i++)
        {
            if (_ants[i].State == ANT_STATE.NONE)
            {
                _ants[i] = ant;
                return;
            }
        }
    }


    public override void _Process(double delta)
    {
        _pheromoneGrid.AddPheromone((int)GlobalPosition.X, (int)GlobalPosition.Y, 0.2f, PHEROMONE_TYPE.RETURNING); // Colony pheromone will be replaced with return pheromone later on.

        // Update ant logic on a fixed interval
        _updateTimer += (float)delta;
        if (_updateTimer > _updateInterval)
        {
            _updateTimer -= _updateInterval;
            UpdateAnts();
        }


        int length = _ants.Length;
        float deltaFloat = (float)delta;
        for (int i = 0; i < length; i++)
        {
            ref Ant a = ref _ants[i];
            AntLogic.Move(ref a, _pheromoneGrid, ref _foodGrid, deltaFloat);
        }

        if (_foodCount > 10) // Spawn ant
        {
            _foodCount -= 10;
            SpawnAnt();
        }
    }

    public int GetAntCount()
    {
        return _ants.Length;
    }

    public Ant[] GetAnts()
    {
        return _ants;
    }


    public void UpdateAnts()
    {
        int length = _ants.Length;
        int updateAmount = Math.Max(1, (int)(length * _updatePercentage));
        for (int i = 0; i < updateAmount; i++)
        {
            ref Ant a = ref _ants[_updateIndex];
            AntLogic.Update(ref a, ref _pheromoneGrid);

            _updateIndex = (_updateIndex + 1) % length;
        }
    }

}
