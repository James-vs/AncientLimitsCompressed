using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("Timer")]
    public float maxEscapeTime;
    protected float escapeTimer;
    public bool timeOut;
    protected bool levelComplete;

    [Header("UI Components")]
    public GameObject uiTimer;
    public GameObject hud;
    public GameObject gameOverScreen;
    public GameObject levelCompleteScreen;
    public GameObject highScore;
    public GameObject levelScore;

    [Header("First Selected Buttons")]
    public GameObject levelCompleteScreenFirst;
    public GameObject gameOverScreenFirst;
    protected bool lockFirstSelect;

    [Header("PlayerPrefs Highscore")]
    public string playerphighscorename;
    
    
    // Start is called before the first frame update
    void Start()
    {
        escapeTimer = maxEscapeTime;
        levelComplete = false;
        gameOverScreen.SetActive(false);
        levelCompleteScreen.SetActive(false);
        hud.SetActive(true);
        lockFirstSelect = false;
    }

    // Update is called once per frame
    void Update()
    {
        CountDown();
        CheckLevelComplete();
        
    }


    protected void CountDown()
    {
        if (escapeTimer > 0f && !levelComplete)
        {
            escapeTimer -= Time.deltaTime;
            uiTimer.GetComponent<TextMeshProUGUI>().SetText(escapeTimer.ToString("0") + "s");
        } 
        else if (escapeTimer < 0f && !levelComplete && !lockFirstSelect)
        {
            //end the level
            
            Time.timeScale = 0;
            timeOut = true;
            hud.SetActive(false);
            gameOverScreen.SetActive(true);
            EventSystem.current.SetSelectedGameObject(gameOverScreenFirst);
            //gameOverScreenFirst.GetComponent<Button>().Select();
            lockFirstSelect = true;
            
        }
        
    }

    public void ExitToStart()
    {
        EventSystem.current.SetSelectedGameObject(null);
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void RestartLevel()
    {
        EventSystem.current.SetSelectedGameObject(null);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void CheckLevelComplete()
    {
        if (levelComplete && !lockFirstSelect)
        {
            levelCompleteScreen.SetActive(true);
            hud.SetActive(false);
            Time.timeScale = 0;
            EventSystem.current.SetSelectedGameObject(levelCompleteScreenFirst);
            CheckHighScore();
            highScore.GetComponent<TextMeshProUGUI>().SetText("High Score: " + PlayerPrefs.GetFloat(playerphighscorename).ToString("0"));
            levelScore.GetComponent<TextMeshProUGUI>().SetText("Level Score: " + escapeTimer.ToString("0"));
            //EventSystem.current.SetSelectedGameObject(null); // debugging
            //levelCompleteScreenFirst.GetComponent<Button>().Select();
            lockFirstSelect = true;
        }
    }

    public virtual void LevelComplete(GameObject player)
    {
        // not in use
        levelComplete = true;
    }


    public virtual void AssignPlayer(GameObject playerObj, bool isplayer1)
    {
        //left empty on purpose
        Debug.Log("Accessing Empty Function");
    }

    protected void CheckHighScore()
    {
        if (!PlayerPrefs.HasKey(playerphighscorename))
        {
            PlayerPrefs.SetFloat(playerphighscorename, escapeTimer);
        } 
        else
        {
            if (escapeTimer > PlayerPrefs.GetFloat(playerphighscorename))
            {
                PlayerPrefs.SetFloat(playerphighscorename, escapeTimer);
            }
        }
    }

    public virtual void NextLevel()
    {
        EventSystem.current.SetSelectedGameObject(null);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
