using PlayerRoles.FirstPersonControl;

namespace DummyAIPlugin.AI.FirstPerson;

/// <summary>
/// First person jump action.
/// </summary>
/// <param name="fpcModule">First person control module to use.</param>
public class JumpStrategy(FirstPersonMovementModule fpcModule) : IActionStrategy
{
    /// <inheritdoc />
    public bool CanPerform => !_controller.IsJumping;

    /// <inheritdoc />
    public bool Complete => _controller.IsJumping;

    /// <summary>
    /// Contains used first person jump controller.
    /// </summary>
    private readonly FpcJumpController _controller = fpcModule.Motor.JumpController;

    /// <inheritdoc />
    public void Start() => _controller.ForceJump(1.0f);

    /// <inheritdoc />
    public void Update() {}

    /// <inheritdoc />
    public void Stop() {}
}
