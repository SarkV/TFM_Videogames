using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnSimpleGame()
    {
        SceneManager.LoadScene("SimpleGameScene");
    }
    public void OnBattleGame()
    {
        SceneManager.LoadScene("BattleGameScene");
    }
    public void OnExit()
    {
        Application.Quit();
    }
}
