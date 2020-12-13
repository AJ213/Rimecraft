using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Animator))]
public class SceneChanger : MonoBehaviour
{
    private int indexToLoad = 0;
    [SerializeField] AudioMixer mixer = default;
    [SerializeField] GameObject exitMenu = default;
    private void Start()
    {
        StartCoroutine(FadeMixerGroup.StartFade(mixer, "Main", 0.2f, 1f));
    }
    public void FadeToScene(int levelIndex)
    {
        indexToLoad = levelIndex;
        StartCoroutine(FadeMixerGroup.StartFade(mixer, "Main", 0.99f, 0f));
        GetComponent<Animator>().SetTrigger("FadeOut");
    }

    private void Update()
    {
        if(SceneManager.GetActiveScene().buildIndex == 1)
        {
            if(Input.GetKeyDown(KeyCode.Escape) && !exitMenu.activeSelf)
            {
                exitMenu.SetActive(true);
                GameObject.FindGameObjectWithTag("GameController").GetComponent<AudioManager>().Play("ButtonPress");
                Cursor.lockState = CursorLockMode.None;
            }
            else if(Input.GetKeyDown(KeyCode.Escape) && exitMenu.activeSelf)
            {
                CloseMenu();
            }
        }
        if(SceneManager.GetActiveScene().buildIndex == 0)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                FadeToScene(-1);
            }
        }
    }

    public void CloseMenu()
    {
        exitMenu.SetActive(false);
        GameObject.FindGameObjectWithTag("GameController").GetComponent<AudioManager>().Play("ButtonPress");
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void LoadScene()
    {
        if (indexToLoad == -1)
        {
            Application.Quit();
            return;
        }
        
        SceneManager.LoadScene(indexToLoad);
    }
}
