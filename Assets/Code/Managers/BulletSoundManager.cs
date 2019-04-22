using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Manager para los sonidos de las balas, que se destruyen al impactar
/// </summary>
public class BulletSoundManager : MonoBehaviour
{
    //
    public GameObject audioObjectPrefab;
    //
    private List<GameObject> audioObjects;
    // Start is called before the first frame update
    void Start()
    {
        //audioObjects = new List<GameObject>(10);
    }

    // Update is called once per frame
    void Update()
    {
        //
        //for(int i = 0; i < audioObjects.Count; i++)
        //{
        //    GameObject nextObject = audioObjects[i];
        //    AudioSource audioSource = nextObject.GetComponent<AudioSource>();
        //    if (!audioSource.isPlaying)
        //    {
        //        Destroy(nextObject);
        //        audioObjects.Remove(nextObject);
        //    }
        //}
    }

    public void CreateAudioObject(AudioClip clip, Vector3 position)
    {
        //
        if (clip == null)
            return;
        //
        GameObject newAudioObject = Instantiate(audioObjectPrefab, position, Quaternion.identity);
        AudioSource audioSource = newAudioObject.GetComponent<AudioSource>();
        audioSource.clip = clip;
        Destroy(newAudioObject,clip.length);
        audioSource.Play();
    }
}
