using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponRecoil : MonoBehaviour
{
    [HideInInspector]public CharacterAiming characterAiming;
    [HideInInspector]public Cinemachine.CinemachineImpulseSource cameraShake;
    [HideInInspector] public Animator rigAnimController;
    public Vector2[] recoilPattern;
     float verticalRecoil;
     float horizontalRecoil;
    public float duration;

    public float recoilModifier = 1.0f;
    
    private float time;
    private int index;
    private void Awake()
    {
        cameraShake = GetComponent<Cinemachine.CinemachineImpulseSource>();
    }

    public void Reset()
    {
        index = 0;
    }

    int NextIndex(int index)
    {
        return (index + 1) % recoilPattern.Length;
    }
    public void GenerateRecoil(string weaponName)
    {
        time = duration;
        cameraShake.GenerateImpulse(Camera.main.transform.forward);
        horizontalRecoil= recoilPattern[index].x;
        verticalRecoil= recoilPattern[index].y;
        index = NextIndex(index);
        
        rigAnimController?.Play("WeaponRecoil_"+weaponName,1,0.0f);
    }

    private void Update()
    {
        if (time > 0)
        {
            characterAiming.yAxis.Value -=((verticalRecoil/10*Time.deltaTime)/duration)*recoilModifier;
            characterAiming.xAxis.Value -= ((horizontalRecoil/10*Time.deltaTime)/duration)*recoilModifier;
            time -= Time.deltaTime;
        }
    }
}
