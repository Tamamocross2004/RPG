using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed;
    public float attackRange = 2;
    public float attackCooldown = 2;
    public float playerDetectRange = 5;
    public Transform detectionPoint;
    public LayerMask playerLayer;

    private float attackCooldownTimer;
    private int facingDirection = -1; // 表示初始面向右边
    private EnemyState enemystate;


    private Rigidbody2D rb;
    private Transform player;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        ChangeState(EnemyState.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        CheckForPlayer();
        if(attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        if(enemystate == EnemyState.Chasing)
        {
            Chase();
        }
        else if(enemystate == EnemyState.Attacking)
        {
            rb.velocity = Vector2.zero;
        }
    }

    void Chase()
    {
        if((player.position.x > transform.position.x && facingDirection == -1) ||
            (player.position.x < transform.position.x && facingDirection == 1))
        {
            Flip();
        }

        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * speed;
    }

    void Flip()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(transform.localScale.x * (-1), transform.localScale.y, transform.localScale.z);
    }

    private void CheckForPlayer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(detectionPoint.position, playerDetectRange, playerLayer);

        if(hits.Length > 0)
        {
            player = hits[0].transform;

            // 检查玩家是否在攻击范围内, 以及冷却时间是否就绪
            if(Vector2.Distance(transform.position, player.position) <= attackRange && attackCooldownTimer <= 0)
            {
                attackCooldownTimer = attackCooldown;
                ChangeState(EnemyState.Attacking);
            }

            // 若距离大于攻击范围，进入追逐状态
            else if(Vector2.Distance(transform.position, player.position) > attackRange)
            {
                ChangeState(EnemyState.Chasing);
            }
        }
        else
        {
            rb.velocity = Vector2.zero;
            ChangeState(EnemyState.Idle);
        }
    }


    void ChangeState(EnemyState newState)
    {
        // 退出当前动画
        if(enemystate == EnemyState.Idle)
        {
            anim.SetBool("isIdle", false);
        }
        else if (enemystate == EnemyState.Chasing)
        {
            anim.SetBool("isChasing", false);
        }
        else if (enemystate == EnemyState.Attacking)
        {
            anim.SetBool("isAttacking", false);
        }

        //更新当前状态 
        enemystate = newState;

        // 更新动画
        if(enemystate == EnemyState.Idle)
        {
            anim.SetBool("isIdle", true);
        }
        else if (enemystate == EnemyState.Chasing)
        {
            anim.SetBool("isChasing", true);
        }
        else if (enemystate == EnemyState.Attacking)
        {
            anim.SetBool("isAttacking", true);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(detectionPoint.position, playerDetectRange);
    }
}

public enum EnemyState
{
    Idle,
    Chasing,
    Attacking
}