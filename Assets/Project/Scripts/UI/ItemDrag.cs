using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemDrag : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IDropHandler
{
    public ItemSO itemData;

    public ItemSlot itemSlot;
    private Transform itemsFolder;
    private ItemSlot lastSlot;
    private CanvasGroup canvasGroup;
    private Image image;
    private void Awake()
    {
        itemsFolder = transform.parent;
        canvasGroup = this.gameObject.AddComponent<CanvasGroup>();
        if (itemData)
        {
            transform.name = itemData.itemName + " OnSlotInventory";            
        }
        image = GetComponent<Image>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {        
        canvasGroup.alpha = 0.75f;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(itemsFolder.parent);        
        if(eventData.button == PointerEventData.InputButton.Left && Input.GetKey(KeyCode.LeftShift))
        {
            Debug.Log("FAST TRANSFER");
        }
        
    }
    public void OnPointerUp(PointerEventData eventData)
    {        
        if (eventData.button == PointerEventData.InputButton.Right && Input.GetKey(KeyCode.LeftShift))
        {
            Debug.Log("Dropa Item");
            RawResource dropped = new GameObject(itemData.name).AddComponent<RawResource>();
            dropped.SetResourceData(itemData);
            dropped.StartUpAsDroppedResource();
            ClearDrag();

        }
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Verificação para recolocar o drag do item de volta no seu slot
        // TODO: FAZER UM JEITO DE DROPAR O ITEM PRA FORA DO INVENTARIO/BAU AO FAZER DRAG PARA FORA DA TELA
        if (lastSlot)
        {
            transform.SetParent(lastSlot.transform);
            transform.localPosition = Vector3.zero;
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (lastSlot)
        {
            lastSlot.RemoveLinkOnSlot();
            lastSlot.RemovedItemFromChest(itemData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            transform.position = Input.mousePosition;
        }
    }
    public void OnDrop(PointerEventData eventData)
    {
        // Verifica ja tem outro item em cima do slot 
        if (eventData.pointerDrag.GetComponent<ItemDrag>())
        {
            // Troca de lugar do seu slot antigo com o novo
            SwapItemOnSlot(eventData.pointerDrag.GetComponent<ItemDrag>());
        }
    }

    public void ClearDrag()
    {
        itemData = null;
        image.sprite = null;
        if (itemSlot)
        {
            itemSlot.isSlotUsed = false;
        }
        gameObject.SetActive(false);
    }
    /// <summary>
    /// Faz a troca de items nos slots ATENÇÃO: ERROS SE FOR USADO EM UM ITEM QUE NÃO POSSUA UMA REFERENCIA DE ITEMSLOT
    /// </summary>
    /// <param name="itemToSwap"></param>
    private void SwapItemOnSlot(ItemDrag itemToSwap)
    {
        if (itemToSwap.GetSlot())
        {
            // Cria uma referencia temporario do slot que foi pego para trocar
            ItemSlot tempSlot = itemToSwap.GetSlot();
            // Força ele no seu slot antigo
            lastSlot.LinkItemToSlot(itemToSwap);
            // Se auto coloca no slot em que estava o item antigo
            tempSlot.LinkItemToSlot(this);
        }
    }

    /// <summary>
    /// Usado pelo script ItemSlot para setar o ultimo slot q esteve, usado para auxiliar no item swap de seus drags
    /// </summary>
    /// <param name="slot"></param>
    public void SetSlot(ItemSlot slot)
    {
        lastSlot = slot;
    }
    /// <summary>
    /// Usado para auxiliar no swap de item drags entre si
    /// </summary>
    /// <returns></returns>
    public ItemSlot GetSlot() { return lastSlot; }

    
}
