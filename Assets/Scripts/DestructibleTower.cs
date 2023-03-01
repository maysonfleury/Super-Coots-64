using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleTower : MonoBehaviour
{
    [SerializeField] private CootsAI _cootsAI;
    [SerializeField] private GameObject _realTower;
    [SerializeField] private GameObject _brokenTowerPrefab;
    [SerializeField] private GameObject _towerPickupPrefab;
    [SerializeField] private Collider _towerTargetTrigger;
    [SerializeField] private Collider _towerAttackTrigger;
    [SerializeField] private Transform AttackPoint;
    [SerializeField] private Transform RespawnPoint;
    [SerializeField] private Vector3 RealPos;
    [SerializeField] private Transform FakePos;
    [SerializeField] private Transform PickupPos;
    [SerializeField] private int HitsRemaining = 3;

    [SerializeField] private GameObject clonedTower;
    [SerializeField] private GameObject clonedPickup;

    private bool isRespawning;
    private bool attackCooldown;

    public AudioClip[] TowerAudioClips;
    [Range(0, 1)] public float TowerAudioVolume = 0.5f;
    
    private void Start()
    {
        RealPos = _realTower.transform.position;
        clonedPickup = Instantiate(_towerPickupPrefab, PickupPos.position, PickupPos.rotation);
    }

    private void Update()
    {
        if(isRespawning)
        {
            UpYouGo();
        }
    }

    public Transform GetAttackPoint()
    {
        return AttackPoint;
    }

    public void AttackTower()
    {
        _towerTargetTrigger.enabled = false;
        if(!attackCooldown)
        {
            StartCoroutine(AttackTowerRoutine());
        }
    }

    private IEnumerator AttackTowerRoutine()
    {
        attackCooldown = true;
        yield return new WaitForSeconds(0.6f);
        PlayClipAt(TowerAudioClips[1], RealPos, TowerAudioVolume);

        HitsRemaining--;
        if(HitsRemaining == 0)
        {
            if(clonedPickup)
            {
                if(!clonedPickup.gameObject.GetComponent<Pickup>().GetAttachState())
                {
                    Destroy(clonedPickup);
                }
            }
            _realTower.SetActive(false);
            _towerAttackTrigger.enabled = false;

            clonedTower = Instantiate(_brokenTowerPrefab, FakePos.position, FakePos.rotation);
            
            var num = Random.Range(2, 7);   // Between [2] and [6]
            PlayClipAt(TowerAudioClips[num], FakePos.position, TowerAudioVolume);
            PlayClipAt(TowerAudioClips[0], FakePos.position, TowerAudioVolume);

            StartCoroutine(TowerRespawn());
        }

        yield return new WaitForSeconds(1.65f);
        attackCooldown = false;
    }

    private IEnumerator TowerRespawn()
    {
        _realTower.transform.position = RespawnPoint.position;
        yield return new WaitForSeconds(2f);
        _cootsAI.SetTarget(null);
        yield return new WaitForSeconds(3f);
        if(clonedTower)
        {
            StartCoroutine(iFrameAnimation(5));
        }

        yield return new WaitForSeconds(25f);
        isRespawning = true;
        _realTower.SetActive(true);
        _realTower.GetComponent<MeshCollider>().enabled = false;
        HitsRemaining = 3;
    }

    private void UpYouGo()
    {
        _realTower.transform.position = _realTower.transform.position + new Vector3(0, 2.5f * Time.deltaTime, 0);
        if (_realTower.transform.position.y >= RealPos.y)
        {
            _towerTargetTrigger.enabled = true;
            _towerAttackTrigger.enabled = true;
            _realTower.GetComponent<MeshCollider>().enabled = true;
            clonedPickup = Instantiate(_towerPickupPrefab, PickupPos.position, PickupPos.rotation);
            isRespawning = false;
        }
    }

    // Flickers the Tower model (count) times, after a delay
    private IEnumerator iFrameAnimation(int count)
    {
        while(count > 0)
        {
            yield return new WaitForSeconds(0.1f);
            clonedTower.SetActive(false);
            yield return new WaitForSeconds(0.05f);
            clonedTower.SetActive(true);
            count--;
        }
        Destroy(clonedTower);
    }

    AudioSource PlayClipAt(AudioClip clip, Vector3 pos, float vol)
    {
        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = pos;
        AudioSource aSource = tempGO.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.volume = vol;
        aSource.rolloffMode = AudioRolloffMode.Linear;
        aSource.maxDistance = 50f;
        aSource.spatialBlend = 1.0f;
        aSource.Play(); // start the sound
        Destroy(tempGO, clip.length); // destroy object after clip duration
        return aSource; // return the AudioSource reference
    }
}
