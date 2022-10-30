using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chomper : MonoBehaviour
{
    private Vector3 _previousPosition;
    public float _curSpeed;

    public Transform target;
    private Animator _animator;
    private UnityEngine.AI.NavMeshAgent _navMeshAgent;

    private void Awake()
    {
        TryGetComponent(out _animator);
        TryGetComponent(out _navMeshAgent);

    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 curMove = transform.position - _previousPosition;
        _curSpeed = curMove.magnitude / Time.deltaTime;
        _previousPosition = transform.position;
        _navMeshAgent.destination = target.position;
        _animator.SetFloat("Speed", _curSpeed);
    }
}
