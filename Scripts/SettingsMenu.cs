using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{

    [SerializeField] GameObject[] characters;

    public void SetSFXVolume(float volume)
    {
        for (int i = 1; i < 16; i++)
        {
            FindObjectOfType<AudioManager>().SetVolume("ChessPiece_" + i.ToString(), volume, 1);
        }
    }
    public void SetBackgroundCharaters(bool isVisible)
    {
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].SetActive(isVisible);
        }
    }
}
