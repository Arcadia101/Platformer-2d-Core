using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [SerializeField] private float _maxHealth;
    [SerializeField] private float _invincibleTime;
    private float _currentHealth;
    private float _invincibleTimeCounter;
    private Vector3 _lastPosition;
    private bool _isDead;
    private bool _isInvincible;
    private bool _takingDamage;
    private string _tag;
    private GameObject _character;

    private void Start()
    {
        _currentHealth = _maxHealth;
        _lastPosition = transform.position;
        _isDead = false;
        _invincibleTimeCounter = _invincibleTime;
        _isInvincible = true;
        _takingDamage = false;
        _character = this.gameObject;
        _tag = _character.tag;
    }

    private void Update()
    {
        if (_takingDamage)
        {
            InvincibleTime();
        }
        else if (_isDead)
        {
            //GameManager.Instance.CharaterIsDead(_isDead, _tag, _character);
        }
    }

    public void TakeDamage(float damage)
    {
        if (!_isDead && ! _isInvincible)
        {
            _currentHealth -= damage;
            _invincibleTimeCounter = _invincibleTime;
            _takingDamage = true;
        }
        else if (_currentHealth <= 0f && !_isInvincible)
        {
            //death animation
            _isDead = true;
        }
    }

    public void Heal(float heal)
    {
        if (!_isDead)
        {
            _currentHealth += heal;
        }
    }

    public void InvincibleTime()
    {
        _isInvincible = true;
        _takingDamage = false;
        if (_invincibleTimeCounter > 0f)
        {
            _invincibleTimeCounter -= Time.deltaTime;
        }
        else
        {
            _invincibleTimeCounter = 0f;
            _isInvincible = false;
        }
    }

    public void SaveLastPosition(Vector3 lastPosition)
    {
        _lastPosition = lastPosition;
    }

    public void Respawn()
    {
        transform.position = _lastPosition;
    }
    
    public void SaveStats(float health, float invincibleTime, Vector3 lastPosition)
    {
        health = _currentHealth;
        invincibleTime = _invincibleTimeCounter;
        lastPosition = _lastPosition;
    }

    
}
