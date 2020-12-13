using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Animator))]
public class SceneChanger : MonoBehaviour
{
    public static int indexToLoad = 0;
    [SerializeField] private AudioMixer mixer = default;

    private void Start()
    {
        indexToLoad = SceneManager.GetActiveScene().buildIndex;
        StartCoroutine(FadeMixerGroup.StartFade(mixer, "Master Volume", 0.2f, 1f));
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void FadeToScene(int levelIndex)
    {
        indexToLoad = levelIndex;
        StartCoroutine(FadeMixerGroup.StartFade(mixer, "Master Volume", 0.99f, 0f));
        GetComponent<Animator>().SetTrigger("FadeOut");
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                FadeToScene(-1);
            }
        }
    }

    public static void LoadScene()
    {
        if (indexToLoad == -1)
        {
            Application.Quit();
            return;
        }

        SceneManager.LoadScene(indexToLoad);
    }
}