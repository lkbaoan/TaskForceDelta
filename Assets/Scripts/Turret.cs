using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Turret : MonoBehaviour
{
    BuildManager buildManager;
    [SerializeField] private GameObject bullet;
    private Transform target;
    private float fireCountdown = 0.0f;

    [Header("Turret Stat")]
    [SerializeField] private int hitPoint = 30;
    [SerializeField] private int shield = 30;
    [SerializeField] private float range = 10f;
    [SerializeField] private int damage = 100;
    [SerializeField] private float fireRate = 1.0f;
    [SerializeField] private bool canSee = false;

    [Header("Upgrade Stat")]
    [SerializeField] private int upDamage = 5;
    [SerializeField] private float upFireRate = 0.5f;
    [SerializeField] private int upShield = 10;
    [SerializeField] private int[] upDamageCost = { 30, 50, 80 };
    [SerializeField] private int[] upFireRateCost = { 50, 80, 100 };
    [SerializeField] private int[] upShieldCost = { 30, 50, 80 };
    private const int MAX_LEVEL = 3;
    private int lvDamage = 0;
    private int lvFireRate = 0;
    private int lvShield = 0;


    void Start()
    {
        buildManager = BuildManager.instance;
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.5f);
    }
    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDistance = Mathf.Infinity;
        GameObject closestEnemy = null;
        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
            // Can choose target if turret can see, or target can be seen
            bool canChoose = canSee ? canSee : enemy.GetComponent<Enemy>().BeSeen;

            if (distanceToEnemy < shortestDistance && canChoose)
            {
                shortestDistance = distanceToEnemy;
                closestEnemy = enemy;
            }
        }
        if (closestEnemy != null && shortestDistance <= range)
        {
            target = closestEnemy.transform;
        }
        else
        {
            target = null;
        }
    }
    void Update()
    {
        if (target == null)
        {
            return;
        }
        if (fireCountdown <= 0)
        {
            Fire();
            fireCountdown = 1f / fireRate;
        }
        fireCountdown -= Time.deltaTime;
    }
    // TODO: bullet spawn location needed to change.
    void Fire()
    {
        GameObject bulletSpawn = Instantiate(bullet, transform.position, Quaternion.identity);
        Bullet bulletProp = bulletSpawn.GetComponent<Bullet>();

        if (bullet != null)
        {
            bulletProp.Damage = damage;
            bulletProp.CanSee = canSee;
            bulletProp.TargetType = "Enemy";
            bulletProp.SetTarget(target);
        }
    }
    public void GetDamaged(int damage)
    {
        hitPoint -= damage;
        if (hitPoint <= 0)
        {
            Destroy(gameObject);
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
    public void UpgradeDamage()
    {
        if (lvDamage < MAX_LEVEL)
        {
            if (Base.Money < upDamageCost[lvDamage])
            {
                Debug.Log("Not enough money");
                return;
            }
            Base.Money -= upDamageCost[lvDamage];
            damage += upDamage;
            lvDamage++;
        }
    }
    public void UpgradeFireRate()
    {
        if (lvFireRate < MAX_LEVEL)
        {
            if (Base.Money < upFireRateCost[lvFireRate])
            {
                Debug.Log("Not enough money");
                return;
            }
            Base.Money -= upFireRateCost[lvFireRate];
            fireRate += upFireRate;
            lvFireRate++;
        }
    }
    public void UpgradeShield()
    {
        if (lvShield < MAX_LEVEL)
        {
            if (Base.Money < upShieldCost[lvShield])
            {
                Debug.Log("Not enough money");
                return;
            }
            Base.Money -= upShieldCost[lvShield];
            shield += upShield;
            lvShield++;
        }
    }
    void OnMouseDown()
    {
        buildManager.SelectTurret(this);
    }
}
