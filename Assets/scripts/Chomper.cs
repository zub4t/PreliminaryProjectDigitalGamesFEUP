using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chomper : MonoBehaviour
{
    private Vector3 _previousPosition;
    public float _curSpeed;

    private int _life = 10;
    private Vector3 _target;
    private Animator _animator;
    private UnityEngine.AI.NavMeshAgent _navMeshAgent;
    private bool _canChange;
    private GameObject _player;
    private bool _canMove;
    private void Awake()
    {
        TryGetComponent(out _animator);
        TryGetComponent(out _navMeshAgent);

    }
    // Start is called before the first frame update
    void Start()
    {
        _canChange = true;
        _canMove = true;
    }

    // Update is called once per frame
    void Update()
    {

        _player = GameObject.Find("Capoerista");
        float dist = Vector3.Distance(_target, transform.position);
        float distToPlayer = Vector3.Distance(_player.transform.position, transform.position);

        if (distToPlayer < 10)
        {

            _canChange = false;
        }
        if (MazeGenerator.cell != null)
            if (dist < 10 && _canChange && MazeGenerator.cell.Length > 0)
            {
                var randomIndexCell = Random.Range(0, MazeGenerator.cell.Length - 1);
                var randomCell = MazeGenerator.cell[randomIndexCell];
                _target = randomCell.GetWorldPosition();
            }
        if (!_canChange)
        {
            _target = _player.transform.position - transform.forward * 1;

        }


        Vector3 curMove = transform.position - _previousPosition;
        _curSpeed = curMove.magnitude / Time.deltaTime;
        _previousPosition = transform.position;
        _animator.SetFloat("Speed", _curSpeed);
        _animator.SetFloat("Dst", distToPlayer);
        if (_canMove)
        {
            _navMeshAgent.destination = _target;

        }
        else
        {
            _navMeshAgent.destination = transform.position;

        }



    }
    public void DisbleHit()
    {

        _animator.SetBool("Hit1", false);
        _animator.SetBool("Hit2", false);
        _animator.SetBool("Hit3", false);

    }
    public void Hited()
    {
        _canMove = false;
        StartCoroutine(MoveAgain());
        if (_life == 0)
        {
            Destroy(gameObject, 0.2f);


        }
        else
        {
            _life--;

            // _animator.SetBool("Hited" + Random.Range(1, 3), true);
            _animator.SetBool("Hit3", true);



        }

    }

    IEnumerator MoveAgain()
    {

        yield return new WaitForSeconds(3f);
        _canMove = true;
    }

    void FixedUpdate()
    {

    }


    private void Check()
    {
        
            float dist = Vector3.Distance(transform.position, _player.transform.position);
            //float dot = Vector3.Dot(transform.forward, _player.transform.forward);
            if (dist < 2)
            {
            _player.gameObject.GetComponent<PlayerMovement>().Hit();

            }

        

        
    }

    void Atk()
    {
        Check();
    }
}
