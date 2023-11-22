using Godot;
using OpenTopDownAI;
using System;
using System.Collections.Generic;
using TargettingCalculations;

namespace OpenTopDownAI
{
    public partial class MoveTowardsBehavior : Node, SteeringBehavior
    {
        Node2D rootObjectNode;

        [Export]
        Target target;

        [Export]
        public float minDistance = 0.0f;

        [Export]
        public float maxDistance = 10.0f;

        public override void _Ready()
        {
            rootObjectNode = Utils.GetAncestorSiblingOfType<Node2D>(this);
        }

        public List<float> CalculateWeights(List<Vector2> directions)
        {
            Vector2 diff = target.GetCalculatedPosition() - rootObjectNode.GlobalPosition;
            float dist = diff.Length();
            List<float> weights = new List<float>();
            foreach (Vector2 potentialDirection in directions)
            {
                if (
                    potentialDirection == Vector2.Zero && dist <= maxDistance && dist >= minDistance
                )
                {
                    weights.Add(float.MaxValue);
                }
                // Too close
                else if (dist < minDistance)
                {
                    weights.Add(-potentialDirection.Normalized().Dot(diff));
                }
                // Too far
                else
                {
                    weights.Add(potentialDirection.Normalized().Dot(diff));
                }
            }

            return weights;
        }
    }
}
