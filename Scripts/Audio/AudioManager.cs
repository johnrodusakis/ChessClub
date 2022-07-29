using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{

    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider menuSfxVolume;
    [SerializeField] private Slider gameSfxVolume;
    [SerializeField] private Slider inGameSfxVolume;
    [SerializeField] private TMPro.TextMeshProUGUI master_value_text;
    [SerializeField] private TMPro.TextMeshProUGUI menuSfx_value_text;
    [SerializeField] private TMPro.TextMeshProUGUI gameSfx_value_text;
    [SerializeField] private TMPro.TextMeshProUGUI inGameSfx_value_text;

    private float master_volume;
    private float menuSfx_volume;
    private float gameSfx_volume;
    private float inGameSfx_volume;

    [SerializeField] Sound[] sounds;

    void Awake()
    {
        foreach (Sound s in sounds)
        {
            s.source = this.gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    private void Start()
    {
        InitializeMasterVolume();
        InitializeGameSFXVolume();
        InitializeMenuSFXVolume();
    }
    private void InitializeMasterVolume()
    {
        masterVolume.value = PlayerPrefs.GetFloat("MasterVolume", master_volume);

        InitializeVolume("ButtonHover", master_volume);
        InitializeVolume("ButtonClick", master_volume/4);
        InitializeVolume("GameOver", master_volume);

        for (int i = 0; i < 16; i++)
        {
            InitializeVolume("ChessPiece_" + (i + 1).ToString(), masterVolume.value);
        }
    }

    private void InitializeGameSFXVolume()
    {
        gameSfxVolume.value = PlayerPrefs.GetFloat("GameSfxVolume", gameSfx_volume);
        for (int i = 0; i < 16; i++)
        {
            InitializeVolume("ChessPiece_" + (i + 1).ToString(), gameSfxVolume.value);
        }
    }
    private void InitializeMenuSFXVolume()
    {
        menuSfxVolume.value = PlayerPrefs.GetFloat("MenuSfxVolume", menuSfx_volume);

        InitializeVolume("ButtonHover", menuSfx_volume);
        InitializeVolume("ButtonClick", menuSfx_volume/4);
        InitializeVolume("GameOver", menuSfx_volume);
    }


    private void InitializeVolume(string name, float value)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound " + name + " not found!");
            return;
        }
        s.source.volume = value;
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound " + name + " not found!");
            return;
        }
        s.source.Play();
    }

    public void SetVolume(string name, float value, int category)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound " + name + " not found!");
            return;
        }

        switch (category)
        {
            case 0:
                PlayerPrefs.SetFloat("MasterVolume", value);
                break;
            case 1:
                PlayerPrefs.SetFloat("GameSfxVolume", value);
                break;
            case 2:
                PlayerPrefs.SetFloat("MenuSfxVolume", value);
                break;
            default:
                Debug.Log("Error with PlayerPrefs");
                break;
        }

        s.source.volume = value;
    }

    public void SelectRandomChessPieceClip()
    {
        ArrayList chessPieceClips = new ArrayList();
        for (int i = 1; i < 16; i++)
        {
            string temp_str = "ChessPiece_" + i.ToString();
            chessPieceClips.Add(temp_str);
        }

        System.Random rnd = new System.Random();
        int r = rnd.Next(1, 10);

        Play(chessPieceClips[r].ToString());
    }
    public void OnMasterVolumeChange()
    {
        master_volume = masterVolume.value;

        SetVolume("ButtonHover", master_volume, 0);
        SetVolume("ButtonClick", master_volume/4, 0);
        SetVolume("GameOver", master_volume, 0);

        for (int i = 1; i < 16; i++)
        {
            SetVolume("ChessPiece_" + i.ToString(), master_volume, 0);
        }

        master_value_text.text = (master_volume * 100).ToString("0");
        PlayerPrefs.SetFloat("MasterVolume", master_volume);

        gameSfxVolume.value = master_volume;
        gameSfx_value_text.text = (master_volume * 100).ToString("0");
        // PlayerPrefs.SetFloat("GameSfxVolume", master_volume);

        inGameSfxVolume.value = master_volume;
        inGameSfx_value_text.text = (master_volume * 100).ToString("0");
        PlayerPrefs.SetFloat("GameSfxVolume", master_volume);

        menuSfxVolume.value = master_volume;
        menuSfx_value_text.text = (master_volume * 100).ToString("0");
        PlayerPrefs.SetFloat("MenuSfxVolume", master_volume);
    }

    public void OnGameSfxVolumeChange()
    {
        gameSfx_volume = gameSfxVolume.value;

        for (int i = 1; i < 16; i++)
        {
            SetVolume("ChessPiece_" + i.ToString(), gameSfx_volume, 1);
        }

        gameSfx_value_text.text = (gameSfx_volume * 100).ToString("0");
        // PlayerPrefs.SetFloat("GameSfxVolume", gameSfx_volume);

        inGameSfxVolume.value = gameSfx_volume;
        inGameSfx_value_text.text = (gameSfx_volume * 100).ToString("0");
        PlayerPrefs.SetFloat("GameSfxVolume", gameSfx_volume);
    }

    public void OnInGameSfxVolumeChange()
    {
        inGameSfx_volume = inGameSfxVolume.value;

        for (int i = 1; i < 16; i++)
        {
            SetVolume("ChessPiece_" + i.ToString(), inGameSfx_volume, 1);
        }

        inGameSfx_value_text.text = (inGameSfx_volume * 100).ToString("0");
        // PlayerPrefs.SetFloat("GameSfxVolume", inGameSfx_volume);

        gameSfxVolume.value = inGameSfx_volume;
        gameSfx_value_text.text = (inGameSfx_volume * 100).ToString("0");
        PlayerPrefs.SetFloat("GameSfxVolume", inGameSfx_volume);
    }

    public void OnMenuSfxVolumeChange()
    {
        menuSfx_volume = menuSfxVolume.value;

        SetVolume("ButtonHover", menuSfx_volume, 2);
        SetVolume("ButtonClick", menuSfx_volume/4, 2);
        SetVolume("GameOver", menuSfx_volume, 2);

        menuSfx_value_text.text = (menuSfx_volume * 100).ToString("0");
        PlayerPrefs.SetFloat("MenuSfxVolume", menuSfx_volume);
    }

    public void OnButtonHover()
    {
        Play("ButtonHover");
    }

    public void OnButtonClick()
    {
        Play("ButtonClick");
    }
}
