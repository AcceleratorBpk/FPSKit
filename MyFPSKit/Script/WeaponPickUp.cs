using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickUp : MonoBehaviour
{
   public RaycastWeapon weaponFab;


   private void OnTriggerEnter(Collider other)
   {
      ActiveWeapon activeWeapon = other.gameObject.GetComponent<ActiveWeapon>();
      if (activeWeapon)
      {
         RaycastWeapon newWeapon = Instantiate(weaponFab);
         activeWeapon.Equip(newWeapon);
         Destroy(gameObject);
         return;
      }
      AIWeapon weapon = other.gameObject.GetComponent<AIWeapon>();
      if (weapon)
      {
      RaycastWeapon newWeapon = Instantiate(weaponFab);
      weapon.Equip(newWeapon);
      Destroy(gameObject);
      return;
      }
         
   }  
      
}

