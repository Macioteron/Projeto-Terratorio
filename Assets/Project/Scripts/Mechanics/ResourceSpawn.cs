using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script temporario para spawnar no mapa recursos
/// </summary>
public class ResourceSpawn : MonoBehaviour
{
    public Transform maxX;
    public Transform minX;
    public Transform maxY;
    public Transform minY;

    public float respawnTimer = 60;
    public int respawnAmount = 5;
    public int maxAmountSpawned = 10;

    public List<ItemSO> resourcesSpawnList = new List<ItemSO>();
    private GameObject resourceFolder;
    IEnumerator RespawnResource()
    {
        while (true)
        {
            if (resourceFolder.transform.childCount < maxAmountSpawned)
            {
                int randomizeAmount = Random.Range(1, respawnAmount);
                for (int i = 0; i < randomizeAmount; i++)
                {
                    ItemSO randomItem = resourcesSpawnList[Random.Range(0, resourcesSpawnList.Count)];
                    GameObject newResource = new GameObject("RawResource");
                    newResource.transform.SetParent(resourceFolder.transform);
                    newResource.AddComponent<RawResource>().SetResourceData(randomItem);
                    Vector3 newPos = new Vector3(Random.Range(minX.position.x, maxX.position.x), Random.Range(minY.position.y, maxY.position.y));
                    newResource.transform.localPosition = newPos;
                }
            }
            yield return new WaitForSeconds(respawnTimer);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        resourceFolder = new GameObject("Resource Folder");
        StartCoroutine(RespawnResource());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
