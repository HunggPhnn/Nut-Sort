using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Panel tham chiếu")]
    public GameObject settingsPanel; 
    public GameObject helpPanel;
    public GameObject lbPanel;

    public void PlayGame()
    {
        SceneManager.LoadScene("Level1"); 
    }

    public void QuitGame()
    {
        Debug.Log("Thoát game!"); 
        Application.Quit(); 
    }

    public void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true); // Hiện panel
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false); // Ẩn panel
    }

    public void OpenHelp()
    {
        if (helpPanel != null) helpPanel.SetActive(true);
    }

    public void CloseHelp()
    {
        if (helpPanel != null) helpPanel.SetActive(false);
    }

    public void OpenLb()
    {
        if (lbPanel != null) lbPanel.SetActive(true);
    }

    public void CloseLb()
    {
        if (lbPanel != null) lbPanel.SetActive(false);
    }
}
