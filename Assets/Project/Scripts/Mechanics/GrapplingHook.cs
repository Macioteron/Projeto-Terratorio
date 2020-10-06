using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    public float maxTravelDistance = 5;
    public float hookSpeed = 10;
    public float pullForce = 5;
    public int returnHookMult = 2;

    public Rigidbody2D hookUser;

    private float currentHookDistance = 0;
    private bool isPulling = false;
    private bool hooked = false;
    private bool hookFlying = false;
    private float hookVelocity;
    private GameObject thisGrapplingHookGO;
    private Vector2 targetPosHook;
    private List<GameObject> segmentsList = new List<GameObject>();

    private Toolbar toolbar;

    // Added Component References
    private Rigidbody2D rb2D;
    private SpriteRenderer spriteRend;
    private CapsuleCollider2D col2D;

    private Vector2 hookedPos;
    IEnumerator ExtendHook()
    {
        
        hookFlying = true;
        float distanceTravelled = 0;
        EnableDisableComponents();

        // Aparecer no mesmo lugar de quem usou o grappling hook
        thisGrapplingHookGO.transform.position = hookUser.transform.position;
        // Rotaciona para olhar para a direção em que foi jogado a hook
        Vector2 convertedPos = thisGrapplingHookGO.transform.position;
        thisGrapplingHookGO.transform.up = targetPosHook - convertedPos;

        // Faz ele seguir infinitamente até atingir a distancia
        while (distanceTravelled < maxTravelDistance && !hooked)
        {

            distanceTravelled += hookVelocity;
            thisGrapplingHookGO.transform.Translate(0, hookVelocity, 0, Space.Self);

            yield return null;
        }

            // Retorna a hook após atingir a distancia maxima e so para ao chegar no usuario
            while (thisGrapplingHookGO.transform.position.normalized != hookUser.transform.position.normalized && !hooked)
            {
                Vector2 convertedOrigin = hookUser.transform.position;
                thisGrapplingHookGO.transform.up = targetPosHook - convertedOrigin;
                thisGrapplingHookGO.transform.position = Vector2.MoveTowards(thisGrapplingHookGO.transform.position, hookUser.transform.position, hookVelocity / returnHookMult);
                yield return null;
            }

        if (!hooked)
        {
            targetPosHook = Vector2.zero;
            EnableDisableComponents();
        }

        hookFlying = false;

        yield return null;
    }

    // Start is called before the first frame update
    private void Awake()
    {
        SetStart();
    }

    private void FixedUpdate()
    {
        HookEffects();
        
    }
    // Update is called once per frame
    void LateUpdate()
    {
        if (toolbar.currentItemSelected)
        {
            if (toolbar.currentItemSelected.itemName == "Hook")
            {
                if (Input.GetMouseButtonDown(0) && !hookFlying && !hooked)
                {
                    targetPosHook = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    StartCoroutine(ExtendHook());
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    isPulling = true;
                }
                else if (Input.GetKeyUp(KeyCode.E))
                {
                    isPulling = false;
                }

                if (hooked)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        EnableDisableComponents();
                        hooked = false;
                    }
                }
            }
        }
    }
    void SetStart()
    {
        if (GetComponent<SpriteRenderer>())
        {
            spriteRend = GetComponent<SpriteRenderer>();
            spriteRend.enabled = false;
        }
        else { Debug.LogError("Não foi encontrado SpriteRenderer no GrapplingHook."); }

        if (GetComponent<CapsuleCollider2D>())
        {
            col2D = GetComponent<CapsuleCollider2D>();
            col2D.enabled = false;
        }
        else { transform.gameObject.AddComponent<CapsuleCollider2D>(); }

        if (GetComponent<Rigidbody2D>())
        {
            rb2D = GetComponent<Rigidbody2D>();
        }
        else { Debug.LogError("Não foi encontrado Rigidbody2D no GrapplingHook, favor criar e setar valores."); }

        toolbar = FindObjectOfType<Toolbar>();
        thisGrapplingHookGO = this.transform.gameObject;
    }

    private void HookEffects()
    {
        hookVelocity = hookSpeed * Time.deltaTime;
        
        if (hooked)
        {
            Vector2 convertedOrigin = hookUser.transform.position;
            thisGrapplingHookGO.transform.up = targetPosHook - convertedOrigin;
            transform.position = hookedPos;
        }

        PullUser();
    }
    private void PullUser()
    {
        if (hooked)
        {
            currentHookDistance = Vector2.Distance(thisGrapplingHookGO.transform.position, hookUser.position);
            Vector2 pullDirection = targetPosHook - hookUser.position;

            if (currentHookDistance > maxTravelDistance)
            {
                hookUser.AddForce(pullDirection * pullForce / (returnHookMult * 2), ForceMode2D.Force);

                if (currentHookDistance > maxTravelDistance * 2)
                {
                    Debug.Log("BREAK HOOK");
                    EnableDisableComponents();
                    hooked = false;
                }
            }

            if (isPulling)
            {
                hookUser.AddForce(pullDirection * pullForce * 2, ForceMode2D.Force);
            }
        }
    }

    private void EnableDisableComponents()
    {
        spriteRend.enabled = !spriteRend.enabled;
        col2D.enabled = !col2D.enabled;
    }
    private void CreateSegmentsPool()
    {
        segmentsList = new List<GameObject>();
        for (int i = 0; i < maxTravelDistance; i++)
        {
            GameObject newSegment = new GameObject();
            segmentsList.Add(newSegment);
        }
    }

   
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.name != hookUser.name)
        {
            hooked = true;
            hookedPos = transform.position;
        }
    }
}
