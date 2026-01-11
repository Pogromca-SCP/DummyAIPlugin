using PlayerRoles.FirstPersonControl;
using UnityEngine;

namespace DummyAIPlugin.AI.FirstPerson;

/// <summary>
/// First person wander action.
/// </summary>
/// <param name="fpcModule">First person control module to use.</param>
/// <param name="wanderRadius">Wander sphere radius.</param>
public class Wander(FirstPersonMovementModule fpcModule, float wanderRadius) : IActionStrategy
{
    /// <inheritdoc />
    public bool CanPerform => !_motor.MovementDetected;

    /// <inheritdoc />
    public bool Complete => _motor.MovementDetected;

    /// <summary>
    /// Contains used first person control motor.
    /// </summary>
    private readonly FpcMotor _motor = fpcModule.Motor;

    /// <summary>
    /// Contains used first person control mouse look.
    /// </summary>
    private readonly FpcMouseLook _mouseLook = fpcModule.MouseLook;

    /// <summary>
    /// Contains wander sphere radius.
    /// </summary>
    private readonly float _wanderRadius = wanderRadius;

    /// <inheritdoc />
    public void Start()
    {
        var randomDirection = Random.insideUnitSphere * _wanderRadius;
        randomDirection = new(randomDirection.x, 0.0f, randomDirection.z);
        randomDirection = _motor.Hub.PlayerCameraReference.TransformDirection(randomDirection).NormalizeIgnoreY();
        randomDirection = _motor.Position + randomDirection * 3.0f;
        _mouseLook.LookAtDirection(randomDirection - _motor.Position);
        _motor.ReceivedPosition = new(randomDirection);
    }

    /// <inheritdoc />
    public void Update() {}

    /// <inheritdoc />
    public void Stop() {}
}
