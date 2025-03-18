using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Jobs;

public class Player : MonoBehaviour
{
    private Rigidbody2D rigidbody2d;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private BoxCollider2D boxCollider2d;

    private AudioSource audioSource;
    [SerializeField] private AudioClip audioJump; // 점프 Sound
    [SerializeField] private AudioClip audioAttack; // Enemy 밟았을 때 Sound
    [SerializeField] private AudioClip audioDamaged; // 피격 시 Sound
    [SerializeField] private AudioClip audioCoin; // 코인 먹는 Sound
    [SerializeField] private AudioClip audioDie; // 죽엇을 때 Sound
    [SerializeField] private AudioClip audioFinish; // Stage를 모두 클리어했을 때 Sound
    
    public event Action<int> OnPlayerDamaged; // Damaged를 입었을 때의 Event
    public event Action<bool> OnPlayerDead; // Player가 죽었을 때의 Event
    public event Action<int> OnPlayerGetScore; // Player가 Coin을 먹었거나 적을 죽였을 때 점수를 얻는 Event;
    public event Action<int> OnPlayerStageClear; // Finish에 도착했을 때의 Event

    private Vector3 originPos; // Player의 처음 시작 Position

    [SerializeField] private int health; // Player의 체력

    private const int DefaultLayer = 9;
    private const int DamagedLayer = 10;

    private int xAxis; // Player의 입력 방향
    private float maxSpeed = 5f; // Player의 최대 속도

    private bool shouldJump = false; // 점프키를 눌렀는가
    private bool isGround = false; // 땅에 서있는가
    private bool isStop = false; // 방향키를 뗐는가
    [SerializeField] private float jumpForce = 10f;
    
    void Start() 
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2d = GetComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>();
        originPos = transform.position;
        GameManager.Instance.OnGameFinish += GameResult;
    }

    void Update() 
    {
        if (GameManager.Instance.IsGameOver)
        {
            return;
        }

        xAxis = (int)Input.GetAxisRaw("Horizontal");
        Flip(); // 방향 전환

        if (Input.GetButtonUp("Horizontal")) // 방향키를 떼면 급정지
        {
            isStop = true;
        }

        if (isGround && Input.GetKeyDown(KeyCode.Space)) // 땅에 닿고있을 때만 점프 가능
        {
            shouldJump = true;
        }

        isGround = Physics2D.Raycast(transform.position, Vector2.down, 1.0f, LayerMask.GetMask("Platform"));

        animator.SetInteger("isWalking", xAxis);
        animator.SetBool("isGround", isGround);
    }

    private void FixedUpdate() 
    {
        if (GameManager.Instance.IsGameOver)
        {
            return;
        }

        DoMove();
        DoJump();
        DoStop();
    }

    private void DoMove()
    {
        rigidbody2d.AddForce(Vector2.right * xAxis, ForceMode2D.Impulse);
        if (rigidbody2d.velocity.x > maxSpeed) 
        {
            rigidbody2d.velocity = new Vector2(maxSpeed, rigidbody2d.velocity.y);
        } 
        else if (rigidbody2d.velocity.x < -maxSpeed) 
        {
            rigidbody2d.velocity = new Vector2(-maxSpeed, rigidbody2d.velocity.y);
        }
    }

    private void DoJump()
    {
        if (shouldJump)
        {
            PlayAudioClip("JUMP");
            rigidbody2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            shouldJump = false;
        }
    }

    private void DoStop()
    {
        if (isStop)
        {
            rigidbody2d.velocity = new Vector2(rigidbody2d.velocity.normalized.x * 0.5f, rigidbody2d.velocity.y); // 속도를 크게 줄임
            isStop = false;
        }
    }

    private void Flip() // 방향 전환
    {
        Vector3 currentScale = transform.localScale;
        if (xAxis == -1) 
        {
            transform.localScale = new Vector3(Mathf.Abs(currentScale.x) * (-1f), currentScale.y, currentScale.z);
        }
        else if (xAxis == 1)
        {
            transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            // 공중에 있고 Player가 Enemy의 y좌표보다 높게있다면 밟기
            if (!isGround && (transform.position.y > collision.transform.position.y))
            {
                OnAttack(collision.gameObject);
            }
            // 아니면 Damage를 입음
            else 
            {
                OnDamaged(collision.transform.position);
            }
        }
        else if (collision.collider.CompareTag("Spike"))
        {
            // 충돌체의 위치 정보를 넘겨줌
            OnDamaged(collision.transform.position);
        }
    }

    private void OnAttack(GameObject enemyObject)
    {
        PlayAudioClip("ATTACK");
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        enemy.OnDamaged();
        OnPlayerGetScore.Invoke(100);
        rigidbody2d.AddForce(Vector2.up * jumpForce * 0.7f, ForceMode2D.Impulse);
    }

    private void OnDamaged(Vector3 crashPos) // 피격 시 무적 상태 On
    {
        PlayAudioClip("DAMAGED");

        DecreaseHealth(); // 체력 감소

        if (GameManager.Instance.IsGameOver)
        {
            return;
        }

        // Change PlayerDamaged Layer, 이 Layer는 Spike와 Enemy와 충돌하지 않음
        gameObject.layer = DamagedLayer; 

        spriteRenderer.color = new Color(1f, 1f, 1f, 0.4f); 

        // 피격 시 충돌한 Object의 반대방향으로 튕겨지는 힘을 받음
        rigidbody2d.velocity = Vector2.zero; // 현재 속도 무효화
        int forceDir = (transform.position.x - crashPos.x > 0 ? 1 : -1);
        float crashForce = 7f;
        rigidbody2d.AddForce(new Vector2(forceDir * crashForce, crashForce * 1.5f), ForceMode2D.Impulse);
        animator.SetTrigger("isDamaged");
        Invoke("OffDamaged", 2f); // 2초 뒤에 무적 상태 Off
    }

    private void OffDamaged() // 무적 상태 Off
    {
        // 원래 Layer로 복구
        gameObject.layer = DefaultLayer; 
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Coin"))
        {
            PlayAudioClip("COIN");

            // Coin 획득 Event 발생. 각 Coin에 할당되어있는 Score 추가
            OnPlayerGetScore?.Invoke(collider.GetComponent<Coin>().Score);
            Destroy(collider.gameObject);
        }
        else if (collider.CompareTag("Finish"))
        {
            OnPlayerStageClear?.Invoke(collider.GetComponent<Finish>().NextStageNum);
        }
        else if (collider.CompareTag("Bottom")) // Platform 아래로 떨어졌으면
        {
            rigidbody2d.velocity = Vector2.zero;
            DecreaseHealth(); // 체력 감소
            if (!GameManager.Instance.IsGameOver) // 죽지 않았을 때만
            {
                transform.position = originPos; // 시작 위치로 이동
                CancelInvoke("OffDamaged");
                OffDamaged(); // 무적 판정 중에 떨어졌을 경우 무적 Off
            }
        }
    }

    private void DecreaseHealth()
    {
        health--;
        // 피격 이벤트 발생
        OnPlayerDamaged?.Invoke(health); 
        if (health == 0) // Game Over
        {
            PlayAudioClip("DIE");
            // 사망 이벤트 발생
            OnPlayerDead?.Invoke(false); // Clear 실패
        }
    }

    private void GameResult(bool result)
    {
        if (result)
        {
            SetPlayerWin();
        }
        else
        {
            SetPlayerLose();
        }
        animator.SetInteger("isWalking", xAxis = 0); // Idle 상태로 변경
    }

    private void SetPlayerWin()
    {
        PlayAudioClip("FINISH");
        rigidbody2d.velocity = Vector2.zero;
    }

    private void SetPlayerLose()
    {
        SetDeathAppearance();
        SetPlayerPhysics();
        Destroy(gameObject, 2f);
    }

    private void SetDeathAppearance()
    {
        spriteRenderer.color = new Color(1f, 1f, 1f, 0.4f);
        spriteRenderer.flipY = true;
    }

    private void SetPlayerPhysics()
    {
        boxCollider2d.enabled = false;
        rigidbody2d.velocity = Vector2.zero;
        rigidbody2d.AddForce(Vector2.up * 3f, ForceMode2D.Impulse);
    }

    private void PlayAudioClip(string action)
    {
        switch (action)
        {
            case "JUMP":
                audioSource.PlayOneShot(audioJump);
                break;
            case "ATTACK":
                audioSource.PlayOneShot(audioAttack);
                break;
            case "DAMAGED":
                audioSource.PlayOneShot(audioDamaged);
                break;
            case "COIN":
                audioSource.PlayOneShot(audioCoin);
                break;
            case "DIE":
                audioSource.PlayOneShot(audioDie);
                break;
            case "FINISH":
                audioSource.PlayOneShot(audioFinish);
                break;
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameFinish -= GameResult;
    }
}  
