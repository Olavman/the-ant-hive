using Godot;
using System;

// Mother of all objects
public partial class Game : Node
{
	[Export] PackedScene AntScene;
	[Export] PackedScene PheromoneMapScene;
	[Export] PackedScene FoodMapScene;
	[Export] PackedScene AntRendererScene;
	private PheromoneGrid _pheromoneGrid = new PheromoneGrid();
	private FoodGrid _foodGrid = new FoodGrid();
	private Hive _hive = new Hive();
	private Timer _timer;

	public FoodRenderer foodRenderer;
	public override void _Ready()
	{

		// Grids
		int gridWidth = 740;
		int gridHeight = 480;
		_pheromoneGrid.Init(gridWidth, gridHeight);
		_foodGrid.Init(gridWidth, gridHeight);

		// Food Renderer
		//var foodRenderer = new FoodRenderer();
		//foodRenderer.Init(_foodGrid);
		//foodRenderer._Ready();
		//AddChild(foodRenderer);

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
		var pheromoneMap = PheromoneMapScene.Instantiate<PheromoneMap>();
		pheromoneMap.Init(_pheromoneGrid);
		AddChild(pheromoneMap);

		// Pheromone map
		var foodMap = FoodMapScene.Instantiate<FoodRenderer>();
		foodMap.Init(_foodGrid);
		AddChild(foodMap);

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
			int x = (int)GetViewport().GetMousePosition()[0];
			int y = (int)GetViewport().GetMousePosition()[1];
			_foodGrid.AddFoodCluster(x, y, x + 10, y + 10, 10);
		}
    }

}
