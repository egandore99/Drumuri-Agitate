using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public Sprite idleSprite;
    public Sprite leapSprite;
    public Sprite deadSprite;

    private bool cooldown;
    private float leftBarrierX;
    private float rightBarrierX;
    private Vector3 spawnPosition;

    public GameManager gm;
    public CameraLogic camera;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spawnPosition = transform.position;
        camera.MoveCamera(0);
    }

    private void Start()
    {
        GameObject[] barriers = GameObject.FindGameObjectsWithTag("Barrier");

        if (barriers.Length < 2)
        {
            Debug.LogError("Need at least 2 barriers tagged as 'Barrier'!");
            return;
        }

        GameObject leftBarrier = barriers[0];
        GameObject rightBarrier = barriers[0];

        foreach (GameObject barrier in barriers)
        {
            float barrierX = barrier.transform.position.x;
            if (barrierX < leftBarrier.transform.position.x)
            {
                leftBarrier = barrier;
            }
            if (barrierX > rightBarrier.transform.position.x)
            {
                rightBarrier = barrier;
            }
        }

        leftBarrierX = leftBarrier.GetComponent<Collider2D>().bounds.max.x;
        rightBarrierX = rightBarrier.GetComponent<Collider2D>().bounds.min.x;

        Debug.Log("Left Barrier X: " + leftBarrierX);
        Debug.Log("Right Barrier X: " + rightBarrierX);
    }

    private void Update()
    {
        if (!disableMovement)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                Move(Vector3.up);
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 180f);
                Move(Vector3.down);
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                Move(Vector3.left);
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                transform.rotation = Quaternion.Euler(0f, 0f, -90f);
                Move(Vector3.right);
            }
        }
    }

    private void LateUpdate()
    {
        CheckParentOutOfBounds(); 
    }

    private bool disableMovement = false;

    private void CheckParentOutOfBounds()
    {
        if (transform.parent != null)
        {
            float parentX = transform.parent.position.x;
            
            if (parentX < (leftBarrierX - 0.1f) || parentX > (rightBarrierX + 0.1f))
            {

                if (!disableMovement)
                    disableMovement = true;
            } else
            {
                if(disableMovement)
                    disableMovement = false;
            }
        }
    }

    private void CheckBarrierOverlap()
    {
        Collider2D barrier = Physics2D.OverlapBox(transform.position, Vector2.zero, 0f, LayerMask.GetMask("Barrier"));
        if (barrier != null)
        {
            Vector3 closestPoint = barrier.ClosestPoint(transform.position);

            Vector3 direction = (transform.position - closestPoint).normalized;

            transform.position = closestPoint + direction * 0.4f;

            transform.SetParent(null);
        }
    }

    private void Move(Vector3 direction)
    {
        if (cooldown)
        {
            return;
        }

        Vector3 destination = transform.position + direction;

        Collider2D barrier = Physics2D.OverlapBox(destination, Vector2.zero, 0f, LayerMask.GetMask("Barrier"));
        Collider2D platform = Physics2D.OverlapBox(destination, Vector2.zero, 0f, LayerMask.GetMask("Platform"));
        Collider2D obstacle = Physics2D.OverlapBox(destination, Vector2.zero, 0f, LayerMask.GetMask("Obstacle"));

        if (barrier != null)
        {
            return;
        }

        if (platform != null)
        {
            StartCoroutine(Leap(destination));
            transform.SetParent(platform.transform);
        }
        else
        {
            transform.SetParent(null);
        }

        if (obstacle != null)
        {
            if (transform.parent == null)
            {
                transform.position = destination;
                Death();
            }
        }
        else
        {
            StartCoroutine(Leap(destination));
        }
    }

    private IEnumerator Leap(Vector3 destination)
    {
        Debug.Log("trying to Leap");
        Vector3 startPosition = transform.position;
        float elapsed = 0f;
        float duration = 0.125f;

        spriteRenderer.sprite = leapSprite;
        cooldown = true;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPosition, destination, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = destination;
        spriteRenderer.sprite = idleSprite;
        cooldown = false;
    }

    public void Death()
    {
        if (enabled)
        {
            StopAllCoroutines();   
            
            transform.rotation = Quaternion.identity;
            spriteRenderer.sprite = deadSprite;
            enabled = false;
            gm.Invoke(nameof(gm.Respawn), 1f);
        }
    }

    public void Respawn()
    {
        transform.SetParent(null);
        transform.rotation = Quaternion.identity;
        transform.position = spawnPosition;
        spriteRenderer.sprite = idleSprite;
        gameObject.SetActive(true);
        enabled = true;
        camera.MoveCamera(0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle") && transform.parent == null)
        {
            Death();
        }

        if(other.gameObject.layer == LayerMask.NameToLayer("CameraTrigger")){
            camera.MoveCamera(1);
        }
    }
}