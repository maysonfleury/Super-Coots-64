using UnityEngine;
using System.Collections;
using Cinemachine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        //public Transform debugTransform;

        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 4.5f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 7.5f;

        [Tooltip("Crouching speed of the character in m/s")]
        public float CrouchSpeed = 2.25f;

        [Tooltip("Rolling speed of the character in m/s")]
        public float RollSpeed = 5f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

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

        [Tooltip("Time required to pass between dodges.")]
        public float DodgeTimeout = 2f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Mouse")]

        [Tooltip("Mouse Sensitivity")]
        public float Sensitivity = 1f;

        [Tooltip("Crosshair Reference")]
        public GameObject Crosshair;

        [Header("Layer Masks")]

        [Tooltip("Which objects should be shootable")]
        [SerializeField] private LayerMask aimColliderMask = new LayerMask();

        [Tooltip("Mask to ignore Player layer")]
        [SerializeField] private LayerMask playerLayerMask = new LayerMask();


        [Header("Combat")]

        [Tooltip("Bullet Prefab")]
        [SerializeField] private Transform bulletPrefab;

        [Tooltip("Bullet Spawn Position")]
        [SerializeField] private Transform bulletSpawnPos;

        [Tooltip("Current Item Held")]
        [SerializeField] private GameObject heldPickup;

        [Tooltip("Player Mesh Renderer")]
        [SerializeField] private SkinnedMeshRenderer playerMesh;

        [Tooltip("How Many Hits the Player can take")]
        [SerializeField] private int HitsRemaining;

        [Tooltip("Triggers Game Over flag when Health hits 0")]
        public bool GameOver;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("Which style of Camera is currently being used (Exploration or Combat)")]
        public CameraStyle currentCamStyle;
        public enum CameraStyle
        {
            Exploration,
            Combat
        }

        [Tooltip("The Camera used when running around")]
        [SerializeField] private CinemachineVirtualCamera ExplorationCam;

        [Tooltip("The Camera used when in combat (aiming down sight)")]
        [SerializeField] private CinemachineVirtualCamera CombatCam;

        [Tooltip("Which direction the Camera should look during Combat")]
        public Transform CombatLookAt;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

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

        // useful bools
        private bool canShoot;
        private bool isCrouching;
        private bool isHurt;
        public bool isRolling;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;
        private float _dodgeTimeoutDelta;

        // animation IDs
        private int _animSpeed;
        private int _animGrounded;
        private int _animJump;
        private int _animFreeFall;
        private int _animMotionSpeed;
        private int _animCrouch;
        private int _animDodgeRoll;
        private int _animAim;
        private int _animFall;

        // user interface
        private PlayerHealthVisual _healthVisual;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
            _dodgeTimeoutDelta = DodgeTimeout;

            HitsRemaining = 8;
            _healthVisual = GetComponent<PlayerHealthVisual>();
            _healthVisual.SetCurrentHealth(8);

            GameOver = false;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            if(!GameOver)
            {
                JumpAndGravity();
                GroundedCheck();
                DodgeRoll();
                if(!isHurt)
                {
                    if(!isRolling)
                    {
                        Crouch();
                        Aim();
                        Shoot();
                    }
                    Move();
                }
            }
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animSpeed = Animator.StringToHash("Speed");
            _animGrounded = Animator.StringToHash("Grounded");
            _animJump = Animator.StringToHash("Jump");
            _animFreeFall = Animator.StringToHash("FreeFall");
            _animMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animCrouch = Animator.StringToHash("Crouch");
            _animDodgeRoll = Animator.StringToHash("DodgeRoll");
            _animAim = Animator.StringToHash("Aim");
            _animFall = Animator.StringToHash("PlayerHurt");
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
                _animator.SetBool(_animGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier * Sensitivity;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier * Sensitivity;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            // set target speed based on move speed, sprint speed and chrouch speed
            float targetSpeed = MoveSpeed;
            if(_input.sprint){ targetSpeed = SprintSpeed; }
            if(_input.ads){ targetSpeed = MoveSpeed - 1; }
            if(_input.crouch){ targetSpeed = CrouchSpeed; }

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            if(currentCamStyle == CameraStyle.Combat && isRolling)
            {
                targetDirection = transform.forward;
                _speed = RollSpeed;
            }

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // Changes how the Character rotates depending on Camera Style
            if(currentCamStyle == CameraStyle.Exploration)
            {
                // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
                // if there is a move input rotate player when the player is moving
                if (_input.move != Vector2.zero)
                {
                    _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
                    float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

                    // Rotate to face input direction relative to camera position
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }
            }
            else if(currentCamStyle == CameraStyle.Combat)
            {
                // Rotate to face towards crosshair's world position
                Vector3 mouseWorldPosition = Vector3.zero;
                Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
                Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
                if (Physics.Raycast(ray, out RaycastHit rcHit, 999f, aimColliderMask))
                {
                    //debugTransform.position = rcHit.point;
                    mouseWorldPosition = rcHit.point;
                }
                Vector3 worldAimTarget = mouseWorldPosition;
                worldAimTarget.y = transform.position.y;
                Vector3 aimDirection = (worldAimTarget - transform.position).normalized;
                transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);

                if (_input.move != Vector2.zero)
                {
                    _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
                    float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
                }
            }

            // update animator
            if (_hasAnimator)
            {
                _animator.SetFloat(_animSpeed, _animationBlend);
                _animator.SetFloat(_animMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator
                if (_hasAnimator)
                {
                    _animator.SetBool(_animJump, false);
                    _animator.SetBool(_animFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // Checks to see whether player jump
                    var cantJump = Physics.Raycast(transform.position, Vector3.up, 2f, playerLayerMask);
                    if(!cantJump && !isHurt && !isRolling)
                    {
                        // the square root of H * -2 * G = how much velocity needed to reach desired height
                        _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                        // update animator if using character
                        if (_hasAnimator)
                        {
                            _animator.SetBool(_animJump, true);
                        }
                    } else {
                        _input.jump = false;
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
                    // update animator
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private void Crouch()
        {
            if (_input.crouch)
            {
                if (_hasAnimator)
                {
                    // Change character height and hitbox when crouching
                    _animator.SetBool(_animCrouch, true);
                    _controller.height = 1.35f;
                    _controller.center = new Vector3(0, 0.6975f, 0.15f);
                    isCrouching = true;

                    // Change camera heights
                    var camExplore = ExplorationCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
                    camExplore.ShoulderOffset = new Vector3(1, -0.15f, 0);
                    var camCombat = CombatCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
                    camCombat.ShoulderOffset = new Vector3(0.7f, -0.25f, 0);
                }
            }
            else
            {
                if(isCrouching)
                {
                    // Checks to see whether player can stand up
                    var cantStandUp = Physics.Raycast(transform.position, Vector3.up, 2f, playerLayerMask);
                    if(!cantStandUp)
                    {
                        if (_hasAnimator)
                        {
                            // Change character height and hitbox when standing up
                            _animator.SetBool(_animCrouch, false);
                            _controller.height = 1.8f;
                            _controller.center = new Vector3(0, 0.93f, 0);
                            isCrouching = false;

                            // Change camera heights
                            var camExplore = ExplorationCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
                            camExplore.ShoulderOffset = new Vector3(1, 0.15f, 0);
                            var camCombat = CombatCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
                            camCombat.ShoulderOffset = new Vector3(0.7f, 0.25f, 0);
                        }
                    }
                }
            }
        }

        private void DodgeRoll()
        {
            if (Grounded && !isHurt)
            {
                // can only roll if cooldown is complete and if player is moving
                if (_input.dodgeroll && _dodgeTimeoutDelta <= 0.0f && _input.move != Vector2.zero)
                {
                    isRolling = true;

                    // update animator and move player forward
                    if (_hasAnimator)
                    {
                        _animator.SetLayerWeight(3, 1f);
                        _animator.SetTrigger(_animDodgeRoll);
                    }

                    // Player gets iframes after small delay
                    StartCoroutine(DelayIFrames(0.2f));

                    // reset the dodge timeout timer
                    _dodgeTimeoutDelta = DodgeTimeout;
                }
                else 
                {
                    _input.dodgeroll = false;
                }

                // dodge timeout
                if (_dodgeTimeoutDelta >= 0.0f)
                {
                    _dodgeTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // if we are not grounded, do not roll
                _input.dodgeroll = false;
            }
        }

        private void Aim()
        {
            if (_input.ads && Grounded)
            {
                if (_hasAnimator)
                {
                    _animator.SetBool(_animAim, true);
                    _animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));
                }

                // Change Camera
                currentCamStyle = CameraStyle.Combat;
                CombatCam.enabled = true;

                // Change Sensitivity & activate Crosshair
                Sensitivity = 0.55f;
                StartCoroutine(ActivateCrosshair());
                canShoot = true;
            }
            else
            {
                if (_hasAnimator)
                {
                    _animator.SetBool(_animAim, false);
                    _animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
                }

                // Change Camera
                currentCamStyle = CameraStyle.Exploration;
                CombatCam.enabled = false;

                // Change Sensitivity & disable Crosshair
                Sensitivity = 1f;
                Crosshair.SetActive(false);
                canShoot = false;
            }
        }

        private void Shoot()
        {
            if (_input.shoot)
            {
                // Removes Shoot input if the player is not in position to shoot
                if(!canShoot)
                {
                    _input.shoot = false;
                } 
                else 
                {
                    // Gets world coords of target
                    Vector3 mouseWorldPosition = Vector3.zero;
                    Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
                    Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
                    if (Physics.Raycast(ray, out RaycastHit rcHit, 999f, aimColliderMask))
                    {
                        mouseWorldPosition = rcHit.point;
                    }

                    // Shoots held item
                    Vector3 aimDir = (mouseWorldPosition - bulletSpawnPos.position).normalized;
                    if(heldPickup != null)
                    {
                        heldPickup.gameObject.GetComponent<Pickup>().Launch(Quaternion.LookRotation(aimDir, Vector3.up));
                        heldPickup = null;
                    } else {
                        //Instantiate(bulletPrefab, bulletSpawnPos.position, Quaternion.LookRotation(aimDir, Vector3.up));
                    }
                    _input.shoot = false;
                }
            }

            if(heldPickup != null)
            {
                heldPickup.transform.position = bulletSpawnPos.position;
                heldPickup.transform.forward = transform.forward;
            }
        }

        private IEnumerator ActivateCrosshair()
        {
            if(!Crosshair.activeSelf)
            {
                yield return new WaitForSeconds(0.2f);
                Crosshair.SetActive(true);
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<HurtPlayer>() != null)
            {
                if(HitsRemaining == 0)
                {
                    // TODO: Game Over
                    GameOver = true;
                }
                else
                {
                    HitsRemaining--;
                    _animator.SetLayerWeight(2, 1f);
                    _animator.SetTrigger(_animFall);
                    _controller.detectCollisions = false;
                    isHurt = true;

                    _healthVisual.TakeDamage(3);

                    // Leave Combat Mode
                    currentCamStyle = CameraStyle.Exploration;
                    CombatCam.enabled = false;
                    Sensitivity = 1f;
                    Crosshair.SetActive(false);
                }
            }

            if (other.GetComponent<Pickup>() != null)
            {
                heldPickup = other.gameObject;
                other.gameObject.transform.localScale = other.gameObject.transform.localScale / 4.0f;
            }
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f && !isHurt && !isRolling)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        private void OnDamageTaken(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                // TODO: Play oof sound
            }
        }

        private void OnFall(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                // TODO: Play fall sound, below is placeholder
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
                StartCoroutine(iFrameAnimation(8, 1.75f));
            }
        }

        private void OnRoll(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                // TODO: Play fall sound, below is placeholder
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
                //StartCoroutine(iFrameAnimation(3, 0.01f));
            }
        }

        private void OnGetUp(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                _animator.SetLayerWeight(2, 0f);
                isHurt = false;
                StartCoroutine(ExtendIFrames(0.85f));
            }
        }

        private void OnRollGetUp(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                _animator.SetLayerWeight(3, 0f);
                isRolling = false;
                StartCoroutine(ExtendIFrames(0.01f));
            }
        }

        // Flickers the Player model (count) times, after a delay
        private IEnumerator iFrameAnimation(int count, float delay)
        {
            yield return new WaitForSeconds(delay);

            while(count > 0)
            {
                yield return new WaitForSeconds(0.1f);
                playerMesh.enabled = false;
                yield return new WaitForSeconds(0.05f);
                playerMesh.enabled = true;
                count--;
            }
        }

        // Makes the Player invincible for a longer amount of time
        private IEnumerator ExtendIFrames(float delay)
        {
            yield return new WaitForSeconds(delay);
            _controller.detectCollisions = true;
        }

        // Delays when the invincibility frames are active
        private IEnumerator DelayIFrames(float delay)
        {
            yield return new WaitForSeconds(delay);
            _controller.detectCollisions = false;
        }
    }
}