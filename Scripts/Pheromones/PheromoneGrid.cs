using Godot;
using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

// Pheromone data and logic
public partial class PheromoneGrid : Node
{
	public int Width { get; private set; }
	public int Height { get; private set; }
	private PheromoneCell[,] _grid;
	private List<Vector2> _activeCells = new List<Vector2>();
	private HashSet<Vector2> _activeSet = new HashSet<Vector2>(); // Avoid duplicates
	private float _activeThreshold = 0.001f;
	private Vector2 _hivePos;
	[Signal] public delegate void PheromonesUpdatedEventHandler();

	// Higher number = spread faster
	const float colonyDiffusionRate = 1.0f;
	const float searchDiffusionRate = 0.01f;
	const float returningDiffusionRate = 0.01f;
	const float alarmDiffusionRate = 0.5f;

	// Higher number = decay faster
	const float colonyDecayRate = 0.000001f;
	const float searchDecayRate = 0.005f;
	const float returningDecayRate = 0.025f;
	const float alarmDecayRate = 0.0001f;

	//public static readonly Vector4 DiffusionRates = new Vector4(0.2f, 0.1f, 0.15f, 0.5f); // Colony, Searching, Returning, Alarm
	//public static readonly Vector4 DecayRates = new Vector4(0.001f, 0.001f, 0.0005f, 0.00001f); // Colony, Searching, Returning, Alarm
	//public static readonly Vector4 DecayOffset = new Vector4(0.001f, 0.001f, 0.001f, 0.001f); // Colony, Searching, Returning, Alarm

	public void Init(int width, int height)
	{
		Width = width;
		Height = height;
		_grid = new PheromoneCell[Width, Height];
	}

	public Vector2 GetHivePos()
	{
		return _hivePos;
	}
	
	public void SetHivePos(Vector2 pos)
    {
		_hivePos = pos;
    }

	public float GetPheromoneLevel(int x, int y, PHEROMONE_TYPE type)
	{
		if (x < 0 || x >= Width || y < 0 || y >= Height) return 0;
		switch (type)
		{
			case PHEROMONE_TYPE.COLONY:
				return _grid[x, y].colony;
			//return _grid[x, y].PheromoneValues.X;
			case PHEROMONE_TYPE.SEARCHING:
				return _grid[x, y].searching;
			//return _grid[x, y].PheromoneValues.Y;
			case PHEROMONE_TYPE.RETURNING:
				return _grid[x, y].returning;
			//return _grid[x, y].PheromoneValues.Z;
			case PHEROMONE_TYPE.ALARM:
				return _grid[x, y].alarm;
			//return _grid[x, y].PheromoneValues.W;
			default: return _grid[x, y].colony;
				//default: return _grid[x, y].PheromoneValues.X;
		}
	}
	
	public float GetTotalPheromoneLevel(int x, int y)
	{
		if (x < 0 || x >= Width || y < 0 || y >= Height) return 0;

		float value = 0;
		value += _grid[x, y].colony;
		value += _grid[x, y].searching;
		value += _grid[x, y].returning;
		value += _grid[x, y].alarm;

		return value;
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
				//_grid[x, y].PheromoneValues.X += value;
				break;
			case PHEROMONE_TYPE.SEARCHING:
				_grid[x, y].searching += value;
				//_grid[x, y].PheromoneValues.Y += value;
				break;
			case PHEROMONE_TYPE.RETURNING:
				_grid[x, y].returning += value;
				//_grid[x, y].PheromoneValues.Z += value;
				break;
			case PHEROMONE_TYPE.ALARM:
				_grid[x, y].alarm += value;
				//_grid[x, y].PheromoneValues.W += value;
				break;
		}

		// Mark cell and it's neighbours as active
		int xx = x;
		int yy = y;
		if (GetTotalPheromoneLevel(xx, yy) > _activeThreshold && _activeSet.Add(new Vector2(xx, yy)))
		{
			_activeCells.Add(new Vector2(xx, yy));
		}
		if (GetTotalPheromoneLevel(xx - 1, yy) > _activeThreshold && _activeSet.Add(new Vector2(xx - 1, yy)))
		{
			_activeCells.Add(new Vector2(xx - 1, yy));
		}
		if (GetTotalPheromoneLevel(xx + 1, yy) > _activeThreshold && _activeSet.Add(new Vector2(xx + 1, yy)))
		{
			_activeCells.Add(new Vector2(xx + 1, yy));
		}
		if (GetTotalPheromoneLevel(xx, yy - 1) > _activeThreshold && _activeSet.Add(new Vector2(xx, yy - 1)))
		{
			_activeCells.Add(new Vector2(xx, yy - 1));
		}
		if (GetTotalPheromoneLevel(xx, yy + 1) > _activeThreshold && _activeSet.Add(new Vector2(xx, yy + 1)))
		{
			_activeCells.Add(new Vector2(xx, yy + 1));
		}
	}

	public void DiffuseActiveCells()
	{
		var newActive = new HashSet<Vector2>();

		foreach (Vector2 pos in _activeCells)
		{
			int x = (int)pos.X;
			int y = (int)pos.Y;

			// Diffuse and decay this cell
			DiffuseCell(x, y, ref _grid);

			// If its still active, keep it
			if (GetTotalPheromoneLevel(x, y) > _activeThreshold)
			{
				newActive.Add(pos);
			}

			// Check neighbours
			foreach (Vector2 neighbour in GetNeighbours(x, y))
			{
				if (GetTotalPheromoneLevel((int)neighbour.X, (int)neighbour.Y) > _activeThreshold)
				{
					newActive.Add(neighbour);
				}
			}
		}
		
	}
	
	IEnumerable<Vector2> GetNeighbours(int x, int y)
    {
		yield return new Vector2(x, Math.Max(0, y - 1));
		yield return new Vector2(x, Math.Min(Height - 1, y + 1));
		yield return new Vector2(Math.Max(0, x - 1), y);
		yield return new Vector2(Math.Min(Width - 1, x + 1), y);
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
		float result = Mathf.Clamp(diffused * (1 - colonyDecayRate), 0, 1);
		if (result < 0.0001f) result = 0;
		grid[x, y].colony = result; // Sets the new pheromone level between 0-1
		//grid[x, y].colony = diffused * (1 - colonyDecayRate) - 0.001f; // Sets the new pheromone level between 0-1

		// Diffuse search
		average = (up.searching + down.searching + left.searching + right.searching) * 0.25f; // Average with neighbours
		diffused = Mathf.Lerp(center.searching, average, searchDiffusionRate);
		result = Mathf.Clamp(diffused * (1 - searchDecayRate), 0, 1);
		if (result < 0.0001f) result = 0;
		grid[x, y].searching = result; // Sets the new pheromone level between 0-1
		//grid[x, y].searching = diffused * (1 - searchDecayRate) - 0.001f; // Sets the new pheromone level between 0-1

		// Diffuse returning
		average = (up.returning + down.returning + left.returning + right.returning) * 0.25f; // Average with neighbours
		diffused = Mathf.Lerp(center.returning, average, returningDiffusionRate);
		result = Mathf.Clamp(diffused * (1 - returningDecayRate), 0, 1);
		if (result < 0.0001f) result = 0;
		grid[x, y].returning = result; // Sets the new pheromone level between 0-1
		//grid[x, y].returning = diffused * (1 - returningDecayRate) - 0.00001f; // Sets the new pheromone level between 0-1

		// Diffuse alarm
		average = (up.alarm + down.alarm + left.alarm + right.alarm) * 0.25f; // Average with neighbours
		diffused = Mathf.Lerp(center.alarm, average, alarmDiffusionRate);
		result = Mathf.Clamp(diffused * (1 - alarmDecayRate), 0, 1);
		if (result < 0.0001f) result = 0;
		grid[x, y].alarm = result; // Sets the new pheromone level between 0-1
		//grid[x, y].alarm = diffused * (1 - alarmDecayRate) - 0.00001f; // Sets the new pheromone level between 0-1
		
		
		
/*
		Vector4 center = 	grid[x, 							y].PheromoneValues;
		Vector4 up = 		grid[x, 							Math.Max(0, y - 1)].PheromoneValues;
		Vector4 down = 		grid[x, 							Math.Min(Height - 1, y + 1)].PheromoneValues;
		Vector4 left = 		grid[Math.Max(0, x - 1), 			y].PheromoneValues;
		Vector4 right = 	grid[Math.Min(Width - 1, x + 1), 	y].PheromoneValues;

		Vector4 average = (up + down + left + right) * 0.25f;
		Vector4 diffused = center +(average - center) * DiffusionRates;
		Vector4 result = diffused * (Vector4.One - DecayRates) - DecayOffset;

		grid[x, y].PheromoneValues = result.Clamp(Vector4.Zero, Vector4.One);*/
	}
}
