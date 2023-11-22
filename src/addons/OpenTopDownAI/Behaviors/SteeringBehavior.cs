using Godot;
using System;
using System.Collections.Generic;

public interface SteeringBehavior
{
    public List<float> CalculateWeights(List<Vector2> directions);
}
