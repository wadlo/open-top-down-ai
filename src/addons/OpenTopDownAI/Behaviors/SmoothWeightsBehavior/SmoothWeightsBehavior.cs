using Godot;
using OpenTopDownAI;
using System;
using System.Collections.Generic;
using TargettingCalculations;

namespace OpenTopDownAI
{
    public partial class SmoothWeightsBehavior : SteeringBehavior
    {
        Agent agent;

        public override void _Ready()
        {
            agent = Utils.GetAncestorOfType<Agent>(this);
        }

        public override List<float> CalculateWeights(List<Vector2> directions)
        {
            List<float> result = agent.GetWeights();
            for (int i = 0; i < result.Count; i++)
            {
                result[i] *= weight;
            }
            return result;
        }
    }
}
