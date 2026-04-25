using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ObjectProceduralGeneration : MonoBehaviour
{
    [SerializeField] private GameObject _objectPrefab;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private int amount;
    void Start()
    {
        SpawnRandom();
    }
    public void SpawnRandom()
    {
        if (amount > _spawnPoints.Length)
        {
            Debug.LogWarning("spawnCount es mayor que la cantidad de spawnPoints!");
            return;
        }

        List<Transform> shuffledList = _spawnPoints.OrderBy(_ => Random.value).ToList();

        for(int i = 0; i < amount; i++)
        {
            Instantiate(_objectPrefab, shuffledList[i].position, shuffledList[i].rotation);
        }
    }  
}
