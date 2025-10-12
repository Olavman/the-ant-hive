using Godot;
using System;

// Mother of all objects
public partial class Game : Node
{
	[Export] PackedScene AntScene;
	[Export] PackedScene MapScene;
	[Export] PackedScene AntRendererScene;
	private PheromoneGrid _pheromoneGrid = new PheromoneGrid();
	private FoodGrid _foodGrid = new FoodGrid();
	private Hive _hive = new Hive();
	private Timer _timer;
	public override void _Ready()
	{
		// Grids
		int gridWidth = 740;
		int gridHeight = 480;
		_pheromoneGrid.Init(gridWidth, gridHeight);
		_foodGrid.Init(gridWidth, gridHeight);

		// Hive
		Vector2 hivePos = new Vector2(_pheromoneGrid.Width / 2, _pheromoneGrid.Height / 2);
		_hive.Init(hivePos, _pheromoneGrid, _foodGrid);
		_hive.GlobalPosition = new Vector2(_pheromoneGrid.Width/2, _pheromoneGrid.Height/2);
		AddChild(_hive);

		// Ant renderer
		var antRenderer = AntRendererScene.Instantiate<AntRenderer>();
		antRenderer.Init(_hive);
		AddChild(antRenderer);

		// Pheromone map
		var map = MapScene.Instantiate<PheromoneMap>();
		map.Init(_pheromoneGrid);
		AddChild(map);

		// Create and configure timer
		_timer = new Timer();
		_timer.WaitTime = 0.05f; // 0.5 seconds
		_timer.Timeout += OnTimerTimout;
		AddChild(_timer);
		_timer.Start();
	}

	private void OnTimerTimout()
	{
		//ant.Update();
	}

	public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("add_colony_pheromone"))
        {
            // Grid.AddPheromone(Math.Abs((int)GD.Randi() % 740), Math.Abs((int)GD.Randi() % 480), 1, PHEROMONE_TYPE.ALARM);
            for (int i = -5; i < 5; i++)
            {
                for (int j = -5; j < 10; j++)
                {
                    _foodGrid.Grid[(int)GetViewport().GetMousePosition()[0]+i, (int)GetViewport().GetMousePosition()[1]+j] = 10;
                }
            }
        }
    }

}
