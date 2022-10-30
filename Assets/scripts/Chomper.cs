using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chomper : MonoBehaviour
{
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
        _navMeshAgent.destination = target.position;
    }
}
