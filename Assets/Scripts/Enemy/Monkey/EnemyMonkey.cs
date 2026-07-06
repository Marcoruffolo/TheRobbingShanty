using UnityEngine;

public class EnemyMonkey : EnemyBase
{
    private enum State { OnNode, Jumping, Throwing }

    [Header("Config")]
    [SerializeField] private float throwRange = 10f;
    [SerializeField] private float throwDelay = 0.4f;
    [SerializeField] private TreeNodeAnchor startingNode;
    [SerializeField] private bool combatEnabled = true;
    [SerializeField] private float nodeWaitTime = 2f;

    private MonkeyTreeHopper _hopper;
    private BombThrower _bombThrower;
    private State _state = State.OnNode;
    private bool _isFleeing;
    private float _nodeArrivalTime;

    protected override void Awake()
    {
        base.Awake();
        _hopper = GetComponent<MonkeyTreeHopper>();
        _bombThrower = GetComponent<BombThrower>();
    }

    protected override void Start()
    {
        base.Start();
        if (startingNode != null) _hopper.SetInitialNode(startingNode);
        _nodeArrivalTime = Time.time;
    }

    private void Update()
    {
        if (IsDead || IsControlLocked) return;
        if (_state == State.OnNode) UpdateOnNode();
    }

    private void UpdateOnNode()
    {
        if (_hopper.CurrentNode == null) return;

        bool playerInThrowRange = combatEnabled && Fov.CanSeePlayer && Fov.PlayerTransform != null &&
            Vector3.Distance(transform.position, Fov.PlayerTransform.position) <= throwRange &&
            _hopper.HasClearLineOfSight(Fov.PlayerTransform.position);

        if (playerInThrowRange && !_isFleeing && _bombThrower.CanThrow)
        {
            EnterThrowing();
            return;
        }

        if (Time.time - _nodeArrivalTime < nodeWaitTime) return;

        if (_hopper.TryFindDirectJumpTarget(out TreeNodeAnchor directTarget))
        {
            EnterJumping(directTarget);
            return;
        }

        if (_hopper.TryFindRepositionTarget(out TreeNodeAnchor repositionTarget))
            EnterJumping(repositionTarget);
    }

    private void EnterThrowing()
    {
        _state = State.Throwing;
        Animator?.SetTrigger(HashAttack);
        Invoke(nameof(DoThrow), throwDelay);
    }

    private void DoThrow()
    {
        if (IsDead || IsControlLocked || Fov.PlayerTransform == null ||
            !_hopper.HasClearLineOfSight(Fov.PlayerTransform.position))
        {
            _state = State.OnNode;
            return;
        }

        _bombThrower.ThrowAt(Fov.PlayerTransform.position, _hopper.CurrentNode.OwnerTree);
        _isFleeing = true;
        _state = State.OnNode;
    }

    private void EnterJumping(TreeNodeAnchor target)
    {
        if (_hopper.JumpTo(target, OnJumpComplete))
            _state = State.Jumping;
    }

    private void OnJumpComplete(bool wasDifferentTree)
    {
        if (wasDifferentTree) _isFleeing = false;
        _state = State.OnNode;
        _nodeArrivalTime = Time.time;
    }

    protected override void Die()
    {
        _hopper.StopAllCoroutines();
        base.Die();
    }

    public override void OnHitFrame() { }
    public override void OnAttackEnd() { }
}
