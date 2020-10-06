using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Toolbar : MonoBehaviour
{
    public ItemSO currentItemSelected;
    private int currentSlotSelectedID;
    private StorageSO toolbarStorage;
    private List<ItemSlot> toolBarSlotsList = new List<ItemSlot>();
    private void Awake()
    {
        toolbarStorage = ScriptableObject.CreateInstance<StorageSO>();
        SetupStart();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        MouseScroll();
    }

    void SetupStart()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            ItemSlot tempSlot = transform.GetChild(i).gameObject.AddComponent<ItemSlot>();
            tempSlot.SetStorageSO(toolbarStorage);
            Outline tempOutline = tempSlot.gameObject.AddComponent<Outline>();
            tempOutline.effectColor = Color.red;
            tempOutline.effectDistance = new Vector2(2, -2);
            tempOutline.enabled = false;

            toolBarSlotsList.Add(tempSlot);
        }

        ToolbarSelect(0);
    }

    void ToolbarSelect(int selectedSlotID)
    {
        if (toolBarSlotsList[currentSlotSelectedID]) { toolBarSlotsList[currentSlotSelectedID].gameObject.GetComponent<Outline>().enabled = false; }
        currentSlotSelectedID = selectedSlotID;
        toolBarSlotsList[currentSlotSelectedID].gameObject.GetComponent<Outline>().enabled = true;
        currentItemSelected = toolBarSlotsList[currentSlotSelectedID].itemOnSlotData;
    }

    void MouseScroll()
    {
        if(Input.GetAxisRaw("Mouse ScrollWheel") > 0)
        {
            int temp = currentSlotSelectedID + 1;
            temp = temp % transform.childCount;
            ToolbarSelect(temp);
        }
        else if(Input.GetAxisRaw("Mouse ScrollWheel") < 0)
        {
            int temp = currentSlotSelectedID - 1;
            if(temp < 0) { temp = transform.childCount-1; }
            ToolbarSelect(temp);
        }

    }
}
