using Godot;
using System;

// Pheromone data and logic
public partial class PheromoneGrid : Node
{
	public int Width { get; private set; } = 720;
	public int Height { get; private set; } = 480;
	private PheromoneCell[,] _grid;
	[Signal] public delegate void PheromonesUpdatedEventHandler();

	// Higher number = spread faster
	const float colonyDiffusionRate = 0.2f;
	const float searchDiffusionRate = 0.1f;
	const float returningDiffusionRate = 0.15f;
	const float alarmDiffusionRate = 0.5f;

	// Higher number = decay faster
	const float colonyDecayRate = 0.001f;
	const float searchDecayRate = 0.001f;
	const float returningDecayRate = 0.0005f;
	const float alarmDecayRate = 0.00001f;

	public void Init()
	{
		_grid = new PheromoneCell[Width, Height];
	}

	public float GetPheromoneLevel(int x, int y, PHEROMONE_TYPE type)
	{
		if (x < 0 || x >= Width || y < 0 || y >= Height) return 0;
		switch (type)
		{
			case PHEROMONE_TYPE.COLONY:
				return _grid[x, y].colony;
			case PHEROMONE_TYPE.SEARCHING:
				return _grid[x, y].searching;
			case PHEROMONE_TYPE.RETURNING:
				return _grid[x, y].returning;
			case PHEROMONE_TYPE.ALARM:
				return _grid[x, y].alarm;
			default: return _grid[x, y].colony;
		}
	}

	public void AddPheromone(int x, int y, float value, PHEROMONE_TYPE type)
	{

		if (x < 0 || x >= Width || y < 0 || y >= Height)
		{
			//GD.PrintErr("Out of bounds");
			return;
		}
		value = Mathf.Clamp(value, 0, 1);
		switch (type)
		{
			case PHEROMONE_TYPE.COLONY:
				_grid[x, y].colony += value;
				break;
			case PHEROMONE_TYPE.SEARCHING:
				_grid[x, y].searching += value;
				break;
			case PHEROMONE_TYPE.RETURNING:
				_grid[x, y].returning += value;
				break;
			case PHEROMONE_TYPE.ALARM:
				_grid[x, y].alarm += value;
				break;
		}
	}

	public void DiffuseGrid()
	{
		// We need to diffuse every other row and every other column to not get a chain reaction
		int width = Width;
		int height = Height;
		PheromoneCell[,] gridClone = _grid.Clone() as PheromoneCell[,];
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				DiffuseCell(j, i, ref gridClone);
			}
		}
		_grid = gridClone;
		EmitSignal(nameof(PheromonesUpdated));
	}

	int ii = 0;
	int jj = 0;
	public void DiffuseGridSlow(int loops)
	{
		// We need to diffuse every other row and every other column to not get a chain reaction
		int width = Width;
		int height = Height;
		PheromoneCell[,] gridClone = _grid.Clone() as PheromoneCell[,];
		for (int i = 0; i < loops; i++)
		{
			DiffuseCell(ii, jj, ref gridClone);

			ii++;
			if (ii >= width)
			{
				ii = 0;
				jj++;
				if (jj >= height)
				{
					jj = 0;
					EmitSignal(nameof(PheromonesUpdated));
				}
			}
		}
		_grid = gridClone;
	}

	private void DiffuseCell(int x, int y, ref PheromoneCell[,] grid)
	{

		PheromoneCell center = _grid[x, y];
		PheromoneCell up = _grid[x, Math.Max(0, y - 1)];
		PheromoneCell down = _grid[x, Math.Min(Height - 1, y + 1)];
		PheromoneCell left = _grid[Math.Max(0, x - 1), y];
		PheromoneCell right = _grid[Math.Min(Width - 1, x + 1), y];

		// Diffuse colony
		float average = (up.colony + down.colony + left.colony + right.colony) * 0.25f; // Average with neighbours
		float diffused = Mathf.Lerp(center.colony, average, colonyDiffusionRate);
		grid[x, y].colony = Mathf.Clamp(diffused * (1 - colonyDecayRate) - 0.001f, 0, 1); // Sets the new pheromone level between 0-1

		// Diffuse search
		average = (up.searching + down.searching + left.searching + right.searching) * 0.25f; // Average with neighbours
		diffused = Mathf.Lerp(center.searching, average, searchDiffusionRate);
		grid[x, y].searching = Mathf.Clamp(diffused * (1 - searchDecayRate) - 0.001f, 0, 1); // Sets the new pheromone level between 0-1

		// Diffuse returning
		average = (up.returning + down.returning + left.returning + right.returning) * 0.25f; // Average with neighbours
		diffused = Mathf.Lerp(center.returning, average, returningDiffusionRate);
		grid[x, y].returning = Mathf.Clamp(diffused * (1 - returningDecayRate) - 0.001f, 0, 1); // Sets the new pheromone level between 0-1

		// Diffuse alarm
		average = (up.alarm + down.alarm + left.alarm + right.alarm) * 0.25f; // Average with neighbours
		diffused = Mathf.Lerp(center.alarm, average, alarmDiffusionRate);
		grid[x, y].alarm = Mathf.Clamp(diffused * (1 - alarmDecayRate) - 0.001f, 0, 1); // Sets the new pheromone level between 0-1

	}
}
