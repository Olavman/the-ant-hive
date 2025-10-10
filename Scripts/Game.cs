using Godot;
using System;

public partial class Game : Node
{
	[Export] PackedScene AntScene;
	[Export]  PackedScene MapScene;
	public PheromoneGrid Grid = new PheromoneGrid();
	public override void _Ready()
	{
		Grid._Ready();
		
		var map = MapScene.Instantiate<PheromoneMap>();
		map.Init(Grid);
		AddChild(map);
		
		var ant = AntScene.Instantiate<Worker>();
		ant.GlobalPosition = new Vector2(20, 20);
		ant.Init(Grid);
		AddChild(ant);
	}

}
