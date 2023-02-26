using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CallingSlime : MonoBehaviour
{
 
    public Sprite[] callSprites; // SET SIZE TO 4 
    [SerializeField] private GameObject SliderObject;
    private Slider slider;
    [SerializeField] private Image activeSprite;
    [SerializeField] private Image WinScreen;

    public AudioClip[] CallAudioClips;
    [Range(0, 1)] public float CallAudioVolume = 0.5f;

    [SerializeField] private GameObject SlimePhone;
    [SerializeField] private GameObject BlankPhone;

    private int call_counter = 0; // for the text
    private int current_sprite = 0; // also for the text

    private bool isCalling;

    private AudioSource audio;

    [SerializeField] private Collider _callArea;

    void Start()
    {
        _callArea = gameObject.GetComponent<Collider>();
        activeSprite.sprite = callSprites[0];
        activeSprite.enabled = false;
        WinScreen.enabled = false;
        slider = SliderObject.gameObject.GetComponent<Slider>();
        SliderObject.SetActive(false);
        SlimePhone.SetActive(false);
        BlankPhone.SetActive(true);
    }

    public void CallSlime()
    {
        StartCoroutine(CallSlimeAnimation());
        audio = PlayClipAt(CallAudioClips[0], transform.position, CallAudioVolume);
        SlimePhone.SetActive(true);
        BlankPhone.SetActive(false);
    }

    public void EndCall()
    {
        current_sprite = 0;
        activeSprite.sprite = callSprites[current_sprite];

        activeSprite.enabled = false;

        call_counter = 0;
        slider.value = 0.0f;
        SliderObject.SetActive(false);

        if(audio.gameObject != null) Destroy(audio.gameObject);
        PlayClipAt(CallAudioClips[1], transform.position, CallAudioVolume);

        SlimePhone.SetActive(false);
        BlankPhone.SetActive(true);
    }

    public void CompleteCall()
    {
        current_sprite = 0;
        activeSprite.sprite = callSprites[current_sprite];

        activeSprite.enabled = false;

        call_counter = 0;
        slider.value = 0.0f;
        SliderObject.SetActive(false);

        Destroy(audio.gameObject);
        PlayClipAt(CallAudioClips[2], transform.position, CallAudioVolume);
        WinScreen.enabled = true;
    }

    public void HidePhones()
    {
        SlimePhone.SetActive(false);
        BlankPhone.SetActive(false);
    }

    public void ShowPhones()
    {
        SlimePhone.SetActive(false);
        BlankPhone.SetActive(true);
    }

    void UpdateSlider(float value)
    {
        slider.value = value;
        call_counter += 1;

        if (call_counter == 10)
        {
            current_sprite += 1;

            if (current_sprite > 3)
                current_sprite = 0;

            activeSprite.sprite = callSprites[current_sprite];
            call_counter = 0;
        }
    }

    IEnumerator CallSlimeAnimation()
    {
        activeSprite.enabled = true;
        SliderObject.SetActive(true);

        yield return new WaitForSeconds(0.5f); // play dial sound maybe?
        
        float value = 0f;
        while (value <= 100.0f)
        {
            yield return new WaitForSeconds(0.1f);
            UpdateSlider(value);
            value += 0.95f;
        }         

        if(value >= 100f) CompleteCall();
    }

    public void DisableCallCollider()
    {
        _callArea.enabled = false;
    }

    public void EnableCallCollider()
    {
        _callArea.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isCalling = true;
            CallSlime();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isCalling = false;
            StopCoroutine(CallSlimeAnimation());
            EndCall();
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
        aSource.maxDistance = 45f;
        aSource.spatialBlend = 1.0f;
        aSource.Play(); // start the sound
        Destroy(tempGO, clip.length); // destroy object after clip duration
        return aSource; // return the AudioSource reference
    }
}