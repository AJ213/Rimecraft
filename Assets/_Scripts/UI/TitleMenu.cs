using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuObject = null;
    [SerializeField] private GameObject settingsObject = null;

    [Header("Main Menu UI Elements")]
    [SerializeField] private TextMeshProUGUI seedField = null;

    [Header("Settings UI Elements")]
    [SerializeField] private Slider viewDistanceSlider = null;

    [SerializeField] private TextMeshProUGUI viewDistanceText = null;
    [SerializeField] private Slider mouseSlider = null;
    [SerializeField] private TextMeshProUGUI mouseText = null;

    private Settings settings;

    private void Awake()
    {
        if (!File.Exists(Application.dataPath + "/settings.cfg"))
        {
            Debug.Log("No settings file found, creating new one.");
            settings = new Settings();
            string jsonExport = JsonUtility.ToJson(settings);
            File.WriteAllText(Application.dataPath + "/settings.cfg", jsonExport);
        }
        else
        {
            Debug.Log("Loading settings.");
            string jsonImport = File.ReadAllText(Application.dataPath + "/settings.cfg");
            settings = JsonUtility.FromJson<Settings>(jsonImport);
        }
    }

    public void EnterSettings()
    {
        viewDistanceSlider.value = settings.viewDistance;
        UpdateViewDstSlider();
        mouseSlider.value = settings.mouseSensitivity;
        UpdateMouseSlider();

        mainMenuObject.SetActive(false);
        settingsObject.SetActive(true);
    }

    public void LeaveSettings()
    {
        settings.viewDistance = Mathf.FloorToInt(viewDistanceSlider.value);
        settings.mouseSensitivity = mouseSlider.value;

        string jsonExport = JsonUtility.ToJson(settings);
        File.WriteAllText(Application.dataPath + "/settings.cfg", jsonExport);

        mainMenuObject.SetActive(true);
        settingsObject.SetActive(false);
    }

    public void StartGame()
    {
        VoxelData.seed = Mathf.Abs(seedField.text.GetHashCode()) / 13333;
        SceneManager.LoadScene("InGame", LoadSceneMode.Single);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void UpdateViewDstSlider()
    {
        viewDistanceText.text = "View Distance: " + viewDistanceSlider.value;
    }

    public void UpdateMouseSlider()
    {
        mouseText.text = "Mouse Sensitivity: " + mouseSlider.value.ToString("F1");
    }
}