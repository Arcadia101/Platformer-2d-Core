using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraDash : MonoBehaviour
{
    private Collider2D _collider;
    private SpriteRenderer _spriteRenderer;

    [SerializeField] private Sprite _activeSprite;
    [SerializeField] private Sprite _inactiveSprite;
    [SerializeField] private float _timeCounter;

    private void Start()
    {
        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Movement2D>().AddExtraDash();
            //animation
            Respawn();
        }
    }

    private void Respawn()
    {
        _collider.enabled = false;
        _spriteRenderer.sprite = _inactiveSprite;

        StartCoroutine(ResetExtraDash());
    }

    private IEnumerator ResetExtraDash()
    {
        yield return new WaitForSeconds(_timeCounter);
        _spriteRenderer.sprite = _activeSprite;
        _collider.enabled = true;
    }
}
