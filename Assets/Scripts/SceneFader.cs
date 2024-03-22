using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    [SerializeField] private float _fadeTime;
    private Image _fadeOutUiImage;

    public enum FadeDirection
    {
        In,
        Out
    }
    void Start()
    {
        _fadeOutUiImage = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public IEnumerator Fade(FadeDirection _fadeDirection)
    {
        float _alpha = _fadeDirection == FadeDirection.Out? 1 : 0;
        float _fadeEndValue = _fadeDirection == FadeDirection.Out? 0 : 1;

        if (_fadeDirection == FadeDirection.Out)
        {
            while (_alpha >= _fadeEndValue)
            {
                SetColorImage(ref _alpha, _fadeDirection);

                yield return null;
            }
            _fadeOutUiImage.enabled = false;
        }
        else
        {
            _fadeOutUiImage.enabled = true;

            while (_alpha <= _fadeEndValue)
            {
                SetColorImage(ref _alpha, _fadeDirection);

                yield return null;
            }
        }
    }

    public IEnumerator FadeAndLoadScene(FadeDirection _fadeDirection, string _levelToLoad)
    {
        _fadeOutUiImage.enabled = true;

        yield return Fade(_fadeDirection);

        SceneManager.LoadScene(_levelToLoad);
    }

    void SetColorImage(ref float _alpha, FadeDirection _fadeDirection)
    {
        _fadeOutUiImage.color = new Color(_fadeOutUiImage.color.r, _fadeOutUiImage.color.g, _fadeOutUiImage.color.b, _alpha);
        
        _alpha += Time.deltaTime * (1/ _fadeTime) * (_fadeDirection == FadeDirection.Out ? -1 : 1);
    }
}
