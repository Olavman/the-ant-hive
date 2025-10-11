using Godot;
using System;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;

// Ants brain
public static class AntLogic
{


    public static void Update(ref Ant ant, ref PheromoneGrid grid)
    {

        switch (ant.State)
        {
            case ANT_STATE.SEARCHING:
                Searching(ref ant, ref grid);
                break;
            case ANT_STATE.RETURNING:
                Returning(ref ant, ref grid);
                break;
            case ANT_STATE.FLEEING:
                break;
        }
    }

    private static void Returning(ref Ant ant, ref PheromoneGrid grid)
    {
        //GD.Print("returning");
        //ant.Velocity = (ant.Velocity + DetectPheromone(ref grid, PHEROMONE_TYPE.COLONY, ref ant)).Normalized();
        ant.Velocity = (ant.Velocity + DetectPheromone2(ref grid, PHEROMONE_TYPE.SEARCHING, ref ant)).Normalized();
        EmitPheromone(PHEROMONE_TYPE.RETURNING, ref grid, ant);

    }

    public static void FoundFood(ref Ant ant)
    {
        //GD.Print("Found food");
        ant.State = ANT_STATE.RETURNING;
        ant.Velocity *= -1;
    }


    public static void Searching(ref Ant ant, ref PheromoneGrid grid)
    {
        Vector2 vel = DetectPheromone(ref grid, PHEROMONE_TYPE.ALARM, ref ant);
        if (vel != new Vector2(0, 0))
        {
            FoundFood(ref ant);
            return;
        }
        ant.Velocity = (ant.Velocity + DetectPheromone2(ref grid, PHEROMONE_TYPE.RETURNING, ref ant)).Normalized();
        EmitPheromone(PHEROMONE_TYPE.SEARCHING, ref grid, ant);
    }

    public static void Move(ref Ant ant, PheromoneGrid grid, float delta)
    {
        Vector2 newPos = ant.Pos + ant.Velocity * ant.Speed;

        // Turn around if hitting edge of map
        if (newPos.X < 0 || newPos.X >= grid.Width || newPos.Y < 0 || newPos.Y >= grid.Height)
        {
            ant.Velocity *= -1;
            return;
        }

        ant.Pos += ant.Velocity * ant.Speed * delta;
    }

    public static Vector2 DetectPheromone(ref PheromoneGrid grid, PHEROMONE_TYPE pheromoneType, ref Ant ant)
    {
        int x = (int)ant.Pos.X;
        int y = (int)ant.Pos.Y;
        float pheromonePriority = 1; // Higher number gives sharper turns

        Vector2 up = Vector2.Up * grid.GetPheromoneLevel(x, y - 1, pheromoneType) * pheromonePriority;
        Vector2 down = Vector2.Down * grid.GetPheromoneLevel(x, y + 1, pheromoneType) * pheromonePriority;
        Vector2 left = Vector2.Left * grid.GetPheromoneLevel(x - 1, y, pheromoneType) * pheromonePriority;
        Vector2 right = Vector2.Right * grid.GetPheromoneLevel(x + 1, y, pheromoneType) * pheromonePriority;
        //ant.Velocity = (ant.Velocity + up + down + left + right).Normalized();
        return (up + down + left + right);
        //GD.Print("New velocity " +ant.Velocity.ToString());
    }

    public static Vector2 DetectPheromone2(ref PheromoneGrid grid, PHEROMONE_TYPE pheromoneType, ref Ant ant)
    {
        int x = (int)ant.Pos.X;
        int y = (int)ant.Pos.Y;

        float sensorDistance = 3.0f;
        float sensorAngle = Mathf.DegToRad(30);
        float randomSteerStrength = Mathf.DegToRad(GD.Randf() * 30);

        Vector2 forwardPos = ant.Pos + new Vector2(Mathf.Cos(ant.Velocity.Angle()), Mathf.Sin(ant.Velocity.Angle())) * sensorDistance;
        Vector2 leftPos = ant.Pos + new Vector2(Mathf.Cos(ant.Velocity.Angle() - sensorAngle), Mathf.Sin(ant.Velocity.Angle() - sensorAngle)) * sensorDistance;
        Vector2 rightPos = ant.Pos + new Vector2(Mathf.Cos(ant.Velocity.Angle() + sensorAngle), Mathf.Sin(ant.Velocity.Angle() + sensorAngle)) * sensorDistance;

        float weightForward = grid.GetPheromoneLevel((int)forwardPos.X, (int)forwardPos.Y, pheromoneType);
        float weightLeft = grid.GetPheromoneLevel((int)leftPos.X, (int)leftPos.Y, pheromoneType);
        float weightRight = grid.GetPheromoneLevel((int)rightPos.X, (int)rightPos.Y, pheromoneType);

        if (weightLeft > weightRight && weightLeft > weightForward)
        {
            return ant.Velocity.Rotated(-sensorAngle * 0.5f - randomSteerStrength);
        }
        else if (weightRight > weightLeft && weightRight > weightForward)
        {
            return ant.Velocity.Rotated(sensorAngle * 0.5f + randomSteerStrength);
        }

        return ant.Velocity * (randomSteerStrength - Mathf.DegToRad(30));
    }

    public static void EmitPheromone(PHEROMONE_TYPE pheromone, ref PheromoneGrid grid, Ant ant)
    {
        float amount = 0;
        switch (pheromone)
        {
            case PHEROMONE_TYPE.COLONY:
                amount = 1;
                break;
            case PHEROMONE_TYPE.ALARM:
                amount = 1;
                break;
            case PHEROMONE_TYPE.SEARCHING:
                amount = 0.02f;
                break;
            case PHEROMONE_TYPE.RETURNING:
                amount = 0.02f;
                break;
        }
        grid.AddPheromone((int)ant.Pos.X, (int)ant.Pos.Y, amount, pheromone);
        //GD.Print("Emitting: " + pheromone.ToString());
    }
}
