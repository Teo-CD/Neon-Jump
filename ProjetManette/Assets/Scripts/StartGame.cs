using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    [SerializeField] Animator _animator;
    void Update()
    {
        if(Input.anyKeyDown)
        {
            _animator.SetTrigger("Fade");
            Invoke("Load",.95f);
        }
    }

    private void Load()
    {
        SceneManager.LoadScene(1);
    }
}
