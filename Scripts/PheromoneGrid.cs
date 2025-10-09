using Godot;
using System;

public partial class PheromoneGrid : Node
{
	public int Width { get; private set; } = 256;
	public int Height { get; private set; } = 256;
	private PheromoneCell[,] _grid;// = new PheromoneCell[10, 10];
	[Signal] public delegate void PheromonesUpdatedEventHandler();

	public override void _Ready()
	{
		_grid = new PheromoneCell[Width, Height];
	}


	public float GetColonyPheromoneLevel(int x, int y)
	{
		return _grid[x, y].colony;
	}

	public float GetAlarmPheromoneLevel(int x, int y)
	{
		return _grid[x, y].alarm;
	}

	public float GetSearchPheromoneLevel(int x, int y)
	{
		return _grid[x, y].search;
	}

	public float GetReturningPheromoneLevel(int x, int y)
	{
		return _grid[x, y].returning;
	}

	public void AddColonyPheromone(int x, int y, float value)
	{

		if (x < 0 || x >= Width || y < 0 || y >= Height)
		{
			GD.PrintErr("Out of bounds");
			return;
		}
		value = Mathf.Clamp(value, 0, 1);
		_grid[x, y].colony += value;

		EmitSignal(nameof(PheromonesUpdated));
	}

	public void AddSearchPheromone(int x, int y, float value)
	{

		if (x < 0 || x >= Width || y < 0 || y >= Height)
		{
			GD.PrintErr("Out of bounds");
			return;
		}
		value = Mathf.Clamp(value, 0, 1);
		_grid[x, y].search += value;

		EmitSignal(nameof(PheromonesUpdated));
	}

	public void AddReturningPheromone(int x, int y, float value)
	{

		if (x < 0 || x >= Width || y < 0 || y >= Height)
		{
			GD.PrintErr("Out of bounds");
			return;
		}
		value = Mathf.Clamp(value, 0, 1);
		_grid[x, y].returning += value;

		EmitSignal(nameof(PheromonesUpdated));
	}

	public void AddAlarmPheromone(int x, int y, float value)
	{

		if (x < 0 || x >= Width || y < 0 || y >= Height)
		{
			GD.PrintErr("Out of bounds");
			return;
		}
		value = Mathf.Clamp(value, 0, 1);
		_grid[x, y].alarm += value;

		EmitSignal(nameof(PheromonesUpdated));
	}

	public void DiffuseRow(int row)
	{
		if (row < 0 || row >= Width)
		{
			GD.PrintErr("Out of bounds");
			return;
		}


	}

	public void DiffuseGrid()
	{
		// We need to diffuse every other row and every other column to not get a chain reaction
		int width = Width ;
		int height = Height ;
		PheromoneCell[,] gridClone = _grid.Clone() as PheromoneCell[,];
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j ++)
			{
				DiffuseCell(j, i, ref gridClone);
				//GD.Print("x: " + j + "    y: " + i);
			}
		}
		_grid = gridClone;
		EmitSignal(nameof(PheromonesUpdated));
	}

	private void DiffuseCell(int x, int y, ref PheromoneCell[,] grid)
	{
		// Higher number = spread faster
	 	const float colonyDiffusionRate = 0.2f;
	 	const float searchDiffusionRate = 0.1f;
	 	const float returningDiffusionRate = 0.1f;
	 	const float alarmDiffusionRate = 0.8f;

		// Higher number = decay faster
	 	const float colonyDecayRate = 0.001f;
	 	const float searchDecayRate = 0.001f;
	 	const float returningDecayRate = 0.001f;
	 	const float alarmDecayRate = 0.003f;

		PheromoneCell center = _grid[x, y];
		PheromoneCell up = _grid[x, Math.Max(0, y - 1)];
		PheromoneCell down = _grid[x, Math.Min(Height - 1, y + 1)];
		PheromoneCell left = _grid[Math.Max(0, x - 1), y];
		PheromoneCell right = _grid[Math.Min(Width - 1, x + 1), y];

		// Diffuse colony
		float average = (up.colony + down.colony + left.colony + right.colony)  * 0.25f; // Average with neighbours
		float diffused = Mathf.Lerp(center.colony, average, colonyDiffusionRate);
		grid[x, y].colony  = Mathf.Clamp(diffused * (1 - colonyDecayRate), 0, 1); // Sets the new pheromone level between 0-1

		// Diffuse search
		average = (up.search + down.search + left.search + right.search)  * 0.25f; // Average with neighbours
		diffused = Mathf.Lerp(center.search, average, searchDiffusionRate);
		grid[x, y].search  = Mathf.Clamp(diffused * (1 - searchDecayRate), 0, 1); // Sets the new pheromone level between 0-1

		// Diffuse returning
		average = (up.returning + down.returning + left.returning + right.returning)  * 0.25f; // Average with neighbours
		diffused = Mathf.Lerp(center.returning, average, returningDiffusionRate);
		grid[x, y].returning  = Mathf.Clamp(diffused * (1 - returningDecayRate), 0, 1); // Sets the new pheromone level between 0-1

		// Diffuse alarm
		average = (up.alarm + down.alarm + left.alarm + right.alarm)  * 0.25f; // Average with neighbours
		diffused = Mathf.Lerp(center.alarm, average, alarmDiffusionRate);
		grid[x, y].alarm  = Mathf.Clamp(diffused * (1 - alarmDecayRate), 0, 1); // Sets the new pheromone level between 0-1

	}
}
