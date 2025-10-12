using Godot;
using System;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;

// Ants brain
public static class AntLogic
{


    public static void Update(ref Ant ant, ref PheromoneGrid pheromoneGrid)
    {

        switch (ant.State)
        {
            case ANT_STATE.IDLE:
                Searching(ref ant, ref pheromoneGrid);
                break;
            case ANT_STATE.SEARCHING:
                Searching(ref ant, ref pheromoneGrid);
                break;
            case ANT_STATE.RETURNING:
                Returning(ref ant, ref pheromoneGrid);
                break;
            case ANT_STATE.FLEEING:
                break;
            case ANT_STATE.FOUND_HOME:
                ReturnHome(ref ant, ref pheromoneGrid);
                break;
        }
    }

    private static void Idle(ref Ant ant, ref PheromoneGrid pheromoneGrid)
    {
        if (pheromoneGrid.GetPheromoneLevel((int)ant.Pos.X, (int)ant.Pos.Y, PHEROMONE_TYPE.RETURNING) > 0)
        {
            //GD.Print("Returning home");
            ant.State = ANT_STATE.SEARCHING;
            return;
        }
        ant.Velocity = (ant.Velocity + DetectPheromone2(ref pheromoneGrid, PHEROMONE_TYPE.COLONY, ref ant)).Normalized();
        EmitPheromone(PHEROMONE_TYPE.RETURNING, ref pheromoneGrid, ant);
    }
    private static void ReturnHome(ref Ant ant, ref PheromoneGrid pheromoneGrid)
    {
        if (pheromoneGrid.GetPheromoneLevel((int)ant.Pos.X, (int)ant.Pos.Y, PHEROMONE_TYPE.COLONY) > 0.8f)
        {
            //GD.Print("Returned home. Start searching");
            ant.Velocity *= -1;
            ant.State = ANT_STATE.SEARCHING;
            return;
        }
        ant.Velocity = (ant.Velocity + DetectPheromone2(ref pheromoneGrid, PHEROMONE_TYPE.COLONY, ref ant)).Normalized();
        EmitPheromone(PHEROMONE_TYPE.RETURNING, ref pheromoneGrid, ant);
    }


    private static void Returning(ref Ant ant, ref PheromoneGrid pheromoneGrid)
    {
        if (pheromoneGrid.GetPheromoneLevel((int)ant.Pos.X, (int)ant.Pos.Y, PHEROMONE_TYPE.COLONY) > 0)
        {
            //GD.Print("Returning home");
            ant.State = ANT_STATE.FOUND_HOME;
            return;
        }
        ant.Velocity = (ant.Velocity + DetectPheromone2(ref pheromoneGrid, PHEROMONE_TYPE.SEARCHING, ref ant)).Normalized();
        EmitPheromone(PHEROMONE_TYPE.RETURNING, ref pheromoneGrid, ant);

    }

    public static void FoundFood(ref Ant ant)
    {
        //GD.Print("Found food");
        ant.State = ANT_STATE.RETURNING;
        ant.Velocity *= -1;
    }


    public static void Searching(ref Ant ant, ref PheromoneGrid pheromoneGrid)
    {
        ant.Velocity = (ant.Velocity + DetectPheromone2(ref pheromoneGrid, PHEROMONE_TYPE.RETURNING, ref ant)).Normalized();
        if (ant.Type == ANT_TYPE.SCOUT) EmitPheromone(PHEROMONE_TYPE.SEARCHING, ref pheromoneGrid, ant);
    }

    public static void Move(ref Ant ant, PheromoneGrid pheromoneGrid, ref FoodGrid foodGrid, float delta)
    {
        if (foodGrid.Grid[(int)ant.Pos.X, (int)ant.Pos.Y] > 0 && ant.State == ANT_STATE.SEARCHING)
        {
            foodGrid.Grid[(int)ant.Pos.X, (int)ant.Pos.Y] -= 1;
            FoundFood(ref ant);
        }
        
        Vector2 newPos = ant.Pos + ant.Velocity * ant.Speed * delta;

        // Turn around if hitting edge of map
        if (newPos.X < 0 || newPos.X >= pheromoneGrid.Width || newPos.Y < 0 || newPos.Y >= pheromoneGrid.Height)
        {
            //ant.Velocity = new Vector2(GD.Randf()*2-1, GD.Randf()*2-1);
            ant.Velocity *= GD.Randf() * -1;
            return;
        }

        ant.Pos = newPos;
    }

    public static Vector2 DetectPheromone(ref PheromoneGrid grid, PHEROMONE_TYPE pheromoneType, ref Ant ant)
    {
        int x = (int)ant.Pos.X;
        int y = (int)ant.Pos.Y;
        //float pheromonePriority = 1; // Higher number gives sharper turns

        Vector2 up = Vector2.Up * grid.GetPheromoneLevel(x, y - 1, pheromoneType);
        Vector2 down = Vector2.Down * grid.GetPheromoneLevel(x, y + 1, pheromoneType);
        Vector2 left = Vector2.Left * grid.GetPheromoneLevel(x - 1, y, pheromoneType);
        Vector2 right = Vector2.Right * grid.GetPheromoneLevel(x + 1, y, pheromoneType);
        //ant.Velocity = (ant.Velocity + up + down + left + right).Normalized();
        return (up + down + left + right);
        //GD.Print("New velocity " +ant.Velocity.ToString());
    }

    public static Vector2 DetectPheromone2(ref PheromoneGrid grid, PHEROMONE_TYPE pheromoneType, ref Ant ant)
    {
        int x = (int)ant.Pos.X;
        int y = (int)ant.Pos.Y;

        float sensorDistance = 10.0f;
        float sensorAngle = Mathf.DegToRad(15);
        float randomSteerStrength = Mathf.DegToRad(15-GD.Randf() * 30);

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

        return ant.Velocity.Rotated(randomSteerStrength);
    }

    public static void EmitPheromone(PHEROMONE_TYPE pheromone, ref PheromoneGrid grid, Ant ant, float amount = 0)
    {
        if (amount == 0)
        {
            switch (pheromone)
            {
                case PHEROMONE_TYPE.COLONY:
                    amount = 1;
                    break;
                case PHEROMONE_TYPE.ALARM:
                    amount = 1;
                    break;
                case PHEROMONE_TYPE.SEARCHING:
                    amount = 0.0002f;
                    break;
                case PHEROMONE_TYPE.RETURNING:
                    amount = 0.001f;
                    break;
            }
        }
        grid.AddPheromone((int)ant.Pos.X, (int)ant.Pos.Y, amount, pheromone);
        //GD.Print("Emitting: " + pheromone.ToString());
    }
}
