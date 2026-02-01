using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    public float facingDirection = 1;
    public Rigidbody2D rb;
    public Animator anim;

    public PlayerCombat playerCombat;

    private void Update()
    {
        if(Input.GetButtonDown("Slash") && playerCombat.enabled == true)
        {
            playerCombat.Attack();
        }
    }

    // 使用与 Cinemachine Confiner 相同的 Polygon Collider 2D
    public PolygonCollider2D boundaryCollider;

    // Tilemap 地图边界
    public Tilemap mapTilemap;

    private bool isKnockedBack;
    public bool isShooting;

    // FixedUpdate is called 50x per frame
    void FixedUpdate()
    {
        if(isShooting == true)
        {
            rb.velocity = Vector2.zero;
        }

        else if(isKnockedBack == false)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            if((horizontal > 0 && transform.localScale.x < 0) || 
                (horizontal < 0 && transform.localScale.x > 0))
            {
                Flip();
            }

            anim.SetFloat("horizontal", Mathf.Abs(horizontal));
            anim.SetFloat("vertical", Mathf.Abs(vertical));

            Vector2 movement = new Vector2(horizontal, vertical) * StatsManager.Instance.speed;
            Vector2 currentPosition = rb.position;
            Vector2 newPosition = currentPosition + movement * Time.fixedDeltaTime;
            
            // 限制玩家位置在边界内（在移动前检查，避免闪回）
            if (mapTilemap != null)
            {
                newPosition = ClampToTilemapBounds(currentPosition, newPosition);
            }
            // 限制玩家位置在边界内（使用与 Cinemachine 相同的边界）
            else if (boundaryCollider != null)
            {
                newPosition = ClampToColliderBounds(currentPosition, newPosition);
            }
            
            rb.MovePosition(newPosition);
        }

    }

    Vector2 ClampToTilemapBounds(Vector2 currentPos, Vector2 targetPos)
    {
        if (mapTilemap == null) return targetPos;
        
        // 获取实际有瓦片的单元格范围
        BoundsInt cellBounds = mapTilemap.cellBounds;
        
        // 获取 Grid 组件
        Grid grid = mapTilemap.layoutGrid;
        if (grid == null) return targetPos;
        
        // 将世界坐标转换为单元格坐标
        Vector3Int currentCell = grid.WorldToCell(currentPos);
        Vector3Int targetCell = grid.WorldToCell(targetPos);
        
        // 检查当前位置是否在有效范围内
        bool currentInBounds = cellBounds.Contains(currentCell) && mapTilemap.GetTile(currentCell) != null;
        
        // 检查目标位置是否在有效范围内
        bool targetInBounds = cellBounds.Contains(targetCell) && mapTilemap.GetTile(targetCell) != null;
        
        // 如果目标位置无效，只允许在边界内的移动
        if (!targetInBounds)
        {
            // 分别检查 X 和 Y 方向的移动
            Vector2 clampedPos = currentPos;
            
            // 检查 X 方向移动
            Vector2 testPosX = new Vector2(targetPos.x, currentPos.y);
            Vector3Int testCellX = grid.WorldToCell(testPosX);
            if (cellBounds.Contains(testCellX) && mapTilemap.GetTile(testCellX) != null)
            {
                clampedPos.x = targetPos.x;
            }
            
            // 检查 Y 方向移动
            Vector2 testPosY = new Vector2(clampedPos.x, targetPos.y);
            Vector3Int testCellY = grid.WorldToCell(testPosY);
            if (cellBounds.Contains(testCellY) && mapTilemap.GetTile(testCellY) != null)
            {
                clampedPos.y = targetPos.y;
            }
            
            return clampedPos;
        }
        
        return targetPos;
    }

    Vector2 ClampToColliderBounds(Vector2 currentPos, Vector2 targetPos)
    {
        if (boundaryCollider == null) return targetPos;
        
        Bounds bounds = boundaryCollider.bounds;
        
        // 如果目标位置在边界内，允许移动
        if (bounds.Contains(targetPos))
        {
            return targetPos;
        }
        
        // 如果目标位置超出边界，只允许在边界内的移动
        Vector2 clampedPos = currentPos;
        
        // 检查 X 方向移动
        Vector2 testPosX = new Vector2(targetPos.x, currentPos.y);
        if (bounds.Contains(testPosX))
        {
            clampedPos.x = targetPos.x;
        }
        else
        {
            // 限制到边界
            clampedPos.x = Mathf.Clamp(targetPos.x, bounds.min.x, bounds.max.x);
        }
        
        // 检查 Y 方向移动
        Vector2 testPosY = new Vector2(clampedPos.x, targetPos.y);
        if (bounds.Contains(testPosY))
        {
            clampedPos.y = targetPos.y;
        }
        else
        {
            // 限制到边界
            clampedPos.y = Mathf.Clamp(targetPos.y, bounds.min.y, bounds.max.y);
        }
        
        return clampedPos;
    }

    void Flip()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(transform.localScale.x * (-1), transform.localScale.y, transform.localScale.z);
    }

    public void Knockback(Transform enemy, float force, float stunTime)
    {
        // 禁用玩家的移动
        isKnockedBack = true;

        // 击退
        Vector2 direction = (transform.position - enemy.position).normalized;
        rb.velocity = direction * force;
        StartCoroutine(KnockbackCounter(stunTime));
    }

    IEnumerator KnockbackCounter(float stunTime)
    {
        yield return new WaitForSeconds(stunTime);
        rb.velocity = Vector2.zero;
        isKnockedBack = false;
    }
}