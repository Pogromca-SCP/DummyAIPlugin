using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DummyAIPlugin.Navigation;

/// <summary>
/// Represents a NavMesh path which can be recalculated.
/// </summary>
/// <param name="parent">Agent which uses this path.</param>
/// <param name="waypointRadius">Radius size used progress detection.</param>
public class Path(IPathParent parent, float waypointRadius = 2.0f)
{
    /// <summary>
    /// Retrieves current waypoint.
    /// </summary>
    public Vector3 CurrentWaypoint => Waypoints.Count > 0 ? Waypoints[Current] : Parent.Destination;

    /// <summary>
    /// Provides information about agent which uses this path.
    /// </summary>
    public IPathParent Parent { get; } = parent;

    /// <summary>
    /// Stores information about NavMesh path.
    /// </summary>
    public NavMeshPath CurrentPath { get; } = new();

    /// <summary>
    /// Stores waypoints which are used for agent navigation.
    /// </summary>
    public List<Vector3> Waypoints { get; } = [];

    /// <summary>
    /// Radius size used progress detection.
    /// </summary>
    public float WaypointRadius { get; } = waypointRadius;

    /// <summary>
    /// Tells whether or not the path end was reached.
    /// </summary>
    public bool Ended { get; private set; } = false;

    /// <summary>
    /// Contains current waypoint index.
    /// </summary>
    public int Current
    {
        get;
        private set => field = Waypoints.Count > 0 ? Mathf.Clamp(value, 0, Waypoints.Count - 1) : 0;
    }

    /// <summary>
    /// Recalculates NavMesh path and resets path state to the beginning.
    /// </summary>
    public void UpdatePath()
    {
        NavMesh.SamplePosition(Parent.Position, out var hit, 2.0f, NavMesh.AllAreas);
        NavMesh.CalculatePath(hit.position, Parent.Destination, NavMesh.AllAreas, CurrentPath);
        Waypoints.Clear();
        Waypoints.AddRange(CurrentPath.corners);
        Waypoints.Add(Parent.Destination);
        Current = 0;
        Ended = false;
        UpdateWaypoint();
    }

    /// <summary>
    /// Evaluates and updates current path progress.
    /// </summary>
    public void UpdateWaypoint()
    {
        if (Ended)
        {
            return;
        }

        if (Current >= Waypoints.Count - 1)
        {
            Ended = true;
            return;
        }

        if ((CurrentWaypoint - Parent.Position).sqrMagnitude < WaypointRadius * WaypointRadius)
        {
            ++Current;
        }
    }
}
