using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CootsAI : MonoBehaviour
{
    [SerializeField] private float WalkSpeed;
    [SerializeField] private Collider CombatTriggerCollider;
    [SerializeField] private Transform _target;
    [SerializeField] private bool isAttacking;
    public Collider[] _hurtBoxes;

    public AudioClip[] CootsAudioClips;
    [Range(0, 1)] public float CootsAudioVolume = 0.5f;

    private Animator _animator;
    private bool _hasAnimator;
    private int _animSlam;
    private int _animScratch;
    private int _animSleep;
    private int _animWalk;

    private float _rotationVelocity;

    // Start is called before the first frame update
    void Start()
    {
        _hasAnimator = TryGetComponent(out _animator);
        AssignAnimationIDs();

        foreach(Collider col in _hurtBoxes)
        {
            col.enabled = false;
        }
    }

    private void AssignAnimationIDs()
    {
        _animSlam = Animator.StringToHash("Slam");
        _animScratch = Animator.StringToHash("Scratch");
        _animSleep = Animator.StringToHash("isSleeping");
        _animWalk = Animator.StringToHash("isWalking");
    }

    // Update is called once per frame
    void Update()
    {
        if(!_target)
        {
            CombatTriggerCollider.enabled = true;
        } else {
            Walk();
        }
    }

    // Called when Player enters Coots' Aggro Range
    public void StartCombat()
    {
        // Wake Up
        Debug.Log("Combat Started");
        _animator.SetBool(_animSleep, false);
    }

    // Called when a Pickup Toy gets close to Coots
    public void DistractCoots()
    {
        //Debug.Log("Coots is distracted!");

        // Check Distance between Coots and Toy
        float dist = Vector3.Distance(_target.position, transform.position);
        // If too far away, target player again
        if(dist > 10f)
        {
            Debug.Log("Coots is no longer interested in the toy...");
            CombatTriggerCollider.enabled = true;
            SetTarget(null);
        }
    }

    // Called when either the Player, a Toy, or a Tower are directly in front of Coots
    public void Attack()
    {
        Debug.Log("Coots Attacks!");
        if (!isAttacking)
        {
            isAttacking = true;

            var num = Random.Range(0, 2);   // Range(inclusive, exclusive)
            if(num == 0)
            {
                _animator.SetTrigger(_animSlam);
            }
            else
            {
                _animator.SetTrigger(_animScratch);
            }
        }
    }

    public void BodySlam()
    {
        StartCoroutine(BodySlamRoutine());
    }

    private IEnumerator BodySlamRoutine()
    {
        if (!isAttacking)
        {
            isAttacking = true;

            _animator.SetBool(_animSleep, true);
            yield return new WaitForSeconds(1f);
            _animator.SetBool(_animSleep, false);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        _target = newTarget;
    }

    // Makes Coots walk towards a target object
    private void Walk()
    {
        // Check Distance between Coots and Target and stop walking if too close
        float dist = Vector3.Distance(_target.position, transform.position);
        if(dist > 2f && !isAttacking)
        {
            _animator.SetBool(_animWalk, true);

            Vector3 difference = _target.position - transform.position;
            float targetRotation = Mathf.Atan2(difference.x, difference.z) * Mathf.Rad2Deg;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref _rotationVelocity, 0.12f);
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

            Vector3 targetPos = new Vector3(_target.position.x, 11.032f, _target.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, WalkSpeed * Time.deltaTime);
            //transform.Translate(Vector3.forward * WalkSpeed * Time.deltaTime);
        }
        else {
            Debug.Log("Coots can hit Lud!!");
            _animator.SetBool(_animWalk, false);
        }
    }

    private void OnSlam(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            PlayClipAt(CootsAudioClips[0], transform.position, CootsAudioVolume);
        }
    }

    private void OnScratchStart(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            PlayClipAt(CootsAudioClips[1], transform.position, CootsAudioVolume);
        }
    }

    private void OnScratch(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            var num = Random.Range(2, 4);   // Either [2] or [3]
            PlayClipAt(CootsAudioClips[num], transform.position, CootsAudioVolume);
        }
    }

    private void OnSmallStep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            var num = Random.Range(4, 6);   // Either [4] or [5]
            PlayClipAt(CootsAudioClips[num], transform.position, CootsAudioVolume);
        }
    }

    private void OnBigStep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            var num = Random.Range(6, 8);   // Either [6] or [7]
            PlayClipAt(CootsAudioClips[num], transform.position, CootsAudioVolume);
        }
    }

    private void OnAttackStarted(AnimationEvent animationEvent)
    {
        foreach(Collider col in _hurtBoxes)
        {
            col.enabled = true;
        }
    }

    private void OnAttackFinished(AnimationEvent animationEvent)
    {
        isAttacking = false;
        foreach(Collider col in _hurtBoxes)
        {
            col.enabled = false;
        }
    }

    AudioSource PlayClipAt(AudioClip clip, Vector3 pos, float vol)
    {
        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = pos;
        AudioSource aSource = tempGO.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.volume = vol;
        aSource.rolloffMode = AudioRolloffMode.Linear;
        aSource.maxDistance = 35f;
        aSource.spatialBlend = 1.0f;
        aSource.Play(); // start the sound
        Destroy(tempGO, clip.length); // destroy object after clip duration
        return aSource; // return the AudioSource reference
    }
}
