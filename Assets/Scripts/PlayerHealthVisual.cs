using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthVisual : MonoBehaviour
{
 
    public Sprite[] healthSprites; // SET SIZE TO 9 AND MAKE FIRST ONE EMPTY AND LAST FULL
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform m_Camera;
    [SerializeField] private GameObject fullSprite;
    public AudioClip[] HealthAudioClips;

    private int _current_health; // can get from somewhere else


    void Start()
    {
        spriteRenderer.sprite = healthSprites[8];
        fullSprite.SetActive(false);
    }

    void LateUpdate()
    {
        fullSprite.transform.LookAt(m_Camera);
    }

    public void TakeDamage(int damage)
    {
        fullSprite.SetActive(true);
        StartCoroutine(HealthLossAnimation(damage));
    }

    public void SetCurrentHealth(int health)
    {
        _current_health = health;
    }

    IEnumerator HealthLossAnimation(int damage)
    {
        int pending_damage = damage;
        spriteRenderer.transform.localScale = Vector3.Lerp(spriteRenderer.transform.localScale, new Vector3(2f, 2f, 2f), Time.deltaTime * 5f);

        //yield return new WaitForSeconds(0.3f);

        while (pending_damage > 0)
        {
            if(_current_health > 0)
            {
                yield return new WaitForSeconds(0.2f);
                spriteRenderer.sprite = healthSprites[_current_health - 1];
                PlayClipAt(HealthAudioClips[_current_health], transform.position, 0.5f);
            }
            _current_health -= 1;
            pending_damage -= 1;
        }

        if(_current_health <= 0) PlayClipAt(HealthAudioClips[0], transform.position, 0.5f);

        spriteRenderer.transform.localScale = Vector3.Lerp(spriteRenderer.transform.localScale, new Vector3(1f,1f,1f), Time.deltaTime * 5f);

        for(int i = 0; i < 4; i++)
        {
            fullSprite.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            fullSprite.SetActive(false);
            yield return new WaitForSeconds(0.05f);
        }
        fullSprite.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        fullSprite.SetActive(false);
    }

    AudioSource PlayClipAt(AudioClip clip, Vector3 pos, float vol)
    {
        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = pos;
        AudioSource aSource = tempGO.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.volume = vol;
        aSource.rolloffMode = AudioRolloffMode.Linear;
        aSource.maxDistance = 45f;
        aSource.spatialBlend = 1.0f;
        aSource.Play(); // start the sound
        Destroy(tempGO, clip.length); // destroy object after clip duration
        return aSource; // return the AudioSource reference
    }
}