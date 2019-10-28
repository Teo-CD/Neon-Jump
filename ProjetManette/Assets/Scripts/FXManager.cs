using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXManager : MonoBehaviour
{
    [SerializeField] GameObject _dashTrailFX;
    [SerializeField] GameObject _dashCdFX;
    [SerializeField] GameObject _jumpFX;

    public void PlayDashCdFX()
    {
        StartCoroutine(DashCdFX());
    }

    public void PlayDashTrailFX()
    {
        StartCoroutine(DashTrailFX());
    }

    public void PlayJumpFX()
    {
        StartCoroutine(JumpFX());
    }
    

    IEnumerator DashCdFX()
    {
        _dashCdFX.SetActive(true);
        yield return new WaitForSeconds(.9f);
        _dashCdFX.SetActive(false);

    }
    IEnumerator DashTrailFX()
    {
        _dashTrailFX.SetActive(true);
        yield return new WaitForSeconds(1f);
        _dashTrailFX.SetActive(false);

    }

    IEnumerator JumpFX()
    {
        _jumpFX.SetActive(true);
        yield return new WaitForSeconds(.2f);
        _jumpFX.SetActive(false);

    }
}
