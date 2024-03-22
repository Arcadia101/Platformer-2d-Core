using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public static CharacterStats Instance;
    private Movement2D _controller;
    public bool _enteringScene = false;
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

    private void Awake()
    {
        _character = this.gameObject;
        _tag = _character.tag;
        if (_tag == "Player")
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
            _controller = GetComponent<Movement2D>();
            DontDestroyOnLoad(gameObject);
        }
    }
    private void Start()
    {
        _currentHealth = _maxHealth;
        _lastPosition = transform.position;
        _isDead = false;
        _invincibleTimeCounter = _invincibleTime;
        _isInvincible = true;
        _takingDamage = false;
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
        if (!_isDead && !_isInvincible)
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

    public void SaveStats(float health, float invincibleTime, Vector3 lastPosition, bool isDead)
    {
        health = _currentHealth;
        invincibleTime = _invincibleTimeCounter;
        lastPosition = _lastPosition;
        isDead = _isDead;
    }

    public void LoadStats(float health, float invincibleTime, Vector3 lastPosition, bool isDead)
    {
        _currentHealth = health;
        _invincibleTimeCounter = invincibleTime;
        _lastPosition = lastPosition;
        _isDead = isDead;
    }

    public void SetStartPosition(Vector3 startPosition)
    {
        transform.position = startPosition;
    }
    public void EnteringScene(Vector2 enteringDirection, float duration)
    {
        StartCoroutine(_controller.WalkIntoNewScene(enteringDirection, duration));
    }
}
