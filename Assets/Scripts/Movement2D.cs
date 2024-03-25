using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement2D : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D _rb;
    private Animator _animator;
    private PlayerInputActions _actions;
    private CharacterStats _stats;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _wallLayer;
    [SerializeField] private LayerMask _cornerCorrectLayer;

    [Header("Movement Variables")]
    [SerializeField] private float _movementAcceleration;
    [SerializeField] private float _maxMoveSpeed;
    [SerializeField] private float _groundLinearDrag;
    private float _horizontalInput;
    private bool _changingDirection => (_rb.velocity.x > 0 && _horizontalInput < 0) || (_rb.velocity.x < 0 && _horizontalInput > 0);
    private bool _facingRigth = true;
    private bool _canMove => !_wallGrab;

    [Header("Jump Variables")]
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _airLinearDrag;
    [SerializeField] private float _fallMultiplier;
    [SerializeField] private float _lowJumpFallMultiplier;
    [SerializeField] private int _extraJumps;
    [SerializeField] private float _coyoteTime;
    [SerializeField] private float _jumpBufferLength;
    private int _extraJumpsValue;
    private float _coyoteTimeCounter;
    private float _jumpBufferCounter;
    private bool _canJump => _jumpBufferCounter > 0f && (_coyoteTime > 0f || _extraJumpsValue > 0 || _canWallJump);
    private bool _canWallJump => _onWall && !_onGround && _wallJumpsCounter > 0f;
    private bool _isJumping = false;

    [Header("Wall Movement Variables")]
    [SerializeField] private float _wallSlideModifier;
    [SerializeField] private float _wallClimbModifier;
    [SerializeField] private float _wallClimbTime;
    [SerializeField] private float _wallJumpXVelocityHaltDelay;
    [SerializeField] private int _maxWallJumps;
    private int _wallJumpsCounter;
    private float _wallClimbTimeCounter;
    private float _verticalInput;
    private bool _wallGrab => _onWall && !_onGround && _actions.Player.Grab.phase == InputActionPhase.Performed && !_wallClimb && _wallClimbTimeCounter > 0f;
    private bool _wallSlide => _onWall && !_onGround && !_wallGrab && _rb.velocity.y < 0f && !_wallClimb;
    private bool _wallClimb => _onWall && (_verticalInput > 0f || _verticalInput < 0f) && _actions.Player.Grab.phase == InputActionPhase.Performed && _wallClimbTimeCounter > 0f;
    
    [Header("Dash Variables")]
    [SerializeField] private float _dashSpeed;
    [SerializeField] private float _dashLength;
    [SerializeField] private float _dashBufferLength;
    private float _dashBufferCounter;
    private bool _isDashing = false;
    private bool _hasDashed = false;
    private bool _canDash => _dashBufferCounter > 0f && !_hasDashed;

    [Header("Ground Collision Variables")]
    [SerializeField] private float _groundRaycastLength;
    [SerializeField] private Vector3 _groundRaycastOffset;
    private bool _onGround;

    [Header("wall Collision Variables")]
    [SerializeField] private float _wallRaycastLength;
    private bool _onWall;
    [SerializeField] private bool _onRightWall;

    [Header("Corner Correction Variables")]
    [SerializeField] private float _topRaycastLength;
    [SerializeField] private Vector3 _edgeRaycastOffset;
    [SerializeField] private Vector3 _innerRaycastOffset;
    private bool _canCornerCorrect;


    private void Awake()
    {
        _actions = new PlayerInputActions();
        _rb = GetComponent<Rigidbody2D>();
        _stats = GetComponent<CharacterStats>();
        //_animator = GetComponent<Animator>();

        _actions.Player.Enable();
    }

    private void Update()
    {    
        if (_stats._enteringScene) return;
        if (_stats._isDead) return;
  
        _horizontalInput = GetInput().x;
        _verticalInput = GetInput().y;
        if (_actions.Player.Jump.WasPressedThisFrame() )
        {
            _jumpBufferCounter = _jumpBufferLength;
        }
        else
        {
            _jumpBufferCounter -= Time.deltaTime;
        }

        if (_actions.Player.Dash.WasPressedThisFrame())
        {
            Debug.Log("dash");
            _dashBufferCounter = _dashBufferLength;
        }
        else
        {
            _dashBufferCounter -= Time.deltaTime;
        }

        //Animations();
        if (_horizontalInput < 0 && _facingRigth && !_wallGrab)
        {
            Flip();
        }
        else if (_horizontalInput > 0 &&!_facingRigth && !_wallGrab)
        {
            Flip();
        }
    }

    private void FixedUpdate()
    {
        if (_stats._enteringScene) return;
        if (_stats._isDead) return; 
        CheckCollisions();
        if (_canDash) StartCoroutine(Dash(_horizontalInput, _verticalInput));
        if (!_isDashing)
        {
            Movement();
        }
        if (_canCornerCorrect) CornerCorrect(_rb.velocity.y);

    }

    private void Movement()
    {
        if (_canMove) MoveCharacter();
        else if (!_wallClimb || !_wallGrab) _rb.velocity = Vector2.Lerp(_rb.velocity, (new Vector2(_horizontalInput * _maxMoveSpeed, _rb.velocity.y)), 0.5f * Time.deltaTime);
        if (_onGround)
        {
            ApplyGroundLinearDrag();
            _extraJumpsValue = _extraJumps;
            _coyoteTimeCounter = _coyoteTime;
            _wallClimbTimeCounter = _wallClimbTime;
            _wallJumpsCounter = _maxWallJumps;
            _rb.gravityScale = 1f;
            _hasDashed = false;
            _stats.SaveLastPosition(transform.position);

        }
        else
        {
            ApplyAirLinearDrag();
            FallMultiplier();
            _coyoteTimeCounter -= Time.fixedDeltaTime;
            if (!_onWall || _rb.velocity.y < 0 || _wallClimb) _isJumping = false;
        }
        if (_canJump)
        {
            if (_onWall && !_onGround && _wallJumpsCounter > 0)
            {
                if (!_wallClimb && (_onRightWall && _horizontalInput > 0 || !_onRightWall && _horizontalInput < 0))
                {
                    Debug.Log("natural wall jump");
                    StartCoroutine(NeutralWallJump());
                }
                else
                {
                    Debug.Log("wall jump");
                    WallJump();
                }
                _wallJumpsCounter--;
                Flip();
            }
            else if(_onGround || (!_onGround && !_onWall && _extraJumpsValue > 0))
            {
                Jump(Vector2.up);
            }
        }
        if (!_isJumping)
        {

            if (_wallSlide) WallSlide();
            if (_wallGrab) GrabWall();
            if (_wallClimb) WallClimb();
            if (_onWall) StickToWall();
        }
    }

    private void MoveCharacter()
    {
        _rb.AddForce(new Vector2(_horizontalInput, 0f) * _movementAcceleration);
        if (Mathf.Abs(_rb.velocity.x) > _maxMoveSpeed)
        {
            _rb.velocity = new Vector2(Mathf.Sign(_rb.velocity.x) * _maxMoveSpeed, _rb.velocity.y);
        }
    }

    private void ApplyGroundLinearDrag()
    {
        if (Mathf.Abs(_horizontalInput) < 0.4f || _changingDirection)
        {
            _rb.drag = _groundLinearDrag;
        }
        else
        {
            _rb.drag = 0f;
        }
    }

    private void ApplyAirLinearDrag()
    {
        _rb.drag = _airLinearDrag;
    }

    private void Jump(Vector2 direction)
    {
        if (!_onGround && !_onWall)
            _extraJumpsValue--;
        
        ApplyAirLinearDrag();
        _rb.velocity = new Vector2(_rb.velocity.x, 0f);
        _rb.AddForce(direction * _jumpForce, ForceMode2D.Impulse);
        _coyoteTimeCounter = 0f;
        _jumpBufferCounter = 0f;
        _isJumping = true;
    }

    private void FallMultiplier()
    {
        if (_rb.velocity.y < 0)
        {
            _rb.gravityScale = _fallMultiplier;
        }
        else if (_rb.velocity.y > 0 && !_actions.Player.Jump.IsPressed())
        {
            _rb.gravityScale = _lowJumpFallMultiplier;
        }
        else
        {
            _rb.gravityScale = 1f;
        }
    }

    private void GrabWall()
    {
        _rb.gravityScale = 0f;
        _rb.velocity = new Vector2(_rb.velocity.x, 0f);
        _wallClimbTimeCounter -= Time.deltaTime;
    }

    private void WallClimb()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, _verticalInput *_maxMoveSpeed * _wallClimbModifier);
        _wallClimbTimeCounter -= Time.deltaTime * 2;        
    }

    private void WallSlide()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, -_maxMoveSpeed * _wallSlideModifier);        
    }

    private void StickToWall()
    {
        //push player towards wall
        if (_onRightWall && _horizontalInput >= 0)
        {
            _rb.velocity = new Vector2(1f, _rb.velocity.y);
        }
        else if(!_onRightWall && _horizontalInput <= 0)
        {
            _rb.velocity = new Vector2(-1f, _rb.velocity.y);
        }

        //face correct direction
        if (_onRightWall && !_facingRigth)
        {
            Flip();
        }
        else if(!_onRightWall && _facingRigth)
        {
            Flip();
        }
    }

    private void WallJump()
    {
        Debug.Log("wall jump Started");;
        Vector2 jumpDirection = _onRightWall? Vector2.left : Vector2.right;
        Jump(Vector2.up + jumpDirection);
    }

    IEnumerator NeutralWallJump()
    {
        Debug.Log("neutral wall jump Started");
        Vector2 jumpDirection = _onRightWall? Vector2.left : Vector2.right;
        Jump(Vector2.up + jumpDirection);
        yield return new WaitForSeconds(_wallJumpXVelocityHaltDelay);
        _rb.velocity = new Vector2(0f, _rb.velocity.y);
    }

    IEnumerator Dash(float x, float y)
    {
        Debug.Log("dash started");
        float dashStartTime = Time.time;
        _hasDashed = true;
        _isDashing = true;
        _isJumping = false;

        _rb.velocity = Vector2.zero;
        _rb.gravityScale = 0f;
        _rb.drag = 0f;
        Vector2 dir;
        if(x != 0f || y != 0f) dir = new Vector2(x, y);
        else
        {
            if (_facingRigth) dir = new Vector2(1f, 0f);
            else dir = new Vector2(-1f, 0f);
        }

        while (Time.time < dashStartTime + _dashLength)
        {
            _rb.velocity = dir.normalized * _dashSpeed;
            yield return null;
        }

        _isDashing = false;
    }

    private void Animations()
    {
        _animator.SetBool("isGrouded", _onGround);
        _animator.SetFloat("HorizontalDirection", MathF.Abs(_horizontalInput));
        if (_rb.velocity.y < 0)
        {
            _animator.SetBool("isJumping", false);
            _animator.SetBool("isFalling", true);
        }
    }

    private void Flip()
    {
        _facingRigth =!_facingRigth;
        transform.Rotate(0f, 180f, 0f);
    }

    private void CornerCorrect(float yVelocity)
    {
        //push player to the right
        RaycastHit2D _hit = Physics2D.Raycast(transform.position - _innerRaycastOffset + Vector3.up * _topRaycastLength , Vector3.left, _topRaycastLength, _cornerCorrectLayer);
        if (_hit.collider != null)
        {
            float _newPos = Vector3.Distance(new Vector3(_hit.point.x, transform.position.y, 0f) + Vector3.up * _topRaycastLength, transform.position - _edgeRaycastOffset + Vector3.up * _topRaycastLength);
            transform.position = new Vector3(transform.position.x + _newPos, transform.position.y, transform.position.z);
            _rb.velocity = new Vector2(_rb.velocity.x, yVelocity);
            return;
        }

        //push player to the left
        _hit = Physics2D.Raycast(transform.position + _innerRaycastOffset + Vector3.up * _topRaycastLength, Vector3.right, _topRaycastLength, _cornerCorrectLayer);
        if (_hit.collider!= null)
        {
            float _newPos = Vector3.Distance(new Vector3(_hit.point.x, transform.position.y, 0f) + Vector3.up * _topRaycastLength, transform.position + _edgeRaycastOffset + Vector3.up * _topRaycastLength);
            transform.position = new Vector3(transform.position.x - _newPos, transform.position.y, transform.position.z);
            _rb.velocity = new Vector2(_rb.velocity.x, yVelocity);
        }
    }

    private void CheckCollisions()
    {
        //ground collisions
        _onGround = Physics2D.Raycast(transform.position + _groundRaycastOffset, Vector2.down, _groundRaycastLength, _groundLayer) ||
                    Physics2D.Raycast(transform.position - _groundRaycastOffset, Vector2.down, _groundRaycastLength, _groundLayer);

        //corner collisions
        _canCornerCorrect = Physics2D.Raycast(transform.position + _edgeRaycastOffset, Vector2.up, _topRaycastLength, _cornerCorrectLayer) && 
                            !Physics2D.Raycast(transform.position + _innerRaycastOffset, Vector2.up, _topRaycastLength, _cornerCorrectLayer) ||
                            Physics2D.Raycast(transform.position - _edgeRaycastOffset, Vector2.up, _topRaycastLength, _cornerCorrectLayer) &&
                            !Physics2D.Raycast(transform.position - _innerRaycastOffset, Vector2.up, _topRaycastLength, _cornerCorrectLayer);
        
        //wall collisions
        _onWall = Physics2D.Raycast(transform.position, Vector2.right, _wallRaycastLength, _wallLayer) || 
                    Physics2D.Raycast(transform.position, Vector2.left, _wallRaycastLength, _wallLayer);
        _onRightWall = Physics2D.Raycast(transform.position, Vector2.right, _wallRaycastLength, _wallLayer);

    }

    private Vector2 GetInput()
    {
        return new Vector2(_actions.Player.Move.ReadValue<Vector2>().x, _actions.Player.Move.ReadValue<Vector2>().y);
    }

    public IEnumerator WalkIntoNewScene(Vector2 exitDir, float delay)
    {
        if (exitDir.y > 0f)
        {
            _rb.velocity = _jumpForce * exitDir;
        }

        if (exitDir.x != 0f)
        {
            _horizontalInput = exitDir.x > 0 ? 1f : -1f;

            MoveCharacter();
        }

        Flip();

        yield return new WaitForSeconds(delay);
        _stats._enteringScene = false;
    }

    public void AddExtraJumps(int value)
    {
        _extraJumpsValue += value;
    }

    public void AddExtraDash()
    {
        _hasDashed = false;
    }

    private void OnDrawGizmos()
    {
        if (_onGround)
        {
            Gizmos.color = Color.red;
        }
        else if (_canCornerCorrect)
        {
            Gizmos.color = Color.yellow;
        }
        else if (_onWall)
        {
            Gizmos.color = Color.blue;
        }
        else
        {
            Gizmos.color = Color.green;
        }
        //Ground Check
        Gizmos.DrawRay(transform.position + _groundRaycastOffset, transform.position + _groundRaycastOffset + Vector3.down * _groundRaycastLength);
        Gizmos.DrawRay(transform.position - _groundRaycastOffset, transform.position - _groundRaycastOffset + Vector3.down * _groundRaycastLength);

        //Corner Check
        Gizmos.DrawLine(transform.position + _edgeRaycastOffset, transform.position + _edgeRaycastOffset + Vector3.up * _topRaycastLength);
        Gizmos.DrawLine(transform.position - _edgeRaycastOffset, transform.position - _edgeRaycastOffset + Vector3.up * _topRaycastLength);
        Gizmos.DrawLine(transform.position + _innerRaycastOffset, transform.position + _innerRaycastOffset + Vector3.up * _topRaycastLength);
        Gizmos.DrawLine(transform.position - _innerRaycastOffset, transform.position - _innerRaycastOffset + Vector3.up * _topRaycastLength);

        //Corner Distance Check

        Gizmos.DrawLine(transform.position - _innerRaycastOffset + Vector3.up * _topRaycastLength,
                        transform.position - _innerRaycastOffset + Vector3.up * _topRaycastLength + Vector3.left * _topRaycastLength);
        Gizmos.DrawLine(transform.position + _innerRaycastOffset + Vector3.up * _topRaycastLength,
                        transform.position + _innerRaycastOffset + Vector3.up * _topRaycastLength + Vector3.right * _topRaycastLength);

        //Wall Check
        Gizmos.DrawRay(transform.position, transform.position + Vector3.right * _wallRaycastLength);
        Gizmos.DrawRay(transform.position, transform.position + Vector3.left * _wallRaycastLength);
    }
}
