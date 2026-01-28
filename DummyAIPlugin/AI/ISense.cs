using UnityEngine;

namespace DummyAIPlugin.AI;

/// <summary>
/// Implement this interface to create new AI sense.
/// </summary>
public interface ISense
{
    /// <summary>
    /// Callback triggered on collider entry.
    /// </summary>
    /// <param name="other">Detected collider.</param>
    void ProcessEnter(Collider other);

    /// <summary>
    /// Callback triggered on collider exit.
    /// </summary>
    /// <param name="other">Detected collider.</param>
    void ProcessExit(Collider other);

    /// <summary>
    /// Performs sense update.
    /// </summary>
    void Update();
}
