using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 1f;
    public float jumpForce = 1f;
    private float velocityX = 0;

    private bool onAir = false;
    private bool isFalling = false;
    private bool isWalking = false;
    private bool isDucking = false;

    private SpriteRenderer spriteRend;
    private Rigidbody2D rb2D;

    public Sprite idleSprite;
    public Sprite jumpSprite;
    public Sprite duckSprite;
    public List<Sprite> walkSprites = new List<Sprite>();
    private int currentWalkFrame = 0;
    public float walkCycleDelay = 1;
    private float cycleCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        SetStart();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        ApplyMovement();
    }

    private void LateUpdate()
    {
        PlayerInputs();
        MoveEffects();
        AnimationControl();
    }

    void SetStart()
    {
        if (GetComponent<SpriteRenderer>())
        {
            spriteRend = GetComponent<SpriteRenderer>();
        }
        else { Debug.LogError("Não foi encontrado SpriteRenderer no PlayerMovement."); }

        if (GetComponent<Rigidbody2D>())
        {
            rb2D = GetComponent<Rigidbody2D>();
        }
        else { Debug.LogError("Não foi encontrado Rigidbody2D no PlayerMovement, favor criar e setar valores."); }
    }

    void PlayerInputs()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !onAir)
        {
            rb2D.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            onAir = true;
        }

        if (Input.GetAxisRaw("Vertical") < 0)
        {
            isDucking = true;
        }
        else
        {
            isDucking = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            InventoryManager.Instance.gameObject.SetActive(!InventoryManager.Instance.gameObject.activeSelf);
        }
    }

    void MoveEffects()
    {
        if(velocityX != 0) { isWalking = true; }
        else { isWalking = false; }
        // Flip image
        if (velocityX > 0) { spriteRend.flipX = false; }
        else if (velocityX < 0) { spriteRend.flipX = true; }

        // Track Y velocity
        if(rb2D.velocity.y < 0) { isFalling = true; }
        if (isFalling)
        {
            if(rb2D.velocity.y == 0) { onAir = false; isFalling = false; }
        }

        // TODO: Gain X & Y velocity on fall
        
    }

    void ApplyMovement()
    {
        velocityX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        transform.Translate(velocityX,0,0);
    }

    void AnimationControl()
    {
        if (isWalking && !isFalling)
        {
            cycleCount += Time.deltaTime;
            if(cycleCount >= walkCycleDelay)
            {
                currentWalkFrame++;
                currentWalkFrame = currentWalkFrame % walkSprites.Count;
                spriteRend.sprite = walkSprites[currentWalkFrame];
                cycleCount = 0;
            }
        }
        else if (!isWalking && !isFalling && !isDucking)
        {
            spriteRend.sprite = idleSprite;
        }       
        else if (isDucking)
        {
            spriteRend.sprite = duckSprite;
        }
        
        if (rb2D.velocity.y != 0)
        {
            spriteRend.sprite = jumpSprite;
        }
    }
}
