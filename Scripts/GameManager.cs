using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject panel;
    public bool isGamePaused = false;
    private bool isPlayingAnimation = false;

    // Start is called before the first frame update
    void Start()
    {
        panel.transform.localScale = Vector2.zero;
        menu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(FindObjectOfType<ChessBoard>().hasGameStarted)
        {
            if (!isPlayingAnimation)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (!isGamePaused)
                    {
                        StartCoroutine(OnOpenMenu());
                    }
                    else if (isGamePaused)
                    {
                        StartCoroutine(OnCloseMenu());
                    }
                }
            }
        }
    }
    IEnumerator OnOpenMenu()
    {
        isPlayingAnimation = true;
        isGamePaused = true;
        menu.SetActive(true);

        float wait_time = 0.5f;
        panel.transform.LeanScale(Vector2.one, wait_time).setEaseOutBack().setIgnoreTimeScale(true);
        yield return new WaitForSecondsRealtime(wait_time);

        isPlayingAnimation = false;
    }

    IEnumerator OnCloseMenu()
    {
        isPlayingAnimation = true;

        float wait_time = 0.5f;
        panel.transform.LeanScale(Vector2.zero, wait_time).setEaseInBack().setIgnoreTimeScale(true);
        yield return new WaitForSecondsRealtime(wait_time);

        isPlayingAnimation = false;
        isGamePaused = false;
        menu.SetActive(false);
    }

    public void OnExitButtonClicked()
    {
        // save any game data here
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public void OnResumeButtonClicked()
    {
        StartCoroutine(OnCloseMenu());
    }
}