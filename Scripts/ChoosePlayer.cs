using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoosePlayer : MonoBehaviour
{
    [SerializeField] private GameObject[] characters;
    [SerializeField] private bool isTeamWhite = false;
    [SerializeField] private bool isTeamBlack = false;

    private GameObject currectCharacter;
    private Animator characterAnimetor;

    private void Awake()
    {
        SetInvisibleAllCharacters();
    }

    private void Start()
    {
        PickOneRandomCharacter();
        characterAnimetor = currectCharacter.GetComponent<Animator>();
        ChooseRandomSittingPose();
    }

    private void Update()
    {
        // if(Input.GetKeyDown(KeyCode.Space))
        // {
        //     OnCheckMade(1); // Black
        // }
    }

    private void SetInvisibleAllCharacters()
    {
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].SetActive(false);
        }
    }

    private void PickOneRandomCharacter()
    {
        int r = Random.Range(0, characters.Length);
        characters[r].SetActive(true);
        currectCharacter = characters[r];
    }

    public void ChooseRandomSittingPose()
    {
        if(isTeamWhite || isTeamBlack)
        {
            int r = Random.Range(0, 2);
            if (r == 0)
                characterAnimetor.SetTrigger("Sitting_Idle_1");
            else if (r == 1)
                characterAnimetor.SetTrigger("Sitting_Idle_2");
        }
    }

    public void OnCheckMade(bool isWhite)
    {
        if (isWhite)
        {
            characterAnimetor.SetTrigger("Check_Good");
            // characterAnimetor.SetTrigger("Check_Bad");
        }
        
        if(!isWhite)
        {
            characterAnimetor.SetTrigger("Check_Good");
            // characterAnimetor.SetTrigger("Check_Bad");
        }

        ChooseRandomSittingPose();
    }

    public void OnPieceEaten(bool isWhite)
    {
        if (isWhite)
        {
            characterAnimetor.SetTrigger("Check_Good");
        }

        if (!isWhite)
        {
            characterAnimetor.SetTrigger("Check_Good");
        }

        ChooseRandomSittingPose();
    }
}