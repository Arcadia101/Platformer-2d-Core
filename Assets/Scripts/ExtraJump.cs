using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraJump : MonoBehaviour
{
    private Collider2D _collider;
    private SpriteRenderer _spriteRenderer;

    [SerializeField] private Sprite _activeSprite;
    [SerializeField] private Sprite _inactiveSprite;
    [SerializeField] private int _extraJumps;
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
            other.GetComponent<Movement2D>().AddExtraJumps(_extraJumps);
            //animation
            Respawn();
        }
    }

    private void Respawn()
    {
        _collider.enabled = false;
        _spriteRenderer.sprite = _inactiveSprite;

        StartCoroutine(ResetExtraJump());
    }

    private IEnumerator ResetExtraJump()
    {
        yield return new WaitForSeconds(_timeCounter);
        _spriteRenderer.sprite = _activeSprite;
        _collider.enabled = true;
    }
}
