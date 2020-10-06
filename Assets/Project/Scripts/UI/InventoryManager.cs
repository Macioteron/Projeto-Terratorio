using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//TODO: ACHAR UM JEITO DE ARRUMAR E SIMPLIFICAR A LOGICA DE INTERAÇÃO E REGISTRO DE ITENS NO BAU E INVENTARIO
public class InventoryManager : MonoBehaviour
{
    public int maxItemStack;
    public Transform slotsFolder;
    public Transform itemDragFolder;
    public Transform panelFolder;

    private StorageSO inventoryStorage;
    // Serve para o controle visual dos itens no inventario de drag e drop
    private List<ItemDrag> itemDragOnInventory = new List<ItemDrag>();
    // Servirá para pick up de item e verificar se tem slot sobrando para adicionar no inventario
    private List<ItemSlot> itemSlotsList = new List<ItemSlot>();

    public static InventoryManager Instance;

    private void Awake()
    {
        if (!Instance) { Instance = this; } else { Destroy(this); Debug.Log("DELETADO COPIA");  }
    }
    void Start()
    {
        StartSetup();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void StartSetup()
    {        
        // Cria as pasta se nao tiver
        if (!slotsFolder) 
        {
            if (transform.Find("Slots"))
            {
                slotsFolder = slotsFolder.Find("Slots");
            }
            else
            {
                slotsFolder = new GameObject("Slots").transform; slotsFolder.SetParent(transform);
            }
        }
        if (!itemDragFolder)
        {
            if (transform.Find("Items"))
            {
                itemDragFolder = transform.Find("Items");
            }
            else
            {
                itemDragFolder = new GameObject("Items").transform; itemDragFolder.SetParent(transform);
            }
        }
        if (!panelFolder)
        {
            panelFolder = new GameObject("Panel Folder").transform;

            panelFolder.transform.SetParent(transform);
        }

        slotsFolder.SetParent(panelFolder.transform);
        itemDragFolder.SetParent(panelFolder.transform);

        inventoryStorage =  ScriptableObject.CreateInstance<StorageSO>();
        inventoryStorage.chestName = "Inventory";       
        // Cria os drag e drop vazio para os itens que serão coletados
        for (int i = 0; i < slotsFolder.childCount; i++)
        {
            ItemSlot newSlot = slotsFolder.GetChild(i).gameObject.AddComponent<ItemSlot>();
            // Adiciona ao slot o seu script e na lista para um futuro uso caso necessario
            itemSlotsList.Add(newSlot);
            newSlot.SetStorageSO(inventoryStorage);
            // Cria uma gameobject para guardar e visualizar os itens no inventario para pool
            GameObject newItemOnSlot = new GameObject("EmptyItemOnInventory");
            // Set ele numa pasta de items (mais para a parte visual)
            newItemOnSlot.transform.SetParent(itemDragFolder);
            // Adiciona os componentes desse item
            newItemOnSlot.AddComponent<Image>();
            newItemOnSlot.AddComponent<ItemDrag>();
            GameObject newText = new GameObject("ItemDragTextCount");
            newText.AddComponent<Text>().font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            newText.transform.SetParent(newItemOnSlot.transform);
            // Adiciona na lista de items para a pool
            itemDragOnInventory.Add(newItemOnSlot.GetComponent<ItemDrag>());
            // Desativa ela por não estar sendo usada
            newItemOnSlot.SetActive(false);
        }

        inventoryStorage.contentsInChest = new List<ItemSO>();
        inventoryStorage.chestTotalSlots = slotsFolder.childCount;

    }

    /// <summary>
    /// Função para procurar um itemDrag vazio e setar a referencia de item contido nele,
    /// </summary>
    /// <param name="itemData"></param>
    private void LinkNewItemOnEmptySlot(ItemSO itemData)
    {
        // Procura por um ItemDrag que não esteja em uso e setado em um slot do inventario
        for (int i = 0; i < itemDragOnInventory.Count; i++)
        {
            if (!FindAndMerge(itemData))
            {
                if (!itemDragOnInventory[i].itemData)
                {
                    // Se achar ele seta o item em questao e passa o icone nele
                    itemDragOnInventory[i].itemData = itemData;
                    itemDragOnInventory[i].transform.GetComponent<Image>().sprite = itemData.icon;
                    itemData.itemCount++;
                    itemDragOnInventory[i].transform.GetChild(0).GetComponent<Text>().text = itemData.itemCount.ToString();
                    // Procura por um slot vazio para ancorar/setar sua posição nele
                    for (int e = 0; e < itemSlotsList.Count; e++)
                    {
                        if (!itemSlotsList[e].isSlotUsed)
                        {
                            itemSlotsList[e].LinkItemToSlot(itemDragOnInventory[i]);

                            return;
                        }
                    }
                }
            }
            else { return; }
        }
        // Caso não encontre uma vaga ele irá chamar a função de drop
        ItemDrag excessToDrop = new ItemDrag();
        excessToDrop.itemData = itemData;
        DropItem(excessToDrop, excessToDrop.itemData.itemCount);
    }
    public void PickItem(ItemSO itemData)
    {
        LinkNewItemOnEmptySlot(itemData);
    }

    /// <summary>
    /// Função para dropar o item para o mundo com possibilidade de escolher a quantidade
    /// Recebe como parametro o Item de drag e drop por conta do seu link visual e de Data
    /// </summary>
    /// <param name="itemOnSlot"></param>
    /// <param name="dropCount"></param>
    public void DropItem(ItemDrag itemOnSlot, int dropCount)
    {        
        // Se o drop requisitado for mais do que possui ele irá dropar tudo e zerar o Item de drag e drop
        if (itemOnSlot.itemData.itemCount < dropCount)
        {
            itemOnSlot.itemData = null;
            // TODO: Criar uma desativação no Item que não tem mais Data
            Debug.Log("Dropped to World By max amount reachead");
            // Adicionar linha para transferir o item para o mundo
        }
        else
        {
            // Caso ele n seja maior ele irá descontar apena e dropar o item com a quantidade requisitada ao mundo
            itemOnSlot.itemData.itemCount -= dropCount;
            // Verifica se ele n requisitou o valor exato 
            if(itemOnSlot.itemData.itemCount == 0)
            {
                // Anula o Data do Item de drag e drop
                itemOnSlot.itemData = null;
                // TODO: Criar uma desativação no Item que não tem mais Data
            }
            Debug.Log("Dropped to World By custom amount");
            // Adicionar linha para transferir o item para o mundo
        }
    }

    private bool FindAndMerge(ItemSO itemData)
    {
        for (int i = 0; i < itemDragOnInventory.Count; i++)
        {
            if (itemDragOnInventory[i].itemData)
            {
                if (itemDragOnInventory[i].itemData.name == itemData.name)
                {
                    if (itemDragOnInventory[i].itemData.itemType != ItemSO.ItemType.Tool)
                    {
                        itemDragOnInventory[i].itemData.itemCount += itemData.itemCount + 1;
                    }
                    else { itemDragOnInventory[i].itemData.itemCount = 1; }
                    itemDragOnInventory[i].transform.GetChild(0).GetComponent<Text>().text = itemDragOnInventory[i].itemData.itemCount.ToString();
                    return true;
                }
            }
        }
        return false;
    }
}
