using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CootsAI : MonoBehaviour
{
    [SerializeField] private float WalkSpeed;
    [SerializeField] private Collider CombatTriggerCollider;
    [SerializeField] private Transform _target;
    [SerializeField] private bool isAttacking;

    public AudioClip[] CootsAudioClips;
    [Range(0, 1)] public float CootsAudioVolume = 0.5f;

    private Animator _animator;
    private bool _hasAnimator;
    private int _animSlam;
    private int _animScratch;
    private int _animSleep;
    private int _animWalk;

    // Start is called before the first frame update
    void Start()
    {
        _hasAnimator = TryGetComponent(out _animator);
        AssignAnimationIDs();
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
        Walk();
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
        if(dist > 20f)
        {
            Debug.Log("Coots is no longer interested in the toy...");
            CombatTriggerCollider.enabled = true;
        }
    }

    // Called when either the Player, a Toy, or a Tower are directly in front of Coots
    public void Attack()
    {
        //_animator.SetBool(_animWalk, false);
        Debug.Log("Coots Attacks!");

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

    public void SetTarget(Transform newTarget)
    {
        _target = newTarget;
    }

    // Makes Coots walk towards a target object
    private void Walk()
    {
        // Check Distance between Coots and Target
        float dist = Vector3.Distance(_target.position, transform.position);
        // Stop walking if too close
        if(dist < 3f)
        {
            Debug.Log("Coots can hit Lud!!");
            _animator.SetBool(_animWalk, false);
        }
        else {
            _animator.SetBool(_animWalk, true);
            transform.Translate(Vector3.forward * WalkSpeed * Time.deltaTime);
            transform.LookAt(_target);
        }
    }

    private void OnSlam(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            // TODO: Change audio clip
            AudioSource.PlayClipAtPoint(CootsAudioClips[0], transform.TransformPoint(transform.position), CootsAudioVolume);
        }
    }
}
