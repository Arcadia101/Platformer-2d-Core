using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IInteractable
{
    public void Interact();
}
public class Interactor : MonoBehaviour
{
    private bool _canInteract;
    private GameObject _interactuable;
    
    void Start()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Inretactuable"))
        {
            _canInteract = true;
            _interactuable = other.gameObject;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Interactuable"))
        {
            _canInteract = false;
            _interactuable = null;
        }
    }
    public void Interact()
    {
        if (_canInteract)
        {
            if (_interactuable.TryGetComponent(out IInteractable interactObj))
            {
                interactObj.Interact();
            }
        }
    }
}
