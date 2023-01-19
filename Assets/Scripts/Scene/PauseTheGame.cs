using UnityEngine;

//Pauses every object that is time-based
public class PauseTheGame : MonoBehaviour
{
    public void PauseGame()
    {
        Time.timeScale = 0.0f;
    }
    public void ResumeGame()
    {
        Time.timeScale = 1.0f;
    }
}
