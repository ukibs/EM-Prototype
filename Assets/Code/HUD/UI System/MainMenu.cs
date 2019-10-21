﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public string firstLevel;

    public GameObject optionsScreen;

    public GameObject levelMenu;
    public Text titleDescription;
    public Text description;

    public GameObject loadingScreen, loadingIcon;
    public Text loadingText;

    void Start()
    {
        //
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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