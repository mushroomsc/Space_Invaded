using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerBehaviour : MonoBehaviour
{
    public GameObject waypointObject;
    public float spawnTimer;

    private float currTimer;
    private List<Vector3> waypoints = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in waypointObject.transform)
        {
            waypoints.Add(child.position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        currTimer += Time.deltaTime;
        if (currTimer > spawnTimer)
        {
            Debug.Log(References.enemyTypes);
            Debug.Log(References.numEnemyTypes);
            Debug.Log(Random.Range(0, References.numEnemyTypes));
            GameObject newEnemy = Instantiate(References.enemyTypes[Random.Range(0, References.numEnemyTypes)], transform.position, transform.rotation);
            EnemyBehaviour newEnemyBehaviour = newEnemy.GetComponent<EnemyBehaviour>();
            HealthSystem newEnemyHealthSystem = newEnemy.GetComponent<HealthSystem>();
            newEnemyBehaviour.waypoints = waypoints;
            currTimer = 0;
        }
    }
}