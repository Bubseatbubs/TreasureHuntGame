using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Implementation of A* pathfinding based on:
// https://www.redblobgames.com/pathfinding/a-star/implementation.html#csharp
public class Pathfinding : MonoBehaviour
{
    public static Dictionary<MazeCell, MazeCell> cameFrom
        = new Dictionary<MazeCell, MazeCell>();
    public static Dictionary<MazeCell, double> costSoFar
        = new Dictionary<MazeCell, double>();

    static public double Heuristic(MazeCell a, MazeCell b)
    {
        return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
    }

    public static Stack<Vector2> AStarSearch(MazeCell start, MazeCell goal)
    {
        if (start == null || goal == null) {
            return null;
        }

        Debug.DrawLine(start.GetWorldPosition(), start.GetWorldPosition() + new Vector2(1, 1), Color.magenta, 3f);
        Debug.DrawLine(goal.GetWorldPosition(), goal.GetWorldPosition() + new Vector2(1, 1), Color.green, 3f);
        var frontier = new PriorityQueue<MazeCell, double>();
        frontier.Enqueue(start, 0);

        cameFrom[start] = start;
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            if (current.Equals(goal))
            {
                break;
            }

            foreach (var next in current.connections)
            {
                // Add 1 as maze cells are always 1 space away from one another
                double newCost = costSoFar[current] + 1;
                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    double priority = newCost + Heuristic(next, goal);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }

        Stack<Vector2> moveCommands = new Stack<Vector2>();
        MazeCell backtrackCell = goal;
        moveCommands.Push(backtrackCell.GetWorldPosition());
        int attempts = 0;
        int maxAttempts = 200;
        while (backtrackCell.GetWorldPosition() != start.GetWorldPosition())
        {
            if (attempts >= maxAttempts)
            {
                Debug.LogWarning("Couldn't generate path!");
                break;
            }

            Debug.DrawLine(backtrackCell.GetWorldPosition(), cameFrom[backtrackCell].GetWorldPosition(), Color.blue, 3f);
            backtrackCell = cameFrom[backtrackCell];
            moveCommands.Push(backtrackCell.GetWorldPosition());
            attempts++;
        }

        cameFrom.Clear();
        costSoFar.Clear();

        return moveCommands;
    }
}

