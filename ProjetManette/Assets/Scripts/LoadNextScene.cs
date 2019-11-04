using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNextScene : MonoBehaviour
{
    private int _sceneToLoadIndex;
    [SerializeField] Animator _anim;

    void Start()
    {
        _sceneToLoadIndex = SceneManager.GetActiveScene().buildIndex + 1;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Invoke("Load", .35f);
        _anim.SetTrigger("FadeOut");
    }

    void Load()
    {
        SceneManager.LoadScene(_sceneToLoadIndex);
    }
}
