using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class RaycastWeapon : MonoBehaviour
{
    class Bullet
    {
        public float time;
        public Vector3 initialPosition;
        public Vector3 initialVelocity;
        public TrailRenderer tracer;
        public int bounce;
    }

    public ActiveWeapon.WeaponSlot weaponSlot;

    public bool isFiring = false;

    public int fireRate = 25;
    public float bulletSpeed = 1000.0f;
    public float bulletDrop = 0.0f;
    public int maxBounce = 0;


    public RuntimeAnimatorController animator;
    public ParticleSystem[] particleSystems;
    public ParticleSystem hitEffect;
    public TrailRenderer tracerEffect;

    public string weaponName;
    public LayerMask layerMask;
    public int ammoCount = 30;
    public int clipSize = 30;
    public float damage = 20f;
    public int clipCount = 2;


    public Transform raycastOrigin;


    public WeaponRecoil recoil;
    public GameObject magazine;

    private Ray ray;
    private RaycastHit hitInfo;
    private float accumulatedTime;

    private List<Bullet> bullets = new List<Bullet>();

    private float maxLiftTime = 3.0f;
    //计算子弹下坠
    private void Awake()
    {
        recoil = GetComponent<WeaponRecoil>();
    }

    Vector3 GetPosition(Bullet bullet)
    {
        //p+v*t + 0.5 g*t*t
        //子弹下坠
        Vector3 gravity = Vector3.down * bulletDrop;
        return bullet.initialPosition + bullet.initialVelocity * bullet.time + 0.5f * gravity * bullet.time * bullet.time;
    }

    Bullet CreateBullet(Vector3 position, Vector3 velocity)
    {
        Bullet bullet = new Bullet();
        bullet.initialPosition = position;
        bullet.initialVelocity = velocity;
        bullet.time = 0.0f;
        bullet.tracer = Instantiate(tracerEffect, position, quaternion.identity);
        bullet.tracer.AddPosition(ray.origin);
        bullet.bounce = maxBounce;
        return bullet;
    }

    public void StartFiring()
    {
        isFiring = true;
        // accumulatedTime = 0.0f;
        if (accumulatedTime > 0.0f)
        {
            accumulatedTime = 0.0f;
        }
        recoil.Reset();
    }
    public void StopFiring()
    {
        isFiring = false;
    }
    public void UpdateWeapon(float deltaTime, Vector3 targetPOS)
    {
        if (isFiring)
        {
            UpdateFiring(deltaTime, targetPOS);
        }

        accumulatedTime += deltaTime;
        UpdateBullets(deltaTime);
    }

    public void UpdateFiring(float deltaTime, Vector3 targetPOS)
    {
        float fireInterval = 1.0f / fireRate;
        while (accumulatedTime >= 0.0f)
        {
            FireBullet(targetPOS);
            accumulatedTime -= fireInterval;
        }
    }

    public void UpdateBullets(float deltaTime)
    {
        SimulateBullets(deltaTime);
        DestroyBullets();
    }

    private void DestroyBullets()
    {
        bullets.RemoveAll(bullet => bullet.time >= maxLiftTime);
    }

    //他这样算，好像子弹射线是一段、一段的。基于deltaTime
    void SimulateBullets(float deltaTime)
    {
        bullets.ForEach(bullet =>
        {
            Vector3 p0 = GetPosition(bullet);
            bullet.time += deltaTime;
            Vector3 p1 = GetPosition(bullet);
            RaycastSegment(p0, p1, bullet);
        });
    }

    //射线进程
    void RaycastSegment(Vector3 start, Vector3 end, Bullet bullet)
    {
        //一段射线初始化

        Vector3 direction = end - start;
        float distance = direction.magnitude;
        ray.origin = start;
        ray.direction = direction;

        //如果击中后
        if (Physics.Raycast(ray, out hitInfo, distance, layerMask))
        {
            //  Debug.DrawRay(ray.origin,hitInfo.point,Color.red,1.0f);
            hitEffect.transform.position = hitInfo.point;
            hitEffect.transform.forward = hitInfo.normal;
            hitEffect.Emit(6);

            bullet.tracer.transform.position = hitInfo.point;
            bullet.time = maxLiftTime;

            if (bullet.bounce > 0)
            {
                bullet.time = 0;
                bullet.initialPosition = hitInfo.point;
                bullet.initialVelocity = Vector3.Reflect(bullet.initialVelocity, hitInfo.normal);
                bullet.bounce--;
            }

            var rb2d = hitInfo.collider.GetComponent<Rigidbody>();
            if (rb2d)
            {
                rb2d.AddForceAtPosition(ray.direction * 20, hitInfo.point, ForceMode.Impulse);
            }

            var hitBox = hitInfo.collider.GetComponent<HitBox>();
            if (hitBox)
            {
                hitBox.OnRaycastHit(this, ray.direction);
            }
        }
        else
        {
            bullet.tracer.transform.position = end;
        }
    }

    public void FireBullet(Vector3 target)
    {
        if (ammoCount <= 0)
        {
            return;
        }

        ammoCount--;
        foreach (var _particleSystem in particleSystems)
        {
            _particleSystem.Emit(1);
        }

        Vector3 velocity = (target - raycastOrigin.position).normalized * bulletSpeed;
        var bullet = CreateBullet(raycastOrigin.position, velocity);

        bullets.Add(bullet);

        recoil.GenerateRecoil(weaponName);

    }
    public bool ShouldReload()
    {
        return ammoCount == 0 && clipCount > 0;
    }
    public bool IsLowAmmo()
    {
        return ammoCount == 0 && clipCount == 0;
    }
    public void RefillAmmo()
    {
        ammoCount = clipSize;
        clipCount--;
    }
}
