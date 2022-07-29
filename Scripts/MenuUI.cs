using System;
using UnityEngine;
using UnityEngine.UI;

public enum CameraAngle
{
    menu =0,
    whiteTeam = 1,
    blackTeam = 2,
    gameOver = 3
}

public class MenuUI : MonoBehaviour
{
    public static MenuUI Instance { set; get; }

    public Server server;
    public Client client;

    [SerializeField] private Animator menuAnimetor;
    [SerializeField] private TMPro.TMP_InputField addressInput;
    [SerializeField] private GameObject[] cameraAngles;

    [SerializeField] private GameObject Timers;
    [SerializeField] private Slider timeSlider;
    [SerializeField] private TMPro.TMP_Text sliderValue;

    private float timeValue = 5f;
    private bool isHost = false;

    public Action<bool> SetLocalGame;

    private void Awake()
    {
        Instance = this;

        RegisterEvents();

        sliderValue.text = timeValue.ToString("0") + " Min";
    }

    // Cameras
    public void ChangeCamera(CameraAngle index)
    {
        for (int i = 0; i < cameraAngles.Length; i++)
            cameraAngles[i].SetActive(false);

        cameraAngles[(int)index].SetActive(true);
    }

    // Buttons
    public void OnLocalGameButtonClicked()
    {
        menuAnimetor.SetTrigger("GameMenu");
        SetLocalGame?.Invoke(true);
        server.Init(8007);
        client.Init("127.0.0.1", 8007);

        Timers.SetActive(true);
        FindObjectOfType<ChessBoard>().hasGameStarted = true;
        FindObjectOfType<ChessBoard>().firstMove = false;
    }
    public void OnOnlineGameButtonClicked()
    {
        menuAnimetor.SetTrigger("OnlineMenu");
    }
    public void OnOnlineHostButtonClicked()
    {
        SetLocalGame?.Invoke(false);
        server.Init(8007);
        client.Init("127.0.0.1", 8007);
        menuAnimetor.SetTrigger("HostMenu");
        SetIsHost(true);
    }
    public void OnOnlineConnectButtonClicked()
    {
        SetLocalGame?.Invoke(false);
        client.Init(addressInput.text, 8007);
    }
    public void OnOnlineBackButtonClicked()
    {
        menuAnimetor.SetTrigger("StartMenu");
    }
    public void OnHostBackButtonClicked()
    {
        server.ShutDown();
        client.ShutDown();
        SetIsHost(false);
        menuAnimetor.SetTrigger("OnlineMenu");
    }

    public void OnSettingsButtonClicked()
    {
        menuAnimetor.SetTrigger("SettingsMenu");
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

    public void OnLeaveFromGameMenu()
    {
        ChangeCamera(CameraAngle.menu);
        menuAnimetor.SetTrigger("StartMenu");
    }

    public void OnGameOver()
    {
        ChangeCamera(CameraAngle.gameOver);

        Timers.SetActive(false);
        FindObjectOfType<ChessBoard>().hasGameStarted = false;
        FindObjectOfType<ChessBoard>().firstMove = false;
    }

    public void OnRematch()
    {
        Timers.SetActive(true);
        FindObjectOfType<ChessBoard>().hasGameStarted = true;
        FindObjectOfType<ChessBoard>().firstMove = false;
    }

    #region
    public void RegisterEvents()
    {
        NetUtility.C_START_GAME += OnStartGameClient;
    }
    public void UnRegisterEvents()
    {
        NetUtility.C_START_GAME -= OnStartGameClient;
    }

    private void OnStartGameClient(NetMessage msg)
    {
        menuAnimetor.SetTrigger("GameMenu");

        Timers.SetActive(true);
        FindObjectOfType<ChessBoard>().hasGameStarted = true;
        FindObjectOfType<ChessBoard>().firstMove = false;
    }

    public void OnTimeSliderChangeValue()
    {
        timeValue = timeSlider.value;
        Server.Instance.SetTimer(getTimeValue() * 60);
        sliderValue.text = timeSlider.value.ToString("0") + " Min";
    }
    #endregion

    // Getters - Setters
    public float getTimeValue()
    {
        return this.timeValue;
    }
    public bool getIsHost()
    {
        return this.isHost;
    }
    public void SetIsHost(bool newHost)
    {
        this.isHost = newHost; 
    }
}
