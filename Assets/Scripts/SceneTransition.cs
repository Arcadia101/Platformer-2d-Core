using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string _transitionTo;
    [SerializeField] private Transform _StartPoint;
    [SerializeField] private Vector2 _exitDirection;
    [SerializeField] private float _exitTime;

    private void Start()
    {
        if (_transitionTo == GameManager.Instance.transitionedFromScene)
        {
            CharacterStats.Instance.SetStartPosition(_StartPoint.position);
            CharacterStats.Instance.EnteringScene(_exitDirection, _exitTime);
        }
        StartCoroutine(UIManager.Instance.sceneFader.Fade(SceneFader.FadeDirection.Out));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !CharacterStats.Instance._enteringScene)
        {
            GameManager.Instance.transitionedFromScene = SceneManager.GetActiveScene().name;
            CharacterStats.Instance._enteringScene = true;
            StartCoroutine(UIManager.Instance.sceneFader.FadeAndLoadScene(SceneFader.FadeDirection.In, _transitionTo));
        }
    }
}
