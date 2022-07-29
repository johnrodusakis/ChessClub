using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsGame : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Dropdown resolutionDropDown;

    Resolution[] resolutions;

    [SerializeField] private GameObject video_btn;
    [SerializeField] private GameObject audio_btn;
    [SerializeField] private GameObject video_select;
    [SerializeField] private GameObject audio_select;
    [SerializeField] private GameObject video_content;
    [SerializeField] private GameObject audio_content;


    [SerializeField] private TMPro.TMP_Text graphicText;
    private int graphic_value = 5;

    private void Awake()
    {
        DisplayGraphicText();

        video_btn.SetActive(false);
        video_select.SetActive(true);
        video_content.SetActive(true);

        audio_btn.SetActive(true);
        audio_select.SetActive(false);
        audio_content.SetActive(false);
    }

    private void Start()
    {
        resolutions = Screen.resolutions;
        resolutionDropDown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " (" + resolutions[i].refreshRate + "Hz)";
            options.Add(option);
            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }
        options.Distinct().ToList();
        resolutionDropDown.AddOptions(options);
        resolutionDropDown.value = currentResolutionIndex;
        resolutionDropDown.RefreshShownValue();
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution res = resolutions[resolutionIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    public void SetGraphics(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    public void OnVideoButtonClicked()
    {
        video_btn.SetActive(false);
        audio_select.SetActive(false);
        audio_content.SetActive(false);

        video_select.SetActive(true);
        video_content.SetActive(true);
        audio_btn.SetActive(true);
    }

    public void OnAudioButtonClicked()
    {
        video_select.SetActive(false);
        video_content.SetActive(false);
        audio_btn.SetActive(false);

        audio_select.SetActive(true);
        audio_content.SetActive(true);
        video_btn.SetActive(true);
    }

    private void DisplayGraphicText()
    {
        switch (graphic_value)
        {
            case 0:
                graphicText.text = "Very Low";
                break;
            case 1:
                graphicText.text = "Low";
                break;
            case 2:
                graphicText.text = "Medium";
                break;
            case 3:
                graphicText.text = "High";
                break;
            case 4:
                graphicText.text = "Very High";
                break;
            case 5:
                graphicText.text = "Ultra";
                break;
            default:
                Debug.Log("Error with graphic values");
                break;
        }
    }

    public void OnLeftArrowClicked()
    {
        if (graphic_value > 0)
        {
            graphic_value--;

            DisplayGraphicText();
            SetGraphics(graphic_value);
        }

    }

    public void OnRightArrowClicked()
    {
        if (graphic_value < 5)
        {
            graphic_value++;

            DisplayGraphicText();
            SetGraphics(graphic_value);
        }
    }
}
