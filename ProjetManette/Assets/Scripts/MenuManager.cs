using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] Animator _animator;

    public void StartGame()
    {
        _animator.SetTrigger("Fade");
        Invoke("LoadGame", .95f);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void LoadGame()
    {
        SceneManager.LoadScene(1);
    }
}
