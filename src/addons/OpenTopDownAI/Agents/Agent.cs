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

    float totalWeight = 0.0f;

    [Export]
    public float timeBetweenRecalculation = 0.05f;

    protected Unit<Vector2> unit;

    public override void _Ready()
    {
        steeringBehaviors = Utils.GetChildrenOfType<SteeringBehavior>(this);
        foreach (SteeringBehavior behavior in steeringBehaviors)
        {
            totalWeight += behavior.weight;
        }

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
        // We calculate them all before resetting weights because some depend on looking at previous weights.
        List<List<float>> calculatedBehaviors = new List<List<float>>();
        foreach (SteeringBehavior behavior in steeringBehaviors)
        {
            calculatedBehaviors.Add(behavior.CalculateWeights(directionsToTravel));
        }

        // Clear current weights
        for (int i = 0; i < weights.Count; i++)
        {
            weights[i] = 0.0f;
        }

        for (int j = 0; j < calculatedBehaviors.Count; j++)
        {
            for (int i = 0; i < weights.Count; i++)
            {
                weights[i] += calculatedBehaviors[j][i] / totalWeight;
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

    Vector2 GetOptimalVector(List<Vector2> potentialDirections, List<float> weights)
    {
        var sortedDirections = new List<Vector2>(potentialDirections);
        sortedDirections = Utils.SortListByWeights<Vector2>(sortedDirections, weights);
        for (int i = 0; i < sortedDirections.Count; i++)
        {
            Vector2 potentialDirection = sortedDirections[i];
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

    public List<float> GetWeights()
    {
        return weights;
    }
}
