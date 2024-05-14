using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlsNavigation : MonoBehaviour
{
    // go back to welcome screen
    public void Back() => SceneManager.LoadScene(0);

}
