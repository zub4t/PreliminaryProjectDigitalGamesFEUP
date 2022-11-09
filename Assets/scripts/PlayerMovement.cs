using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using Input = UnityEngine.Input;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private GameObject Victory;
    [SerializeField]
    private GameObject YouDieMenu;
    [SerializeField]
    private GameObject h1;
    [SerializeField]
    private GameObject h2;
    [SerializeField]
    private GameObject h3;
    [Header("Player")]
    [Tooltip("How many hits can the player handle")]
    [SerializeField]
    private int _life;
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 5.335f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;
    public AudioClip SwordClip;
    public AudioClip KickClip;
    public AudioClip DamageClip;
    public AudioClip[] RandomSpeakClips;
    public AudioClip CoinCollected;

    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;


    [SerializeField]
    private TextMeshProUGUI _textCoin;
    private int _nCoins;

    public GameObject Sword;
    public GameObject Kick;

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // player
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;
    private bool _speaking;
    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    private AudioSource _audio;

    private float _timeMoving = 0f;

    private Animator _animator;
    private CharacterController _controller;
    private GameObject _mainCamera;
    private Vector2 _mousePosition;
    private const float _threshold = 0.01f;
    public float horizontalSpeed = 2.0F;
    public float verticalSpeed = 2.0F;



    private void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }
    private bool _hasAnimator;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        _hasAnimator = TryGetComponent(out _animator);
        _controller = GetComponent<CharacterController>();
        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
        EnableMovement();
        _audio = GetComponent<AudioSource>();
    }

    private void LateUpdate()
    {
        CameraRotation();
    }
    // Update is called once per frame
    void Update()
    {

        if ((_life > 0))
        {


            GroundedCheck();

            JumpAndGravity();
            Move();

            BattleMode();


            if (_animator.GetBool("CanMove") && !_speaking)
            {
                int r = UnityEngine.Random.Range(0, 1000);
                if (r < 50)
                {
                    _audio.time = 0.0f;
                    _speaking = true;
                    _audio.clip = RandomSpeakClips[UnityEngine.Random.Range(0, RandomSpeakClips.Length - 1)];
                    _audio.Play();
                    StartCoroutine(CanSpeakAgain());
                }
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Debug.Log("You die");
            var enemies = GameObject.FindGameObjectsWithTag("enemy");

            foreach (GameObject enemy in enemies)
            {
                Destroy(enemy);
            }
            YouDieMenu.active = true;
        }
    }
    IEnumerator CanSpeakAgain()
    {
        yield return new WaitForSeconds(10f);
        _speaking = false;
    }
    public void SwordON()
    {
        Sword.active = true;
        _audio.time = 0.8f;
        _audio.clip = SwordClip;

        _audio.Play();

    }
    public void SwordOFF()
    {
        Sword.active = false;
    }
    public void KickON()
    {
        _audio.time = 0.0f;
        _audio.clip = KickClip;
        Kick.active = true;
        _audio.Play();

    }
    public void KickOFF()
    {
        Kick.active = false;
    }
    public void DesableHit()
    {
        Debug.Log("DesableHit");
        _animator.SetBool("Hit", false);
    }
    public void DesableDie()
    {
        Debug.Log("DisableDie");
        _animator.SetBool("Die", false);
    }
    public void Hit()
    {
        if (_life == 1)
        {
            _animator.SetBool("Die", true);

        }
        else
        {
            _animator.SetBool("Hit", true);

        }
        switch (_life)
        {
            case 3:
                h3.active = false;
                break;
            case 2:
                h2.active = false;
                 break;
            case 1:
                h1.active = false;
                break;

        }
        _life--;
    }
    private void Check()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("enemy"))
        {
            float dist = Vector3.Distance(transform.position, obj.transform.position);
            float dot = Vector3.Dot(transform.forward, obj.transform.forward);
            if (dist < 3 && dot < 0)
            {
                obj.gameObject.GetComponent<Chomper>().Hited();

            }

            /*
            RaycastHit hit;

            Vector3 p1 = transform.position + _controller.center;
            float distanceToObstacle = 0;

            // Cast a sphere wrapping character controller 10 meters forward
            // to see if it is about to hit anything.
            if (Physics.SphereCast(p1, _controller.height / 2, transform.forward, out hit, 10, 1 << 8))
            {
                distanceToObstacle = hit.distance;
                Debug.Log(hit.collider.tag);
                if (hit.collider.CompareTag("enemy"))
                {
                    hit.collider.gameObject.GetComponent<Chomper>().Hited();
                }

            }*/


        }
    }

    public float explosionRadius = 5.0f;

    void OnDrawGizmosSelected()
    {
        // Display the explosion radius when selected
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
    void BattleMode()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            _animator.SetBool("BattleMode", !_animator.GetBool("BattleMode"));

        }
        if (Input.GetMouseButtonDown(0))
        {
            _animator.SetBool("Punch", true);


            Check();



        }
        if (Input.GetMouseButtonDown(1))
        {
            _animator.SetBool("Kick", true);
            Check();

        }
    }


    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);


        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetBool("isGrounded", Grounded);
        }
    }

    private void CameraRotation()
    {
        float h = horizontalSpeed * Input.GetAxis("Mouse X");
        float v = verticalSpeed * Input.GetAxis("Mouse Y");





        _cinemachineTargetYaw += h;
        _cinemachineTargetPitch += v;


        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch,
            _cinemachineTargetYaw, 0.0f);

    }

    public void EnableMovement()
    {
        _animator.SetBool("CanMove", true);
    }
    public void DisableMovement()
    {
        _animator.SetBool("CanMove", false);
    }
    public void EnableKick()
    {
        _animator.SetBool("Kick", true);
    }
    public void EnablePunch()
    {
        _animator.SetBool("Punch", true);
    }
    public void DisableKick()
    {
        _animator.SetBool("Kick", false);
    }
    public void DisablePunch()
    {
        _animator.SetBool("Punch", false);
    }
    private void Move()
    {
        Vector2 moveV = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (!_animator.GetBool("CanMove"))
        {
            moveV = Vector2.zero;
        }

        if (moveV.magnitude < 0.2f)
        {
            _timeMoving = 0f;
        }
        else
        {
            _timeMoving += Time.deltaTime;
            if (_timeMoving > 1)
                _timeMoving = 1f;
        }
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? SprintSpeed : MoveSpeed;

        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is no input, set the target speed to 0
        if (moveV == Vector2.zero) targetSpeed = 0.0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed,
                _timeMoving);

            // round speed to 3 decimal places
            _speed += Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }
        //  Debug.Log(currentHorizontalSpeed + " : " + targetSpeed);
        //  Debug.Log("_timeMoving" + " : " + _timeMoving);
        //   Debug.Log("_timeMoving" + " : " + _timeMoving);

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // normalise input direction
        Vector3 inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical")).normalized;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (moveV != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                RotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }


        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        // move the player
        //targetDirection.normalized * (_speed * Time.deltaTime) +
        _controller.Move(
                         new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetFloat("Speed", _animationBlend);
        }
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool("Jump", false);
                _animator.SetBool("JumpF", false);
                _animator.SetBool("FreeFall", false);
            }

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (Input.GetKey("space") && _jumpTimeoutDelta <= 0.0f && !_animator.GetBool("Punch") && !_animator.GetBool("Kick"))
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                // update animator if using character
                if (_hasAnimator)
                {
                    if (_animationBlend > 1.5)
                    {
                        StartCoroutine(Delay(0.1f));

                        _animator.SetBool("JumpF", true);

                    }
                    else
                    {
                        StartCoroutine(Delay(1.2f));

                        _animator.SetBool("Jump", true);

                    }
                }



            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool("FreeFall", true);
                }
            }


        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private IEnumerator Delay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);


    }
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            _audio.time = 0.0f;
            _audio.clip = CoinCollected;
            _audio.Play();
            Destroy(other.gameObject);
            _nCoins++;
            _textCoin.text = "Coins " + _nCoins.ToString();
        }

        if (other.CompareTag("PS5")) {
            Victory.active = true;
            StartCoroutine(MenuLoad());
        }

    }
    private IEnumerator MenuLoad() {
        var enemies = GameObject.FindGameObjectsWithTag("enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("menu");

    }


}
