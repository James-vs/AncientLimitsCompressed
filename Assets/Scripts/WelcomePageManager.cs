using UnityEngine;
using UnityEngine.SceneManagement;

public class WelcomePageManager : MonoBehaviour
{
    public void LoadSinglePlayer() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

    public void LoadMultiPlayer() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 3);

    public void LoadControlsScreen() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 5);

    public void ExitGame() => Application.Quit();
}
