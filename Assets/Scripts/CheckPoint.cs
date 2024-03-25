using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckPoint : MonoBehaviour, IInteractable
{
    public string _sceneName {get; private set;}
    public bool _active;

    void Start()
    {
        _sceneName = SceneManager.GetActiveScene().name;
    }

    public void Interact()
    {
        if (_active)
        {
            Debug.Log("Checkpoint Activated");
        }
        else
        {
            Debug.Log("Checkpoint Interact");
            _active = true;
        }
    }

}
