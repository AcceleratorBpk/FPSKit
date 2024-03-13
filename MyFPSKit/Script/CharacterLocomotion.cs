using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterLocomotion : MonoBehaviour
{
    public Animator _rigAnimator;
    
    public float jumpHeight;
    public float gravity;
    public float stepDown;
    public float airControl;
    public float jumpDamp;
    public float groundSpeed;

    public float pushPower;
    
    private Animator _animator;
    private CharacterController cc;
    private ActiveWeapon _activeWeapon;
    private ReloadWeapon _reloadWeapon;

    private CharacterAiming characterAiming;
    
    private Vector2 input;

    private Vector3 rootMotion;

    private Vector3 velocity;
    private bool isJumping;

    private int isSprintingParam = Animator.StringToHash("isSprinting");
    
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
        _activeWeapon = GetComponent<ActiveWeapon>();
        _reloadWeapon = GetComponent<ReloadWeapon>();
        characterAiming = GetComponent<CharacterAiming>();
    }

    // Update is called once per frame
    void Update()
    {
        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");
        _animator.SetFloat("InputX",input.x);
        _animator.SetFloat("InputY",input.y);

        UpdateIsSprinting();
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
        
    }

    bool IsSprinting()
    {
        bool isSprinting= Input.GetKey(KeyCode.LeftShift);
        bool isFiring = _activeWeapon.IsFiring();
        bool isReloading = _reloadWeapon.isReloading;
        bool isChangingWeapon = _activeWeapon.isChangingWeapon;
        bool isAiming = characterAiming.isAiming;
        return isSprinting && !isFiring && !isReloading && !isChangingWeapon&&!isAiming;
    }

    private void UpdateIsSprinting()
    {
        bool isSprinting = IsSprinting();
            _animator.SetBool(isSprintingParam,isSprinting);
        _rigAnimator.SetBool(isSprintingParam,isSprinting);
    }

    private void OnAnimatorMove()
    {
        rootMotion += _animator.deltaPosition;
    }

    private void FixedUpdate()
    {
        if (isJumping) //is in AirState
        {
            UpdateInAir();
        }
        else //isGrounded state
        {
            UpdateOnGround();
        }
    }

    private void UpdateOnGround()
    {
        Vector3 stepForwardAmount = rootMotion*groundSpeed;
        Vector3 stepDownAmount = Vector3.down*stepDown;
        cc.Move(stepForwardAmount+stepDownAmount);
        rootMotion = Vector3.zero;
        if (!cc.isGrounded)
        {
           SetInAir(0);
        }
    }

    private void UpdateInAir()
    {
        velocity.y -= gravity * Time.fixedDeltaTime;
        Vector3 displacement = velocity * Time.fixedDeltaTime;
        displacement += CalculateAirControl();
        cc.Move(displacement);
        isJumping = !cc.isGrounded;
        rootMotion = Vector3.zero;
        _animator.SetBool("isJumping",isJumping);
    }

    void Jump()
    {
        if (!isJumping)
        {
            float jumpVelocity = MathF.Sqrt(2 * gravity * jumpHeight);
            SetInAir(jumpVelocity);
        }
    }

    private void SetInAir(float jumpVelocity)
    {
        isJumping = true;
        //v方=2gh
        velocity =jumpDamp*groundSpeed* _animator.velocity;
        velocity.y = jumpVelocity;
        _animator.SetBool("isJumping",true);
    }

    //计算空中移动
    Vector3 CalculateAirControl()
    {
        return ((transform.forward * input.y) + (transform.right * input.x))*airControl/100;
    }
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // no rigidbody
        if (body == null || body.isKinematic)
            return;

        // We dont want to push objects below us
        if (hit.moveDirection.y < -0.3f)
            return;

        // Calculate push direction from move direction,
        // we only push objects to the sides never up and down
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // If you know how fast your character is trying to move,
        // then you can also multiply the push velocity by that.

        // Apply the push
        body.velocity = pushDir * pushPower;
    }
}
