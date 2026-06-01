using UnityEngine;
public enum ClimbExitReason
{
    Manual,
    Jump,
    Top,
    Bottom
}

public class PlayerSurfaceClimber : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerInputHandler input;

    private Vector3 _pathOffset;
    private bool _movementWasEnabled;

    public bool IsClimbing { get; private set; }
    public float ClimbProgress { get; private set; }
    public ClimbableSurface ActiveSurface { get; private set; }


    private void OnEnable()
    {
        input.OnJump += HandleJump;
    }

    private void OnDisable()
    {
        if (IsClimbing)
            StopClimb(ClimbExitReason.Manual, true);

        input.OnJump -= HandleJump;
    }


    void Update()
    {

        if (!IsClimbing)
            return;

        float previousProgress = ClimbProgress;
        float climbInput = input.MoveInput.y;

        ClimbProgress = Mathf.Clamp01(
            ClimbProgress + (climbInput * ActiveSurface.ClimbSpeed * Time.deltaTime) / ActiveSurface.PathLength
        );

        Vector3 pathPoint = ActiveSurface.GetPointAtProgress(ClimbProgress);
        _pathOffset = Vector3.MoveTowards(_pathOffset, Vector3.zero, ActiveSurface.AlignmentSpeed * Time.deltaTime);

        controller.Move(pathPoint + _pathOffset - transform.position);

        if (ClimbProgress > previousProgress && ActiveSurface.CanExitTop(ClimbProgress, pathPoint))
        {
            StopClimb(ClimbExitReason.Top, true);
        }
        else if (ClimbProgress < previousProgress && ActiveSurface.CanExitBottom(ClimbProgress, pathPoint))
        {
            StopClimb(ClimbExitReason.Bottom, true);
        }
    }

    public bool TryStartClimb(ClimbableSurface surface)
    {
        if (IsClimbing || !playerMovement.enabled)
            return false;

        if (!surface.CanStartClimb(transform.position, out Vector3 pathPoint, out float progress))
            return false;

        IsClimbing = true;
        ActiveSurface = surface;
        ClimbProgress = progress;
        _pathOffset = Vector3.ProjectOnPlane(transform.position - pathPoint, surface.PathDirection);
        _movementWasEnabled = playerMovement.enabled;

        playerMovement.ResetMotionState();
        playerMovement.enabled = false;

        return true;
    }

    public void StopClimb(ClimbExitReason exitReason, bool notifySurface)
    {
        if (!IsClimbing)
            return;

        ClimbableSurface finishedSurface = ActiveSurface;

        IsClimbing = false;
        ActiveSurface = null;
        ClimbProgress = 0f;
        _pathOffset = Vector3.zero;

        if (exitReason == ClimbExitReason.Top || exitReason == ClimbExitReason.Bottom)
        {
            Transform exitPoint = finishedSurface.GetExitPoint(exitReason);
            playerMovement.TeleportTo(exitPoint.position, transform.rotation);
        }

        playerMovement.ResetMotionState();
        playerMovement.enabled = _movementWasEnabled;

        if (notifySurface)
            finishedSurface.NotifyClimbFinished(this);
    }

    private void HandleJump()
    {
        if (IsClimbing && ActiveSurface.AllowJumpExit)
            StopClimb(ClimbExitReason.Jump, true);
    }
}
