using System;
using UnityEngine;


public class CharacterAiming : MonoBehaviour
{
    public float turnSpeed = 15;
    public float aimDuration=0.3f;
    public Cinemachine.AxisState xAxis;
     public Cinemachine.AxisState yAxis;
     public Transform cameraLookAt;
     
    private Camera _camera;
    private Animator _animator;
    private ActiveWeapon _activeWeapon;
    public bool isAiming;
    
    private int isAimingParam = Animator.StringToHash("isAiming");

    private RaycastWeapon weapon;
    
    // Start is called before the first frame update
    void Start()
    {
        _camera = Camera.main;
        _animator = GetComponent<Animator>();
        _activeWeapon = GetComponent<ActiveWeapon>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
       
    }

    private void Update()
    {
        float yawCamera = _camera.transform.rotation.eulerAngles.y;
        transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.Euler(0,yawCamera,0),turnSpeed*Time.deltaTime
        );
        
        isAiming = Input.GetMouseButton(1);
        _animator.SetBool(isAimingParam,isAiming);

        weapon = _activeWeapon.GetActiveWeapon();
        if (weapon)
        {
            weapon.recoil.recoilModifier = isAiming ? 0.4f : 1.0f;
        }
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        xAxis.Update(Time.smoothDeltaTime);
        yAxis.Update(Time.smoothDeltaTime);
        cameraLookAt.eulerAngles = new Vector3(yAxis.Value, xAxis.Value, 0);
    }


}
