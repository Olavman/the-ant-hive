using Godot;
using System;

public partial class FoodGrid : Node
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int[,] Grid;
    public void Init(int width, int height)
    {
        Width = width;
        Height = height;
        Grid = new int[Width, Height];

        for (int i = 0; i < 10; i++)
        {
            //Grid[i, 20] = 100000;
            //Grid[700, i + 300] = 100000;
            //Grid[i + 500, 400] = 100000;
            //Grid[100, 20 + i] = 100000;
        }
    }

}
