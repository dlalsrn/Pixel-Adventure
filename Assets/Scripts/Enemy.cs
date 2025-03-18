using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Rigidbody2D rigidbody2d;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private BoxCollider2D boxCollider2d;

    private float moveSpeed = 1.5f;
    private int nextDir;

    private float lastChangeTime; // 마지막으로 방향을 바꾼 시각
    private float moveTime; // 특정 방향으로 움직이는 시간

    private bool isGround;
    private bool isDeath = false;

    private void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        boxCollider2d = GetComponent<BoxCollider2D>();
        ChangeMove();
    }

    private void Update()
    {
        if (isDeath) // 죽었으면 Update X
        {
            return;
        }

        if (Time.time - lastChangeTime >= moveTime)
        {
            ChangeMove();
        }

        // 앞에 Platform이 있는지 확인. false일 경우 낭떠러지임
        isGround = Physics2D.Raycast(transform.position + (Vector3.right * nextDir * 0.5f), Vector2.down, 1.5f, LayerMask.GetMask("Platform"));

        if (!isGround)
        {
            nextDir *= -1;
            Flip(); // 방향 전환
            ChangeMoveTime(); // 낭떠러지라서 방향을 바꿨을 경우 움직이는 시간을 재설정
        }
    }

    private void FixedUpdate()
    {
        rigidbody2d.velocity = new Vector2(moveSpeed * nextDir, rigidbody2d.velocity.y);
    }

    private void ChangeMove() // 움직이는 방향 결정
    {
        nextDir = Random.Range(-1, 2); // 왼쪽, 정지, 오른쪽 방향
        animator.SetInteger("isWalking", nextDir);
        Flip(); // 방향 전환
        ChangeMoveTime();
    }

    private void ChangeMoveTime() // 움직일 시간 변경
    {
        lastChangeTime = Time.time;
        moveTime = Random.Range(2f, 4f);
    }

    private void Flip() // 방향 전환
    {
        Vector3 currentScale = transform.localScale;
        if (nextDir == -1) 
        {
            transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
        else if (nextDir  == 1)
        {
            transform.localScale = new Vector3(Mathf.Abs(currentScale.x) * (-1f), currentScale.y, currentScale.z);
        }
    }

    public void OnDamaged() // Player에게 밟혔을 때 호출되는 함수, GameObject 삭제
    {
        spriteRenderer.color = new Color(1f, 1f, 1f, 0.4f);
        spriteRenderer.flipY = true;
        boxCollider2d.enabled = false;
        moveSpeed = 0f;
        isDeath = true;
        animator.SetInteger("isWalking", 0); // Idle 상태로 변경

        // 밟히고 나서 살짝 Jump
        rigidbody2d.AddForce(Vector2.up * 3f, ForceMode2D.Impulse);

        Destroy(gameObject, 3f);
    }
}
