using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IDropHandler
{
    public bool isSlotUsed = false;
    public ItemSO itemOnSlotData;
    private RectTransform rectTransform;
    private StorageSO chestData;
    // Start is called before the first frame update
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void OnDrop(PointerEventData eventData)
    {
        // Para caso esteja com drag de um itemDrag ele irá pega-lo e setar em sua posição do slot
        if (eventData.pointerDrag != null && !isSlotUsed)
        {
            if (eventData.button == PointerEventData.InputButton.Left && eventData.pointerDrag.GetComponent<ItemDrag>())
            {                
                LinkItemToSlot(eventData.pointerDrag.GetComponent<ItemDrag>());
                AddItemToChest(eventData.pointerDrag.GetComponent<ItemDrag>().itemData);
            }
        }

    }
    
    
    /// <summary>
    /// Da a referencia do bau para ser usado para adicionar ou remover os conteudos dentro do ChestSO
    /// </summary>
    /// <param name="chestReference"></param>
    public void SetStorageSO(StorageSO chestReference)
    {
        chestData = chestReference;
    }

    #region Set & Remove ItemDrag on Slot
    /// <summary>
    /// Chama a função para atualizar seu tamanho e posição do itemDrag
    /// </summary>
    /// <param name="item"></param>
    public void LinkItemToSlot(ItemDrag item)
    {
        //TODO: ACHAR SOLUCAO DO PQ O BAU NA PRIMEIRA ABRIDA NAO MOSTRA OS ITENS
        // Verifica se foi atualizado o seu tamanho do slot para ver se a UI foi atualizada
        isSlotUsed = true;
        if (rectTransform.sizeDelta != Vector2.zero)
        {
            LinkItemToSlotAfterFrame(item);
        }
        // Caso veja que não foi ele irá chamar uma Coroutine para esperar ele terminar o frame para atualizar
        else { if (this.gameObject.activeSelf) { StartCoroutine(CorrectOnFrame(item)); } }
    }

    /// <summary>
    /// Função que atualzia o tamanho e a seta o itemDrag na posição do slot, 
    /// aproveita e seta o sprite que tem e torna sua gameobject ativa
    /// </summary>
    /// <param name="item"></param>
    private void LinkItemToSlotAfterFrame(ItemDrag item)
    {
        item.SetSlot(this);
        item.transform.SetParent(this.transform);
        item.GetComponent<RectTransform>().sizeDelta = rectTransform.sizeDelta;
        if (item.transform.childCount > 0)
        {
            item.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = rectTransform.sizeDelta;
        }
        item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        item.GetComponent<Image>().sprite = item.itemData.icon;
        item.itemSlot = this;
        itemOnSlotData = item.itemData;
        item.gameObject.SetActive(true);
    }

    /// <summary>
    /// Coroutine chamada para fazer o trabalho da função LinkItemToSlotAfterFrame após ter a UI atualizada
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    IEnumerator CorrectOnFrame(ItemDrag item)
    {
        yield return new WaitForEndOfFrame();
        LinkItemToSlotAfterFrame(item);
        //LinkItemFromChestToSlot(item);
        yield return null;
    }


    /// <summary>
    /// Função para mostrar que o slot esta desocupado por um itemDrag
    /// </summary>
    public void RemoveLinkOnSlot()
    {        
        isSlotUsed = false;
    }

    public void AddItemToChest(ItemSO itemAdded)
    {
        chestData.contentsInChest.Add(itemAdded);
    }

    public void RemovedItemFromChest(ItemSO itemRemoved)
    {
        chestData.contentsInChest.Remove(itemRemoved);
        foreach(ItemSO chest in chestData.contentsInChest)
        {
            Debug.Log(chest.itemName);
        }
    }
    #endregion
}
