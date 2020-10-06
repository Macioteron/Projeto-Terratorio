using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Serve para armazenar os dados de um bau contendo o numero de slots e itens dentro dele
/// Além de detectar o clique do player para abrir e chamar a UI ChestManager
/// </summary>
public class Chest : MonoBehaviour, IPointerDownHandler
{
    public StorageSO chestData;

    private ChestManager chest;
    private InventoryManager inventoryManager;
    private bool chestOpen = false;
    private void Start()
    {
        inventoryManager = InventoryManager.Instance;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (chestData )
        {
            if (!chest)
            {
                chest = FindObjectOfType<ChestManager>();
            }
            chest.BuildChestUI(chestData);
            chest.ActivatePanel();
            inventoryManager.panelFolder.gameObject.SetActive(true);
        }
        else { Debug.LogError("Bau sem dados para montagem"); }
    }

}
