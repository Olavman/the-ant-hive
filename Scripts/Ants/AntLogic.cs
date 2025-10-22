using Godot;
using System;
using System.Windows;
using System.ComponentModel;
using System.Configuration.Assemblies;
using System.IO.Pipes;
//using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;

// Ants brain
public static class AntLogic
{
	public delegate void FoodReturnedEventHandler(int amount);
	public static FoodReturnedEventHandler OnFoodReturned;

	public static void Update(ref Ant ant, ref PheromoneGrid pheromoneGrid)
	{
		ant.WanderTimer++;
		if ((ant.Pos - pheromoneGrid.GetHivePos()).Length() < 10f)
		{
			ReturnedHome(ref ant, ref pheromoneGrid);
		}
		if (ant.WanderTimer > 2000) // Dead
		{
			ant.Velocity = Vector2.Zero;
			ant.State = ANT_STATE.IDLE;
		}
		else if (ant.WanderTimer > 200) // Return home
		{
			if (ant.State != ANT_STATE.FOUND_HOME && ant.State != ANT_STATE.RETURNING)
			{
				ant.Velocity = -ant.HomeVector.Normalized();
				ant.State = ANT_STATE.RETURNING;
			}
		}
		
		switch (ant.State)
		{
			case ANT_STATE.IDLE:
				ant.Velocity = Vector2.Zero;
				break;
			case ANT_STATE.SEARCHING:
				Searching(ref ant, ref pheromoneGrid);
				break;
			case ANT_STATE.RETURNING:
				Returning(ref ant, ref pheromoneGrid);
				break;
			case ANT_STATE.FLEEING:
				break;
		}
	}

	private static void ReturnedHome(ref Ant ant, ref PheromoneGrid pheromoneGrid)
	{
		ant.WanderTimer = 0;
		ant.HomeVector = Vector2.Zero; // Point home
									   //GD.Print("Returned home. Start searching");
		if (ant.HasFood)
		{
			OnFoodReturned?.Invoke(1);
			ant.HasFood = false;
			ant.Velocity *= -1;
		}
		else
        {
            
			OnFoodReturned?.Invoke(0);
        }
		ant.State = ANT_STATE.SEARCHING;
		return;
	
	}


	private static void Returning(ref Ant ant, ref PheromoneGrid pheromoneGrid)
	{
		ant.Velocity = (ant.Velocity + DetectPheromone(ref pheromoneGrid, PHEROMONE_TYPE.SEARCHING, ref ant)).Normalized();
		if (ant.HasFood) EmitPheromone(PHEROMONE_TYPE.RETURNING, ref pheromoneGrid, ant);

	}

	public static void FoundFood(ref Ant ant)
	{
		//GD.Print("Found food");
		ant.HasFood = true;
		ant.State = ANT_STATE.RETURNING;
		ant.Velocity = -ant.HomeVector.Normalized();
	}


	public static void Searching(ref Ant ant, ref PheromoneGrid pheromoneGrid)
	{
		ant.Velocity = (ant.Velocity + DetectPheromone(ref pheromoneGrid, PHEROMONE_TYPE.RETURNING, ref ant)).Normalized();
		if (ant.Type == ANT_TYPE.SCOUT) EmitPheromone(PHEROMONE_TYPE.SEARCHING, ref pheromoneGrid, ant);
	}

	public static void Move(ref Ant ant, PheromoneGrid pheromoneGrid, ref FoodGrid foodGrid, float delta)
	{
		if (foodGrid.GetFood((int)ant.Pos.X, (int)ant.Pos.Y) > 0 && ant.State == ANT_STATE.SEARCHING)
		{
			foodGrid.SubtractFood((int)ant.Pos.X, (int)ant.Pos.Y, 1);
			FoundFood(ref ant);
		}
		
		Vector2 displacement = ant.Velocity * ant.Speed * delta;
		Vector2 newPos = ant.Pos + displacement;

		// Turn in a random direction if hitting edge of map
		if (newPos.X < 0 || newPos.X >= pheromoneGrid.Width || newPos.Y < 0 || newPos.Y >= pheromoneGrid.Height)
		{
			//ant.Velocity = new Vector2(GD.Randf()*2-1, GD.Randf()*2-1);
			ant.Velocity *= GD.Randf() * -1;
			return;
		}

		ant.Pos = newPos;

		ant.HomeVector += displacement;
	}

	public static Vector2 DetectPheromone(ref PheromoneGrid grid, PHEROMONE_TYPE pheromoneType, ref Ant ant)
	{
		float sensorDistance = 5.0f;
		float sensorAngle = Mathf.DegToRad(30);

		Vector2 leftPos = ant.Pos + new Vector2(
			Mathf.Cos(ant.Velocity.Angle() - sensorAngle),
			Mathf.Sin(ant.Velocity.Angle() - sensorAngle)
			) * sensorDistance;

		Vector2 rightPos = ant.Pos + new Vector2(
			Mathf.Cos(ant.Velocity.Angle() + sensorAngle),
			Mathf.Sin(ant.Velocity.Angle() + sensorAngle)
			) * sensorDistance;

		// Probabilistic
		float bias = 6f;
		float leftValue = Mathf.Max(0.001f, grid.GetPheromoneLevel((int)leftPos.X, (int)leftPos.Y, pheromoneType));
		float rightValue = Mathf.Max(0.001f, grid.GetPheromoneLevel((int)rightPos.X, (int)rightPos.Y, pheromoneType));

		// Exponentiate to amplify differences
		leftValue = Mathf.Pow(leftValue, bias);
		rightValue = Mathf.Pow(rightValue, bias);

		float total = leftValue + rightValue;
		float leftProbability = leftValue / total;
		
		if (GD.Randf() < leftProbability)
		{
			// GD.Print("Turn left");
			return ant.Velocity.Rotated(-sensorAngle * 0.5f);
		}
		else 
		{
			// GD.Print("Turn right");
			return ant.Velocity.Rotated(sensorAngle * 0.5f);
		}
	}

	public static Vector2 DetectPheromone2(ref PheromoneGrid grid, PHEROMONE_TYPE pheromoneType, ref Ant ant)
	{
		int x = (int)ant.Pos.X;
		int y = (int)ant.Pos.Y;

		float sensorDistance = 3.0f;
		float sensorAngle = Mathf.DegToRad(90);
		float randomSteerStrength = Mathf.DegToRad(2-GD.Randf() * 4);

		Vector2 forwardPos = ant.Pos + new Vector2(Mathf.Cos(ant.Velocity.Angle()), Mathf.Sin(ant.Velocity.Angle())) * sensorDistance;
		Vector2 leftPos = ant.Pos + new Vector2(Mathf.Cos(ant.Velocity.Angle() - sensorAngle), Mathf.Sin(ant.Velocity.Angle() - sensorAngle)) * sensorDistance;
		Vector2 rightPos = ant.Pos + new Vector2(Mathf.Cos(ant.Velocity.Angle() + sensorAngle), Mathf.Sin(ant.Velocity.Angle() + sensorAngle)) * sensorDistance;

		float weightForward = grid.GetPheromoneLevel((int)forwardPos.X, (int)forwardPos.Y, pheromoneType);
		float weightLeft = grid.GetPheromoneLevel((int)leftPos.X, (int)leftPos.Y, pheromoneType);
		float weightRight = grid.GetPheromoneLevel((int)rightPos.X, (int)rightPos.Y, pheromoneType);
		float value = (-sensorAngle * weightLeft + sensorAngle * weightRight + randomSteerStrength)*2;
		//GD.Print("Rotat: " +value);

		return ant.Velocity.Rotated(value);

	   /*  if (weightLeft > weightRight && weightLeft > weightForward)
		{
			return ant.Velocity.Rotated(-sensorAngle * 0.5f - randomSteerStrength);
		}
		else if (weightRight > weightLeft && weightRight > weightForward)
		{
			return ant.Velocity.Rotated(sensorAngle * 0.5f + randomSteerStrength);
		} */

		//return ant.Velocity.Rotated(randomSteerStrength);
	}

	public static void EmitPheromone(PHEROMONE_TYPE pheromone, ref PheromoneGrid grid, Ant ant, float amount = 0)
	{
		if (amount == 0)
		{
			switch (pheromone)
			{
				case PHEROMONE_TYPE.COLONY:
					amount = 0.2f;
					break;
				case PHEROMONE_TYPE.ALARM:
					amount = 1;
					break;
				case PHEROMONE_TYPE.SEARCHING:
					amount = 0.15f;
					break;
				case PHEROMONE_TYPE.RETURNING:
					amount = 0.15f;
					break;
			}
		}
		grid.AddPheromone((int)ant.Pos.X, (int)ant.Pos.Y, amount, pheromone);
		//GD.Print("Emitting: " + pheromone.ToString());
	}
}
