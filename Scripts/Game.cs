using Godot;
using System;

// Mother of all objects
public partial class Game : Node
{
	[Export] PackedScene AntScene;
	[Export] PackedScene MapScene;
	public PheromoneGrid Grid = new PheromoneGrid();
	private Hive _hive = new Hive();
	private Timer _timer;
	Worker ant;
	public override void _Ready()
	{
		Grid.Init();
		_hive.Init(new Vector2(Grid.Width/2, Grid.Height/2), Grid);
		_hive.GlobalPosition = new Vector2(Grid.Width/2, Grid.Height/2);
		AddChild(_hive);

		var map = MapScene.Instantiate<PheromoneMap>();
		map.Init(Grid);
		AddChild(map);

		//ant = AntScene.Instantiate<Worker>();
		//ant.GlobalPosition = new Vector2(20, 20);
		//ant.Init(Grid);
		//AddChild(ant);

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
}
