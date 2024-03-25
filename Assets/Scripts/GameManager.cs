using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public string transitionedFromScene;
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }


    public void CharaterIsDead(bool isDead, string tag, GameObject character)
    {
        if (tag == "Player")
        {
            UIManager.Instance.ActiveGameOverScreen();
        }
        character.SetActive(false);
    }
}
