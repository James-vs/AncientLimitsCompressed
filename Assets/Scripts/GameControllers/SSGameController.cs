using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SSGameController : GameController
{

    private GameObject player1;
    private GameObject player2;
    public GameObject winnerText;
    public bool playersConnected;
    private bool isP1Connected;
    private bool isP2Connected;

    public GameObject startScreen;
    public GameObject startCamera;



    private void Start()
    {
        escapeTimer = maxEscapeTime;
        levelComplete = false;
        gameOverScreen.SetActive(false);
        levelCompleteScreen.SetActive(false);
        hud.SetActive(false);
        startScreen.SetActive(true);
        lockFirstSelect = false;
        playersConnected = false;
        isP1Connected = false;
        isP2Connected = false;
        Time.timeScale = 0f;

    }

    private void Update()
    {
        if (!playersConnected)
        {
            CheckPlayersConnected();
        }
        else
        {
            Debug.Log("players connected");
            CountDown();
            CheckLevelComplete();
        }
    }


    private void CheckPlayersConnected()
    {
        if (player1 != null) isP1Connected = true;
        if (player2 != null) isP2Connected = true;
        if (isP1Connected && isP2Connected)
        {
            Time.timeScale = 1f;
            startScreen.SetActive(false);
            startCamera.SetActive(false);
            hud.SetActive(true);
        }

        playersConnected = isP1Connected && isP2Connected;
        //Debug.Log("Players Connected: " + isP1Connected + " " + isP2Connected + " " + playersConnected);
    }


    // assign player objects to gamecontroller
    public override void AssignPlayer(GameObject playerObj, bool isplayer1)
    {
        if (isplayer1)
        {
            player1 = playerObj;
        } else
        {
            player2 = playerObj;
        }
    }

    public new void ExitToStart()
    {
        player1.GetComponent<SSControllerPlayerMovement>().ResetFirstPlayer();
        EventSystem.current.SetSelectedGameObject(null);
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public new void RestartLevel()
    {
        player1.GetComponent<SSControllerPlayerMovement>().ResetFirstPlayer();
        EventSystem.current.SetSelectedGameObject(null);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public override void LevelComplete(GameObject player)
    {
        levelComplete = true;
        if (player == player1) winnerText.GetComponent<TextMeshProUGUI>().SetText("Player 2 Wins!!");
        if (player == player2) winnerText.GetComponent<TextMeshProUGUI>().SetText("Player 1 Wins!!");
    }

    public override void NextLevel()
    {
        EventSystem.current.SetSelectedGameObject(null);
        Time.timeScale = 1f;
        player1.GetComponent<SSControllerPlayerMovement>().ResetFirstPlayer();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

}
