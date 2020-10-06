using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// O script controla toda a parte visual do bau para criar ou esconder slots como também guardar referencia do bau atual aberto
/// Ele também faz o controle do registro de itens que entra e sai do bau
/// </summary>
public class ChestManager : MonoBehaviour
{
    public Transform chestSlotsFolder;
    public Transform panelFolder;
    private Transform chestItemDragFolder;
    public GameObject slotPrefab;

    public StorageSO tempSO;
    private StorageSO currentChestData;

    // Para manter um controle nos slots do bau
    private List<ItemSlot> chestSlotList = new List<ItemSlot>();
    // Para manter o controle dos slots inativos
    private List<ItemSlot> chestInactiveSlotList = new List<ItemSlot>();
    // Para manter um controle nos drags de item
    private List<ItemDrag> chestDragItemsList = new List<ItemDrag>();

    private bool isFirstStart = true;
    IEnumerator CallResize()
    {
        yield return new WaitForEndOfFrame();
        // TODO: Se encontrar um jeito de calcular tudo em uma vez mudar
        // A primeira serve para calcular
        ResizeSlotFolderSize();
        // A segunda para arrumar o tamanho
        ResizeSlotFolderSize();
        yield return null;
    }
    private void Start()
    { 
        BuildChestUI(tempSO);
        isFirstStart = false;
        panelFolder.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { panelFolder.gameObject.SetActive(false); }
    }
    /// <summary>
    /// Constroe a UI do bau de acordo com seus slots e adiciona os itens na lista que o bau possui
    /// </summary>
    /// <param name="chestData"></param>
    public void BuildChestUI(StorageSO chestData)
    {
        if (chestSlotsFolder)
        {
            currentChestData = chestData;
            if (!chestItemDragFolder)
            {
                // Cria uma pasta para guardar os item drags
                GameObject newFolder = new GameObject("Items"); 
                newFolder.transform.SetParent(transform);
                RectTransform temRect = newFolder.AddComponent<RectTransform>();
                temRect.anchoredPosition = Vector2.zero;                
                chestItemDragFolder = newFolder.transform;
            }

            // Cria os slots adicionais fazendo a diferença dos que falta
            if (currentChestData.chestTotalSlots > chestSlotsFolder.childCount - chestInactiveSlotList.Count)
            {
                int newCount = currentChestData.chestTotalSlots - chestSlotsFolder.childCount;
                if(newCount < 0) { newCount *= -1; }
                CreateSlotForChest(newCount);
            }
            // Esconde os slots em excesso a partir da diferença
            else if(currentChestData.chestTotalSlots < chestSlotsFolder.childCount)
            {
                int newCount = chestSlotsFolder.childCount - currentChestData.chestTotalSlots;
                HideUnusedSlots(newCount);
            }

            if (!isFirstStart)
            {
                RebuildItemsOnChest(chestData);
            }
        }
        else
        {
            Debug.LogError("Não foi dado referencia da pasta com slots para o bau!");
        }
        transform.gameObject.SetActive(true);
    }

    /// <summary>
    /// Cria os slots de acordo com o numero passado cuidar para n duplicar mais slots do que deve
    /// </summary>
    /// <param name="copies"></param>
    private void CreateSlotForChest(int copies)
    {
        int newCount = copies;
        // Chama a função para procurar pelos GO inativos na lista de SlotList
        int inactiveSlotCount = chestInactiveSlotList.Count;
       
        // Reativa as gameobjetcs desativadas, reaproveitando e evitando instanciar sempre
        if (inactiveSlotCount > 0)
        {
            int toActivate = newCount - inactiveSlotCount;
            // Se o valor de novos slots for menos que os inativos ele converte para um valor positivo
            if(toActivate < 0) { toActivate *= -1; newCount = 0; }
            ActivateHidedSlots(toActivate);
        }

        for (int i = 0; i < newCount; i++)
        {
            // Cria o slot do bau com uma prefab
            GameObject newSlot = Instantiate(slotPrefab, chestSlotsFolder);
            newSlot.name += chestSlotsFolder.childCount;
            chestSlotList.Add(newSlot.AddComponent<ItemSlot>());
            newSlot.GetComponent<ItemSlot>().SetStorageSO(currentChestData);
            // Cria um item para drag
            GameObject newItemDrag = new GameObject("ItemDrag");
            newItemDrag.transform.SetParent(chestItemDragFolder);
            newItemDrag.AddComponent<Image>();
            chestDragItemsList.Add(newItemDrag.AddComponent<ItemDrag>());
            newItemDrag.SetActive(false);
        }
        StartCoroutine(CallResize());
        
    }   

    /// <summary>
    /// Ativa os slots desativados guardados na lista de slotsInactive
    /// </summary>
    /// <param name="activeCount"></param>
    private void ActivateHidedSlots(int activeCount)
    {

        if (activeCount > chestInactiveSlotList.Count)
        { activeCount = chestInactiveSlotList.Count; }

        for (int i = 0; i < activeCount; i++)
        {
            chestInactiveSlotList[i].gameObject.SetActive(true);
        }
        for (int i = 0; i < chestInactiveSlotList.Count; i++)
        {
            if (chestInactiveSlotList[i].gameObject.activeSelf)
            {
                chestInactiveSlotList.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Desativa o excesso de slots e guarda numa lista para reativar depois
    /// </summary>
    /// <param name="hideCount"></param>
    private void HideUnusedSlots(int hideCount)
    {
        int count = hideCount;
        for (int i = chestSlotList.Count - 1; i >= 0; i--)
        {
            if(hideCount > 0)
            {
                chestInactiveSlotList.Add(chestSlotList[i]);
                chestSlotList[i].gameObject.SetActive(false);
                hideCount--;
            }
            else { return; }
        }
    }
    /// <summary>
    /// Faz o calculo para resize da tela em Y quando slot é adicionado e ajudar na scroll dela
    /// </summary>
    private void ResizeSlotFolderSize()
    {
        RectTransform chestRect = chestSlotsFolder.GetComponent<RectTransform>();

        // Conta as colunas para saber quantas tem no total
        int colums = 0;
        // Usa as linhas para ver qual o tamanho em Y deve ser o Rect, mais para caso da tela ser redimensionada
        int rows = 0;

        // Valor adicional para caso possua um padding
        float paddingY = 0;
        paddingY = chestSlotsFolder.GetComponent<GridLayoutGroup>().padding.top +
                   chestSlotsFolder.GetComponent<GridLayoutGroup>().padding.bottom;

        Vector2 slotSize = Vector2.zero;
       
        // Serve como referencia para calcular a altura e o espaçamento para quando aumentar o tamanho da Rect em Y
        if (slotPrefab) { slotSize = slotPrefab.GetComponent<RectTransform>().sizeDelta; }
        else if(chestSlotsFolder.childCount > 0){ slotSize = chestSlotsFolder.GetChild(0).GetComponent<RectTransform>().sizeDelta; }

        float currentYLoop = chestSlotsFolder.GetChild(0).GetComponent<RectTransform>().anchoredPosition.y;
        // Loop para contagem das colunas
        for (int i = 0; i < chestSlotsFolder.childCount; i++)
        {            
            RectTransform tempSlotChild = chestSlotsFolder.GetChild(i).GetComponent<RectTransform>();
            // Conta as colunas usando o Y atual como referencia
            if(tempSlotChild.anchoredPosition.y == currentYLoop)
            {
                //chestSlotsFolder.GetChild(i).GetComponent<Image>().color = Color.blue;
                colums++;
            }
            // Termina a contagem das colunas
            else { break; }
        }
        // Loop para contagem das linhas
        for (int i = 0; i < chestSlotsFolder.childCount; i+= colums)
        {
            if (chestSlotsFolder.GetChild(i))
            {                
                //chestSlotsFolder.GetChild(i).GetComponent<Image>().color = Color.red;
                rows++;
            }
        }

        //Debug.Log("Colunas: " + colums + " Linhas: " + rows);
        // Dar o novo tamanho de acordo com tamanho dos slots e das colunas
        chestRect.sizeDelta = new Vector2(chestSlotsFolder.transform.parent.GetComponent<RectTransform>().rect.width, (rows + 1) * slotSize.y);
        chestItemDragFolder.GetComponent<RectTransform>().sizeDelta = chestRect.sizeDelta;
        chestRect.GetComponent<GridLayoutGroup>().SetLayoutVertical();

    }

    /// <summary>
    /// Seta os drag de item nos slots que estavam salvos
    /// </summary>
    private void RebuildItemsOnChest(StorageSO chestData)
    {
        List<ItemSlot> availableSlots = new List<ItemSlot>();
        for (int i = 0; i < chestDragItemsList.Count; i++)
        {
            chestDragItemsList[i].ClearDrag();
        }

        currentChestData = chestData;
        for (int i = 0; i < chestSlotList.Count; i++)
        {
            chestSlotList[i].SetStorageSO(currentChestData);
            chestSlotList[i].isSlotUsed = false;
        }
        for (int i = 0; i < chestSlotList.Count; i++)
        {
            ItemSlot chest = chestSlotList[i];
            if (chest.gameObject.activeSelf && !chest.isSlotUsed)
            {
                availableSlots.Add(chest);
            }
        }

        // TODO: Fazer os itemDrag aparecer no mesmo slot ao abrir o bau, ao inves deles aparecerem organizados automaticamente

        for (int i = 0; i < currentChestData.contentsInChest.Count; i++)
        {
            ItemSO item = currentChestData.contentsInChest[i];
            // Se existir os slots vazio
            if (availableSlots[i]) 
            {                
                if(item.GetChestSlotID() <= -1)
                {
                    SetItemDragData(item,availableSlots[i]);
                }
            }
            // Dropar o item em questão
            else
            {

            }
        }       
        
    }

    /// <summary>
    /// Função para procurar por um itemDrag vazio e setar os dados do itemSO nele e coloca no slot providenciado
    /// </summary>
    /// <param name="itemData"></param>
    /// <param name="slotToSet"></param>
    private void SetItemDragData(ItemSO itemData, ItemSlot slotToSet)
    {
        for (int i = 0; i < chestDragItemsList.Count; i++)
        {
            ItemDrag itemDrag = chestDragItemsList[i];
            if (!itemDrag.itemData)
            {
                itemDrag.itemData = itemData;
                slotToSet.LinkItemToSlot(itemDrag);
                return;
            }
        }
    }

    public void ActivatePanel()
    {
        panelFolder.gameObject.SetActive(true);
    }
    /// <summary>
    /// Função para dropar itens em excesso no bau caso exista, ou que os itens foram adicionados de forma ilegal
    /// </summary>
    private void DropExcess()
    {

    }

    /// <summary>
    /// Usar para detectar resize na UI
    /// </summary>
    private void OnRectTransformDimensionsChange()
    {
        
    }
}
