using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DummyAIPlugin.AI.Senses;

/// <summary>
/// A base sight sense class used to detect specific components.
/// </summary>
/// <typeparam name="TComponent">Type of components to detect.</typeparam>
/// <param name="dummy">Target dummy's reference hub.</param>
public abstract class SightSense<TComponent>(ReferenceHub dummy) : ISense where TComponent : Component
{
    /// <summary>
    /// Attempts to retrieve target component from collider.
    /// </summary>
    /// <param name="collider">Collider to use.</param>
    /// <returns>Retrieved component instance or <see langword="null" /> if nothing was found.</returns>
    protected static TComponent? GetComponent(Collider collider) => collider.GetComponentInParent<TComponent>();

    /// <summary>
    /// Holds FOV*2 of character for <see cref="SightSense{TComponent}"/>, divide desired FOV by 2.
    /// </summary>
    public float FOV { get; set; } = 90.0f;

    /// <summary>
    /// Contains dummy's reference hub.
    /// </summary>
    public ReferenceHub Dummy { get; } = dummy;

    /// <summary>
    /// Contains components within sight.
    /// </summary>
    public HashSet<TComponent> ComponentsWithinSight { get; } = [];

    /// <summary>
    /// Contains physics layer mask used for detection.
    /// </summary>
    protected abstract LayerMask LayerMask { get; }

    /// <summary>
    /// Provides a mapping from colliders to components.
    /// </summary>
    private readonly Dictionary<Collider, TComponent> _collidersToComponent = [];

    /// <summary>
    /// Contains dummy's instance id.
    /// </summary>
    private readonly int _dummyInstanceID = dummy.gameObject.GetInstanceID();

    /// <summary>
    /// Stores collision mask used for obstruction detection.
    /// </summary>
    private readonly LayerMask _obstructionLayerMask =
        LayerMask.GetMask(Perception.DefaultLayer, Perception.DoorLayer, Perception.InteractableLayer);

    /// <inheritdoc />
    public void ProcessEnter(Collider other)
    {
        if ((LayerMask & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }

        var component = GetComponent(other);

        if (component && component.gameObject.GetInstanceID() != _dummyInstanceID)
        {
            _collidersToComponent[other] = component;
        }
    }

    /// <inheritdoc />
    public void ProcessExit(Collider other)
    {
        if ((LayerMask & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }

        var component = GetComponent(other);

        if (component && component.gameObject.GetInstanceID() != _dummyInstanceID)
        {
            _collidersToComponent.Remove(other);

            if (!_collidersToComponent.ContainsValue(component))
            {
                ComponentsWithinSight.Remove(component);
            }
        }
    }

    /// <inheritdoc />
    public void Update()
    {
        foreach (var group in _collidersToComponent.GroupBy(mapping => mapping.Value))
        {
            var visible = false;
            var component = group.Key;

            foreach (var collider in group.Select(ctc => ctc.Key))
            {
                var center = collider.bounds.center;

                if (IsPositionWithinFov(center) && !IsPositionObstructed(center))
                {
                    visible = true;
                    break;
                }
            }

            if (visible)
            {
                ComponentsWithinSight.Add(component);
            }
            else
            {
                ComponentsWithinSight.Remove(component);
            }
        }
    }

    /// <summary>
    /// Calculates the distance to target vector.
    /// </summary>
    /// <param name="targetPosition">Target vector.</param>
    /// <returns>Calculated distance value.</returns>
    public float GetDistanceToPosition(Vector3 targetPosition) => Vector3.Distance(targetPosition, Dummy.PlayerCameraReference.position);

    /// <summary>
    /// Checks if target vector visibility is obstructed.
    /// </summary>
    /// <param name="targetPosition">Target vector to check.</param>
    /// <returns>Whether or not the target vector visibility is obstructed.</returns>
    public bool IsPositionObstructed(Vector3 targetPosition) => IsPositionObstructed(targetPosition, out _);

    /// <summary>
    /// Checks if target vector visibility is obstructed.
    /// </summary>
    /// <param name="targetPosition">Target vector to check.</param>
    /// <param name="obstructtionHit">Hit detection for obstructing object or <see langword="default" /> if no obstruction detected.</param>
    /// <returns>Whether or not the target vector visibility is obstructed.</returns>
    public bool IsPositionObstructed(Vector3 targetPosition, out RaycastHit obstructtionHit)
    {
        var camera = Dummy.PlayerCameraReference;
        var cameraPosition = camera.position + camera.forward;
        var isObstructed = Physics.Linecast(cameraPosition, targetPosition, out var hit, _obstructionLayerMask, QueryTriggerInteraction.Ignore);
        obstructtionHit = isObstructed ? hit : default;
        return isObstructed;
    }

    /// <summary>
    /// Checks if target vector is located within camera's FOV.
    /// </summary>
    /// <param name="targetPosition">Target vector to check.</param>
    /// <returns>Whether or not the target position is within camera's FOV.</returns>
    public bool IsPositionWithinFov(Vector3 targetPosition)
    {
        var camera = Dummy.PlayerCameraReference;
        return IsWithinFov(camera.position, camera.forward, targetPosition);
    }

    /// <summary>
    /// Checks if target transform is located within camera's FOV.
    /// </summary>
    /// <param name="transform">Camera transform.</param>
    /// <param name="targetTransform">Target transform to check.</param>
    /// <returns>Whether or not the target transform is within camera's FOV.</returns>
    protected bool IsWithinFov(Transform transform, Transform targetTransform) =>
        IsWithinFov(transform.position, transform.forward, targetTransform.position);

    /// <summary>
    /// Checks if target vector is located within camera's FOV.
    /// </summary>
    /// <param name="position">Camera position.</param>
    /// <param name="forward">Camera forward vector.</param>
    /// <param name="targetPosition">Target position to check.</param>
    /// <returns>Whether or not the target position is within camera's FOV.</returns>
    protected bool IsWithinFov(Vector3 position, Vector3 forward, Vector3 targetPosition)
    {
        var diff = Vector3.Normalize(targetPosition - position);

        if (Vector3.Dot(forward, diff) < 0.0f)
        {
            return false;
        }

        if (Vector3.Angle(forward, diff) > FOV)
        {
            return false;
        }

        return true;
    }
}