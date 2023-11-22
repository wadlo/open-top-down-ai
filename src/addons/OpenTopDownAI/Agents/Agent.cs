using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTopDownAI;

public abstract partial class Agent : Node
{
    protected List<Vector2> directionsToTravel;
    List<float> weights;
    List<SteeringBehavior> steeringBehaviors;
    Timer timer;

    [Export]
    public float timeBetweenRecalculation = 0.05f;

    protected Unit<Vector2> unit;

    public override void _Ready()
    {
        steeringBehaviors = Utils.GetChildrenOfInterfaceType<Node, SteeringBehavior>(this);
        unit = Utils.GetSiblingOfType<Unit<Vector2>>(this);

        timer = Utils.RepeatFunctionOnTimer(
            this,
            timeBetweenRecalculation,
            () =>
            {
                RecalculateMoveDirection();
            }
        );
    }

    protected void RecalculateMoveDirection()
    {
        for (int i = 0; i < weights.Count; i++)
        {
            weights[i] = 0.0f;
        }

        foreach (SteeringBehavior behavior in steeringBehaviors)
        {
            List<float> currentBehaviorWeights = behavior.CalculateWeights(directionsToTravel);
            for (int i = 0; i < weights.Count; i++)
            {
                weights[i] += currentBehaviorWeights[i];
            }
        }

        Vector2 optimalDirection = GetOptimalVector(directionsToTravel, weights);
        unit.SetMoveDirection(optimalDirection);
    }

    protected void InitializeEmptyWeights()
    {
        weights = new List<float>();
        foreach (Vector2I vector in directionsToTravel)
        {
            weights.Add(0);
        }
    }

    protected List<Vector2> GetLocalMapCoordinatesInCircle(
        float radius,
        int numberOfPoints,
        bool roundResults = false
    )
    {
        List<Vector2> result = new List<Vector2>();
        for (int i = 0; i < numberOfPoints; i++)
        {
            float angle = i * 2.0f * Mathf.Pi / numberOfPoints;
            Vector2 coordinate = radius * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            if (roundResults)
            {
                coordinate = new Vector2(Mathf.Round(coordinate.X), Mathf.Round(coordinate.Y));
            }

            // Make sure there are no repeats
            if (result.Count == 0 || result[result.Count - 1] != coordinate)
            {
                // When it comes all the way around, it can have duplicates of the first value.
                if (result.Count != 0 && result[0] == coordinate)
                {
                    return result;
                }
                result.Add(coordinate);
            }
        }
        return result;
    }

    Vector2 GetOptimalVector(List<Vector2> potentialDirections, List<float> weights)
    {
        potentialDirections = Utils.SortListByWeights<Vector2>(potentialDirections, weights);
        for (int i = 0; i < potentialDirections.Count; i++)
        {
            Vector2 potentialDirection = potentialDirections[i];
            if (IsDirectionEmpty(potentialDirection))
            {
                return potentialDirection;
            }
        }
        return Vector2.Zero;
    }

    // Some movements may wish to restrict certain directional movements. For example if a direction is already occupied
    protected virtual bool IsDirectionEmpty(Vector2 direction)
    {
        return true;
    }
}
