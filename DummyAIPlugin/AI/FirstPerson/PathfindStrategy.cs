using DummyAIPlugin.Navigation;
using DummyAIPlugin.Utils;
using PlayerRoles.FirstPersonControl;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace DummyAIPlugin.AI.FirstPerson;

/// <summary>
/// First person pathfinding action.
/// </summary>
/// <param name="fpcModule">FPC module to use.</param>
/// <param name="cancellationBelief">Optional belief to cancel pathfinding.</param>
public class PathfindStrategy(FirstPersonMovementModule fpcModule, Belief? cancellationBelief = null) : IActionStrategy, IPathParent
{
    /// <inheritdoc />
    public bool CanPerform => Target is not null;

    /// <inheritdoc />
    public bool Complete => (_cancellationBelief is not null && _cancellationBelief.Evaluate()) || (Destination - _motor.Position).sqrMagnitude <= 1.0f;

    /// <inheritdoc />
    public Vector3 Position => _motor.Position;

    /// <summary>
    /// Contains current target position to reach.
    /// </summary>
    public Func<Vector3>? Target { get; set; } = null;

    /// <inheritdoc />
    public Vector3 Destination
    {
        get;
        set
        {
            if (value == _motor.Position)
            {
                field = value;
                return;
            }

            field = !NavMesh.SamplePosition(value, out var hit, 50.0f, NavMesh.AllAreas) ? value : hit.position;
        }
    }

    /// <summary>
    /// Contains used first person control motor.
    /// </summary>
    private readonly FpcMotor _motor = fpcModule.Motor;

    /// <summary>
    /// Contains used first person control mouse look.
    /// </summary>
    private readonly FpcMouseLook _mouseLook = fpcModule.MouseLook;

    /// <summary>
    /// Stores belief used to cancel pathfinding process.
    /// </summary>
    private readonly Belief? _cancellationBelief = cancellationBelief;

    /// <summary>
    /// Timer used for delays between waypoint updates.
    /// </summary>
    private readonly CountdownTimer _timer = new(0.3f);

    /// <summary>
    /// Contains currently followed path.
    /// </summary>
    private Path? _path;

    /// <summary>
    /// Contains remaining amount of time before next path update.
    /// </summary>
    private float _repath = 0.3f;

    /// <summary>
    /// Contains previously requested movement force.
    /// </summary>
    private Vector3 _requestedForce = Vector3.zero;

    /// <inheritdoc />
    public void Start()
    {
        Destination = Target is null ? _motor.Position : Target();
        _path ??= new(this);
    }

    /// <inheritdoc />
    public void Update()
    {
        var isAtDestination = (Destination - _motor.Position).sqrMagnitude <= 1.0f;
        _requestedForce = Vector3.MoveTowards(_requestedForce, Vector3.zero, 30.0f * Time.deltaTime);

        if (isAtDestination)
        {
            Destination = _motor.Position;
            return;
        }

        _timer.Tick(Time.deltaTime);

        if (!_timer.IsFinished)
        {
            return;
        }

        _repath -= Time.deltaTime;

        if (_repath > 0.0f)
        {
            _path!.UpdateWaypoint();
        }
        else
        {
            _path!.UpdatePath();
            _repath = 0.3f;
        }

        var waypoint = _path.CurrentWaypoint;
        var pos = _motor.Position;
        waypoint.y = 0.0f;
        pos.y = 0.0f;
        var direction = (waypoint - pos).normalized;
        var targetPosition = _motor.Position + direction * (30.0f * Time.deltaTime);
        _mouseLook.LookAtDirection(targetPosition - _motor.Position);
        _motor.ReceivedPosition = new(targetPosition);
    }

    /// <inheritdoc />
    public void Stop() {}
}
