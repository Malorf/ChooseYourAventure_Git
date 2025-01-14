using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyKnightDog : EnemyAi
{
    public static int NumberChienvalierQuest;
    void Start()
    {
        agent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
        animations = gameObject.GetComponent<Animator>();
        collider = GetComponent<CapsuleCollider>();
        attackTime = Time.time;
        waypointIndex = 0;
        transform.LookAt(points[waypointIndex].position);
        basePositions = transform.position;
        hpImage.enabled = false;
        backgroundHp.enabled = false;
        hpEnemy = hpMax;
    }



    void Update()
    {

        float percentageHp = ((hpEnemy * 100) / hpMax) / 100; //barre de vie
        hpImage.fillAmount = percentageHp;

        if (!isDead)
        {

            // On cherche le joueur en permanence
            Target = GameObject.Find("Player").transform;

            // On calcule la distance entre le joueur et l'ennemi, en fonction de cette distance on effectue diverses actions
            Distance = Vector3.Distance(Target.position, transform.position);

            // On calcule la distance entre l'ennemi et sa position de base
            DistanceBase = Vector3.Distance(basePositions, transform.position);

            // Quand on s'enfuit apr�s avoir tap� l'ennemi
            if (Distance > chaseRange * 2)
            {
                if (hpEnemy != hpMax)
                {
                    backgroundHp.enabled = false;
                    hpImage.enabled = false;
                    BackBase();
                }
            }
            //Patrouille de base du monstre
            if (Distance > chaseRange)
            {
                if (hpEnemy == hpMax)
                {
                    transform.LookAt(points[waypointIndex].position);
                    dist = Vector3.Distance(transform.position, points[waypointIndex].position);
                    walk();
                    if (dist < 1f)
                    {
                        IncreaseIndex();
                    }
                    Patrol();
                }
            }

            // Quand l'ennemi est proche mais pas assez pour attaquer
            if (Distance < chaseRange && Distance > attackRange)
            {
                backgroundHp.enabled = true;
                hpImage.enabled = true;
                chase();
            }

            // Quand l'ennemi est assez proche pour attaquer
            if (Distance < attackRange)
            {
                backgroundHp.enabled = true;
                hpImage.enabled = true;
                attack();
            }

            //quand le monstre se fait taper de loin
            if (Distance > chaseRange && Distance <= 2 * chaseRange)
            {
                if (hpEnemy != hpMax)
                {
                    chase();
                }
            }
        }

    }
    protected override void CreateEnnemy()
    {
        GameObject clone = Instantiate(PrefabToSpawn, transform.position, Quaternion.identity);
        clone.GetComponent<EnemyAi>().PrefabToSpawn = PrefabToSpawn;
        clone.GetComponent<EnemyAi>().points = points;
    }
    protected override void chase()
    {
        agent.enabled = true;
        animations.Play("RunForwardBattle");
        agent.destination = Target.position;
    }
    protected override void BackBase()
    {
        animations.Play("RunForwardBattle");
        agent.destination = basePositions;
        StartCoroutine(TimeBeforeBase());
    }

    protected override void walk()
    {
        animations.Play("WalkForwardBattle");
    }
    protected override void attack()
    {
        agent.destination = transform.position;

        if (Time.time > attackTime)
        {
            animations.Play("Attack01");
            Target.GetComponent<PlayerInventory>().ApplyDamage(TheDammage);
            attackTime = Time.time + attackRepeatTime;
        }
    }
    protected override void Dead()
    {
        isDead = true;
        {
            StartCoroutine(Respawn());
        }
        backgroundHp.enabled = false;
        collider.enabled = false;
        animations.Play("Die");
        Experience();
        Destroy(transform.gameObject, 61);

    }
    public override void ApplyDammage(float TheDammage)
    {
        if (!isDead)
        {
            hpEnemy = hpEnemy - TheDammage;
            animations.Play("GetHit");
            if (hpEnemy <= 0)
            {
                Dead();
                if (DialogueBlackSmith.QuestChienvalier == true)
                {
                    NumberChienvalierQuest += 1;
                }
            }
        }
    }
}