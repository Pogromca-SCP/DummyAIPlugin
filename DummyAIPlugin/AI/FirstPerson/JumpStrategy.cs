using PlayerRoles.FirstPersonControl;

namespace DummyAIPlugin.AI.FirstPerson;

/// <summary>
/// First person jump action.
/// </summary>
/// <param name="fpcModule">First person control module to use.</param>
public class JumpStrategy(FirstPersonMovementModule fpcModule) : IActionStrategy
{
    /// <inheritdoc />
    public bool CanPerform => !_controller.IsJumping && _fpcModule.IsGrounded;//changed to also check if dummy is touching ground

    /// <inheritdoc />
    public bool Complete => _controller.IsJumping;

    /// <summary>
    /// Contains FirstPersonMovementModule for character state checking.
    /// </summary>
    private readonly FirstPersonMovementModule _fpcModule = fpcModule;

    /// <summary>
    /// Contains used first person jump controller.
    /// </summary>
    private readonly FpcJumpController _controller = fpcModule.Motor.JumpController;

    /// <inheritdoc />
    public void Start()
    {
        if (CanPerform)//made an if that can probably be simplified but proof of fix mostly
        {
            //changed 1.0f to fpcModule.Motor.MainModule.JumpSpeed, 1.0f does a small impulse instead of the characters jump force  so it jiggles without this
            _controller.ForceJump(_fpcModule.Motor.MainModule.JumpSpeed);
        }
    }
    /// <inheritdoc />
    public void Update() { }

    /// <inheritdoc />
    public void Stop() { }
}
