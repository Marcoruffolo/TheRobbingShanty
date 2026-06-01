using UnityEngine;
using UnityEngine.Events;

public class ClimbableSurface : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionPrompt = "Para Subir";

    [SerializeField] private Transform bottomPoint;
    [SerializeField] private Transform topPoint;
    [SerializeField] private Transform bottomExitPoint;
    [SerializeField] private Transform topExitPoint;


    [SerializeField] private float climbSpeed  = 1f;
    [SerializeField] private float alignmentSpeed = 1f;
    [SerializeField] private float maxGrabDistance = 1f;

    [SerializeField] private float topExitTolerance = 1f;
    [SerializeField] private float bottomExitTolerance = 1f;
    [SerializeField] private bool requireGroundForBottomExit;
    [SerializeField] float bottomGroundCheckDistance;
    [SerializeField] private LayerMask groundMask;

    [SerializeField] private bool allowJumpExit = true;

    private PlayerSurfaceClimber _activeClimber;

    public string InteractionPrompt => interactionPrompt;

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public float ClimbSpeed => climbSpeed;
    public float AlignmentSpeed => alignmentSpeed;
    public bool AllowJumpExit => allowJumpExit;

    public float PathLength => (topPoint.position - bottomPoint.position).magnitude;
    public Vector3 PathDirection => (topPoint.position - bottomPoint.position).normalized;


    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        PlayerSurfaceClimber climber = interactor.GetComponentInParent<PlayerSurfaceClimber>();
        interactSuccessful = climber.TryStartClimb(this);

        if (interactSuccessful) _activeClimber = climber;
    }
    
    public void Interact()
    {
        FindAnyObjectByType<PlayerSurfaceClimber>().TryStartClimb(this);
    }

    public void EndInteraction()
    {
        if (_activeClimber != null && _activeClimber.ActiveSurface == this)
            _activeClimber.StopClimb(ClimbExitReason.Manual, false);

        _activeClimber = null;
        OnInteractionComplete?.Invoke(this);
    }

    public bool CanStartClimb(Vector3 playerPosition, out Vector3 closestPoint, out float progress)
    {
        ProjectPosition(playerPosition, out closestPoint, out progress);
        return Vector3.Distance(playerPosition, closestPoint) <= maxGrabDistance;
    }

    public Vector3 GetPointAtProgress(float progress)
    {
        return Vector3.Lerp(bottomPoint.position, topPoint.position, Mathf.Clamp01(progress));
    }

    public bool CanExitTop(float progress, Vector3 currentPathPoint)
    {
        return progress >= 1f || Vector3.Distance(currentPathPoint, topPoint.position) <= topExitTolerance;
    }

    public bool CanExitBottom(float progress, Vector3 currentPathPoint)
    {
        bool reachedBottom = progress <= 0f || Vector3.Distance(currentPathPoint, bottomPoint.position) <= bottomExitTolerance;
        return reachedBottom && HasValidBottomGround();
    }

    public Transform GetExitPoint(ClimbExitReason exitReason)
    {
        return exitReason switch
        {
            ClimbExitReason.Top => topExitPoint,
            ClimbExitReason.Bottom => bottomExitPoint,
            _ => null
        };
    }

    public void NotifyClimbFinished(PlayerSurfaceClimber climber)
    {
        if (_activeClimber == climber)
            _activeClimber = null;

        OnInteractionComplete?.Invoke(this);
    }

    private void ProjectPosition(Vector3 position, out Vector3 closestPoint, out float progress)
    {
        Vector3 axis = topPoint.position - bottomPoint.position;
        progress = Mathf.Clamp01(Vector3.Dot(position - bottomPoint.position, axis) / axis.sqrMagnitude);
        closestPoint = bottomPoint.position + axis * progress;
    }

    private bool HasValidBottomGround()
    {
        if (!requireGroundForBottomExit)
            return true;

        return Physics.Raycast(
            bottomExitPoint.position,
            Vector3.down,
            bottomGroundCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }
}
