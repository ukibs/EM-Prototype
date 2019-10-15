using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Manager para los sonido creados por objetos al destruirse
/// </summary>
public class AudioObjectManager : MonoBehaviour
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

    /// <summary>
    /// fsdfdsfs
    /// TODO: Hacer editable alance y demás
    /// TODO: Meter pitch variation
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="position"></param>
    public void CreateAudioObject(AudioClip clip, Vector3 position, float pitchVariation = 0, float maxSoundDistance = 0)
    {
        //
        if (clip == null)
            return;
        //
        GameObject newAudioObject = Instantiate(audioObjectPrefab, position, Quaternion.identity);
        AudioSource audioSource = newAudioObject.GetComponent<AudioSource>();
        audioSource.clip = clip;
        if (pitchVariation != 0.0f)
        {
            audioSource.pitch = UnityEngine.Random.Range(1.0f - pitchVariation, 1.0f + pitchVariation);
        }
        //
        if (maxSoundDistance > 0)
            audioSource.maxDistance = maxSoundDistance;
        //
        Destroy(newAudioObject,clip.length);
        audioSource.Play();
    }
}
