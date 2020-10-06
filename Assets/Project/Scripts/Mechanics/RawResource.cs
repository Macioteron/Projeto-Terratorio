using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RawResource : MonoBehaviour, IPointerDownHandler,IPointerUpHandler
{
    public float resourceHP = 5;
    private float maxHP = 0;
    public bool startDropped = false;
    public ItemSO startDroppedData;
    private bool hitting = false;
    private bool isDestroyed = false;
    private ItemSO resourceData;
    private SpriteRenderer spriteRend;
    private Rigidbody2D rb2D;
    private Toolbar toolbar;
    private InventoryManager inventoryManager;
    private BoxCollider2D boxCol;
    private void Awake()
    {
        if (GetComponent<SpriteRenderer>())
        {
            spriteRend = GetComponent<SpriteRenderer>();
        }
        else { spriteRend = transform.gameObject.AddComponent<SpriteRenderer>(); }

        if (!GetComponent<BoxCollider2D>()) { boxCol = transform.gameObject.AddComponent<BoxCollider2D>(); }
        if (!GetComponent<Rigidbody2D>()) 
        { 
            rb2D = transform.gameObject.AddComponent<Rigidbody2D>(); 
            rb2D.gravityScale = 0; 
            rb2D.constraints = RigidbodyConstraints2D.FreezeAll; 
        }
        inventoryManager = InventoryManager.Instance;
        if (startDropped)
        {
            SetResourceData(startDroppedData);
            StartUpAsDroppedResource();
        }
        maxHP = resourceHP;
        toolbar = FindObjectOfType<Toolbar>();
    }
    IEnumerator WaitPickUpOnDrop()
    {
        yield return new WaitForSeconds(2);
        isDestroyed = true;
    }
    public void StartUpAsFixedResource()
    {
        
    }

    public void StartUpAsDroppedResource()
    {        
        rb2D.constraints = RigidbodyConstraints2D.None;
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        transform.position = player.localPosition;
        rb2D.gravityScale = 1;
        boxCol.size = spriteRend.sprite.bounds.size;
        rb2D.AddForce(player.right, ForceMode2D.Force);
        StartCoroutine(WaitPickUpOnDrop());
        isDestroyed = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (hitting)
        {
            resourceHP -= Time.deltaTime;
            transform.localScale = new Vector3(resourceHP /5, resourceHP / 5, resourceHP / 5);
            
            if (resourceHP <= 0)
            {
                rb2D.gravityScale = 1;
                rb2D.constraints = RigidbodyConstraints2D.None;
                rb2D.AddForce(new Vector2(Random.Range(1, 2), Random.Range(1, 2)), ForceMode2D.Force);
                isDestroyed = true;
                transform.localScale = Vector3.one;
            }
        }
    }

    public void SetResourceData(ItemSO itemData)
    {
        resourceData = itemData;
        spriteRend.sprite = resourceData.icon;
        boxCol.size = spriteRend.sprite.bounds.size;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(toolbar.currentItemSelected)
        {
            if (toolbar.currentItemSelected.itemType == ItemSO.ItemType.Tool)
            {
                hitting = true;
                spriteRend.color = Color.red;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        hitting = false;
        spriteRend.color = Color.white;

        transform.localScale = Vector3.one;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player") && isDestroyed)
        {
            if(!inventoryManager)
            { inventoryManager = InventoryManager.Instance; }
            inventoryManager.PickItem(resourceData);
            Destroy(this.gameObject);
        }
        else if(collision.transform.CompareTag("Player") && !isDestroyed)
        {
            boxCol.isTrigger = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            boxCol.isTrigger = false;
        }
    }
}
