using System.Collections;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations.Rigging;


public class ActiveWeapon : MonoBehaviour
{
    public enum WeaponSlot
    {
        Primary = 0,
        Secondary = 1,
    }

    public Transform crossHairTarget;
    public Rig handIK;
    public Transform[] weaponSlots;

    public CharacterAiming  characterAiming;
    public AmmoWidget ammoWidget;
    public bool isChangingWeapon;
    private RaycastWeapon[] _raycastWeapon = new RaycastWeapon[2];
    private int activeWeaponIndex;

    private bool isHolstered = false;

    public Animator RigAnimController;

    public ReloadWeapon reloadWeapon;
   [ReadOnly] public RaycastWeapon raycastWeapon;
    private void Start()
    {

        RaycastWeapon existingWeapon = GetComponentInChildren<RaycastWeapon>();
        characterAiming = GetComponent<CharacterAiming>();
        if (existingWeapon)
        {
            Equip(existingWeapon);
        }
    }

    public bool IsFiring()
    {
        RaycastWeapon currentWeapon = GetActiveWeapon();
        if (!currentWeapon)
        {
            return false;
        }

        return currentWeapon.isFiring;
    }
    
    public RaycastWeapon GetActiveWeapon()
    {
        return GetWeapon(activeWeaponIndex);
    }

RaycastWeapon GetWeapon(int index)
    {
        if (index < 0 || index >= _raycastWeapon.Length)
        {
            return null;
        }
        return _raycastWeapon[index];
    }
    private void Update()
    {
        //if (!raycastWeapon) return;
        raycastWeapon = GetWeapon(activeWeaponIndex);
        bool notSprinting = RigAnimController.GetCurrentAnimatorStateInfo(2).shortNameHash==Animator.StringToHash("notSprinting");
        bool canFire = !isHolstered && notSprinting && !reloadWeapon.isReloading;
        if (raycastWeapon )
        {
            if (Input.GetButton("Fire1") && !raycastWeapon.isFiring&& canFire)
            {
                raycastWeapon.StartFiring();
            }

            if (Input.GetButtonUp("Fire1") || !canFire)
            {
                raycastWeapon.StopFiring();
            }
            raycastWeapon.UpdateWeapon(Time.deltaTime,crossHairTarget.position);
            // if (Input.GetButtonDown("Fire1"))
            // {
            //     raycastWeapon.StartFiring();
            // }
            //
            // if (raycastWeapon.isFiring)
            // {
            //     raycastWeapon.UpdateFiring(Time.deltaTime);
            // }
            //
            // raycastWeapon.UpdateBullets(Time.deltaTime);
            // if (Input.GetButtonUp("Fire1"))
            // {
            //     raycastWeapon.StopFiring();
            // }
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            ToggleActiveWeapon();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetActiveWeapon(WeaponSlot.Primary);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetActiveWeapon(WeaponSlot.Secondary);
        }
    }

    public void Equip(RaycastWeapon newWeapon)
    {
        int weaponSlotIndex =(int) newWeapon.weaponSlot;
        var weapon=GetWeapon(weaponSlotIndex);
        if (weapon)
        {
            Destroy(weapon.gameObject);
        }
        weapon = newWeapon;
      
        weapon.recoil.characterAiming = characterAiming;
        weapon.recoil.rigAnimController = RigAnimController;
        var raycastTransform = weapon.transform;
        raycastTransform.SetParent(weaponSlots[weaponSlotIndex], false);
        handIK.weight = 1;

        //捡到哪个武器，放入数组，然后赋值激活序号
        _raycastWeapon[weaponSlotIndex] = weapon;
        SetActiveWeapon(newWeapon.weaponSlot);
        reloadWeapon.weapon = GetActiveWeapon();
        raycastWeapon = GetActiveWeapon();
        ammoWidget.Refresh(weapon.ammoCount, weapon.clipCount);
        
    }

    void ToggleActiveWeapon()
    {
        bool isHolstered = RigAnimController.GetBool("holster_weapon");
        if (isHolstered)
        {
            StartCoroutine(ActiveateWeapon(activeWeaponIndex));
        }
        else
        {
            StartCoroutine(HolsterWeapon(activeWeaponIndex));
        }
    }
    
    void SetActiveWeapon(WeaponSlot weaponSlot)
    {
        int holsterIndex = activeWeaponIndex;
        int activeIndex = (int)weaponSlot;

        if (holsterIndex == activeIndex)
        {
            holsterIndex = -1;//使getweapon返回null
        }
        
        StartCoroutine(SwitchWeapon(holsterIndex,activeIndex));
    }
    
    
    IEnumerator HolsterWeapon(int index)
    {
        isChangingWeapon = true;
        isHolstered = true;
        var weapon = GetWeapon(index);
        if (weapon)
        {
            RigAnimController.SetBool("holster_weapon",true);
                yield return new WaitForSeconds(0.1f);

            do
            {
                yield return new WaitForEndOfFrame();

            } while (RigAnimController.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f);
        }

        isChangingWeapon = false;
    }
    
    IEnumerator ActiveateWeapon(int index)
    { isChangingWeapon = true;
        var weapon = GetWeapon(index);
        if (weapon)
        {
            RigAnimController.SetBool("holster_weapon",false);
            RigAnimController.Play("WeaponGun_"+weapon.weaponName);
            yield return new WaitForSeconds(0.1f);
            do
            {
                yield return new WaitForEndOfFrame();

            } while (RigAnimController.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f);

            isHolstered = false; 
        }
        isChangingWeapon = false;
    }
    IEnumerator SwitchWeapon(int holsterIndex,int activeIndex)
    {
        RigAnimController.SetInteger("weaponIndex",activeIndex);
        yield return StartCoroutine(HolsterWeapon(holsterIndex));
        yield return StartCoroutine(ActiveateWeapon(activeIndex));
        activeWeaponIndex = activeIndex;
    }
    public void DropWeapon()
    {
        var currentWeapon = GetActiveWeapon();
        if (currentWeapon)
        {
            
            currentWeapon.transform.SetParent(null);
            currentWeapon.gameObject.GetComponent<BoxCollider>().enabled = true;
            currentWeapon.gameObject.AddComponent<Rigidbody>();
            _raycastWeapon[activeWeaponIndex] = null;
        }
    }
    public void RefillAmmo(int clipCount)
    {
        var currentWeapon = GetActiveWeapon();
        if (currentWeapon)
        {
            currentWeapon.clipCount += clipCount;
            ammoWidget.Refresh(currentWeapon.ammoCount,currentWeapon.clipCount);
        }
    }

    ///IK控件保存到动画文件当中
    // public Transform weaponLeftGrip;
    // public Transform weaponRightGrip;
    // [Button]
    // void SaveWeaponPose()
    // {
    //     ///保存记录动画文件
    //     GameObjectRecorder recorder = new GameObjectRecorder(gameObject);
    //     recorder.BindComponentsOfType<Transform>(weaponParent.gameObject,false);
    //     recorder.BindComponentsOfType<Transform>(weaponLeftGrip.gameObject,false);
    //     recorder.BindComponentsOfType<Transform>(weaponRightGrip.gameObject,false);
    //     recorder.TakeSnapshot(0.0f);
    //     recorder.SaveToClip(_raycastWeapon.weaponAnimation);
    //     UnityEditor.AssetDatabase.SaveAssets();
    // }
}
