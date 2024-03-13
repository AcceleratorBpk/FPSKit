using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class ReloadWeapon : MonoBehaviour
{
    public Animator rigController;
    public WeaponAnimationEvents animationEvents;
    public ActiveWeapon activeWeapon;
    public Transform leftHand;
    public GameObject handMagazine;
    public AmmoWidget ammoWidget;
    public bool isReloading;
    
    [ReadOnly]
    public RaycastWeapon weapon;


    
    // Start is called before the first frame update
    void Start()
    {
        animationEvents.WeaponAnimationEvent.AddListener(OnAnimationEvent);
    }

    // Update is called once per frame
    void Update()
    {
        if (weapon)
        {
            if (Input.GetKeyDown(KeyCode.R) || weapon.ShouldReload())
            {
                isReloading = true; 
                rigController.SetTrigger("reload_weapon");
             }
        if (weapon.isFiring)
            {
                ammoWidget.Refresh(weapon.ammoCount,weapon.clipCount);
            }
        }

      
    }

    void OnAnimationEvent(string eventName)
    {
        switch (eventName)
        {
            case "detach_magazine":
                DetachMag();
                break;
            case "drop_magazine":
                DropMag();
                break;
            case "refill_magazine":
                RefillMag();
                break;
            case "attach_magazine":
                AttachMag();
                break;
        }
    }

    private void AttachMag()
    {
        weapon = activeWeapon.GetActiveWeapon();
      
        weapon.magazine.SetActive(true);
        Destroy(handMagazine);
        weapon.RefillAmmo();
        rigController.ResetTrigger("reload_weapon");
        if(ammoWidget!=null)
        ammoWidget.Refresh(weapon.ammoCount,weapon.clipCount);
        isReloading = false;
    }

    private void RefillMag()
    {
        handMagazine.SetActive(true);
    }

    private void DropMag()
    {
        GameObject droppedMagazine =
            Instantiate(handMagazine, handMagazine.transform.position, handMagazine.transform.rotation);
        droppedMagazine.AddComponent<Rigidbody>();
        droppedMagazine.AddComponent<BoxCollider>();
        //隐藏的是手上的
        handMagazine.SetActive(false);
    }

    private void DetachMag()
    {
        weapon = activeWeapon.GetActiveWeapon();
        handMagazine = Instantiate(weapon.magazine, leftHand, true);
        //隐藏的是武器上的
        weapon.magazine.SetActive(false);
    }
}