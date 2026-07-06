using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyRagdoll : MonoBehaviour
{
    private Animator _animator;
    private NavMeshAgent _agent;
    private Rigidbody[] _bodies;
    private Rigidbody _rootRigidbody;
    private Collider _mainCollider;
    private SkinnedMeshRenderer[] _meshes;

    public Transform RagdollAnchor => _bodies.Length > 0 ? _bodies[0].transform : transform;

    void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _mainCollider = GetComponent<Collider>();
        _rootRigidbody = GetComponent<Rigidbody>();
        _bodies = GetComponentsInChildren<Rigidbody>();
        _meshes = GetComponentsInChildren<SkinnedMeshRenderer>();

        SetKinematic(true);
    }

    public void Activate()
    {
        foreach (var mesh in _meshes)
            mesh.updateWhenOffscreen = true;

        if (_animator != null) _animator.enabled = false;
        if (_agent != null) _agent.enabled = false;
        if (_mainCollider != null) _mainCollider.enabled = false;

        SetKinematic(false);
    }

    void SetKinematic(bool kinematic)
    {
        foreach (var rb in _bodies)
            rb.isKinematic = kinematic;
    }

    private Rigidbody[] GetRagdollBodies()
    {
        Rigidbody[] bodies = GetComponentsInChildren<Rigidbody>();
        if (_rootRigidbody == null)
            return bodies;

        List<Rigidbody> ragdollBodies = new List<Rigidbody>(bodies.Length);
        foreach (var body in bodies)
        {
            if (body != _rootRigidbody)
                ragdollBodies.Add(body);
        }

        return ragdollBodies.ToArray();
    }
}
