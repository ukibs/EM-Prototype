using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenu : MonoBehaviour
{
    public string firstLevel;

    public GameObject optionsScreen;

    public GameObject levelMenu;
    public Text titleDescription;
    public Text description;

    public GameObject loadingScreen, loadingIcon;
    public Text loadingText;

    public VideoClip[] videoClips;
    public float maxTimeToVideo = 30;

    private VideoPlayer videoPlayer;
    private float currentTimeToVideo;
    private int currentVideoIndex = 0;

    void Start()
    {
        //
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        //
        if (videoClips.Length > 0)
        {
            videoPlayer = GetComponent<VideoPlayer>();
            videoPlayer.Prepare();
            videoPlayer.clip = videoClips[0];
        }
        
    }

    private void Update()
    {
        if (videoClips.Length > 0)
            UpdateVideoStuff();
        //
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            GameManager.instance.GameProgression = 0;
            Debug.Log("Progression reset");
        }
    }

    private void UpdateVideoStuff()
    {
        //
        if (Input.anyKeyDown)
        {
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
                //
                currentVideoIndex++;
                if (currentVideoIndex == videoClips.Length) currentVideoIndex = 0;
                videoPlayer.clip = videoClips[currentVideoIndex];
            }
            else
            {
                videoPlayer.Play();
            }
        }
        //
        if (!videoPlayer.isPlaying)
        {
            currentTimeToVideo += Time.deltaTime;
            //
            if (currentTimeToVideo >= maxTimeToVideo)
            {
                Debug.Log("Playing video");
                videoPlayer.Play();
                currentTimeToVideo = 0;
                currentVideoIndex++;
            }
        }
    }

    public void StartGame(string levelName)
    {
        StartCoroutine(LoadLevel(levelName));
    }

    public void OpenOptions()
    {
        optionsScreen.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsScreen.SetActive(false);
    }

    public void OpenLevelMenu()
    {
        //levelMenu.SetActive(true);
        SceneManager.LoadSceneAsync("Map");
    }

    public void CloseLevelMenu()
    {
        levelMenu.SetActive(false);
    }

    public void QuitGame()
    {
        //
        if(videoPlayer != null)
            videoPlayer.Stop();
        //
        Application.Quit();
    }
    public void TitleDescription (string text)
    {
        titleDescription.text = text;
    }

    public void Description (string text)
    {
        description.text = text;
    }

    public IEnumerator LoadLevel(string level)
    {
        levelMenu.SetActive(true);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(level);

        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= .9f)
            {
                loadingText.text = "Press any key to continue";
                loadingIcon.SetActive(false);

                if (Input.anyKeyDown)
                {
                    asyncLoad.allowSceneActivation = true;

                    Time.timeScale = 1f;
                }
            }

            yield return null;
        }
    }
}
