using Godot;
using System;

public partial class FoodGrid : Node
{
    public int Width { get; set; }
    public int Height { get; set; }
    private int[,] _grid;
    [Signal]public delegate void FoodGridChangedEventHandler();
    public void Init(int width, int height)
    {
        Width = width;
        Height = height;
        _grid = new int[Width, Height];

        for (int i = 0; i < 10; i++)
        {
            //Grid[i, 20] = 100000;
            //Grid[700, i + 300] = 100000;
            //Grid[i + 500, 400] = 100000;
            //Grid[100, 20 + i] = 100000;
        }
    }

    public int GetFood(int x, int y)
    {
        return _grid[x, y];
    }

    public void SubtractFood(int x, int y, int amount)
    {
        _grid[x, y] = Math.Max(_grid[x, y] - amount, 0);
        EmitSignal(nameof(FoodGridChanged));
    }

    public void AddFood(int x, int y, int amount)
    {
        _grid[x, y] += amount;
        EmitSignal(nameof(FoodGridChanged));
    }

    public void AddFoodCluster(int startX, int startY, int stopX, int stopY, int amount)
    {
        for (int i = startX; i < stopX; i++)
        {
            for (int j = startY; j < stopY; j++)
            {
                _grid[i, j] += amount;
            }
        }
        EmitSignal(nameof(FoodGridChanged));
    }

}
