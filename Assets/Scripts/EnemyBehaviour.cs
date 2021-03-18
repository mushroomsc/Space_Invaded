using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    [Header("Attributes")]
    public float maxHealth;
    public float speed;

    [Header("Attacking")]
    public GameObject bulletPrefab;
    public float attackTimer;
    public float range;
    public float damage;
    public LayerMask turretMask;

    [Header("Pathing")]
    public List<Vector3> waypoints;
    public int currWaypointIndex = 0;

    [Header("Obstacle Avoidance")]
    public float raycastOffset;
    public LayerMask obstaclesMask;
    public float rayDistance;
    public float rotationSpeed = 2f;

    private Rigidbody ourBody;
    private HealthSystem ourHealth;
    private float currAttackTimer;

    // Start is called before the first frame update
    void Start()
    {
        ourBody = GetComponent<Rigidbody>();
        ourHealth = GetComponent<HealthSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        currAttackTimer += Time.deltaTime;

        List<GameObject> nearbyTowers = GetNearbyTowers();

        if (nearbyTowers.Count > 0)
        {
            if (currAttackTimer > attackTimer)
            {
                ourBody.velocity = Vector3.zero;
                AttackClosestTower(nearbyTowers);
                currAttackTimer = 0;
            }
        }
        else
        {
            Vector3 newDirection;
            if (DetectObstacles(out newDirection))
            {
                AvoidObstacles(newDirection);
            }
            else
            {
                LookAtNextWaypoint();
            }
            Move();
            if (currWaypointIndex < waypoints.Count - 1 && (waypoints[currWaypointIndex] - transform.position).magnitude < 5)
            {
                currWaypointIndex++;
            }
        }
    }

    private List<GameObject> GetNearbyTowers()
    {
        List<GameObject> nearbyTowers = new List<GameObject>();
        Collider[] nearbyTowerColliders = Physics.OverlapSphere(transform.position, range, turretMask);
        for (int i = 0; i < nearbyTowerColliders.Length; i++)
        {
            nearbyTowers.Add(nearbyTowerColliders[i].gameObject);
        }
        return nearbyTowers;
    }

    private void AttackClosestTower(List<GameObject> nearbyTowers)
    {
        int nearestTowerIndex = 0;
        float closestDistance = 50;
        for (int i = 0; i < nearbyTowers.Count; i++)
        {
            float distance = Mathf.Abs((nearbyTowers[i].transform.position - transform.position).magnitude);
            if (distance < closestDistance)
            {
                nearestTowerIndex = i;
            }
        }
        transform.LookAt(nearbyTowers[nearestTowerIndex].transform.position, Vector3.up);
        Shoot();
    }

    private void Shoot()
    {
        BulletBehaviour newBullet = Instantiate(bulletPrefab, transform.position + transform.forward, transform.rotation).GetComponent<BulletBehaviour>();
        newBullet.speed = 10;
        newBullet.damage = damage;
        newBullet.timeToLive = 5;
        newBullet.isFriendly = false;
    }

    private void LookAtNextWaypoint()
    {
        Quaternion rotation = Quaternion.LookRotation(waypoints[currWaypointIndex] - transform.position, transform.up);
        Turn(rotation);
    }

    private void Move()
    {
        ourBody.velocity = transform.forward.normalized * speed;
    }

    private void Turn(Quaternion rotation)
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }

    private bool DetectObstacles(out Vector3 newDirection)
    {
        Vector3 forwardRayPos = transform.position;
        Vector3 leftRayPos = transform.position - transform.right * raycastOffset;
        Vector3 rightRayPos = transform.position + transform.right * raycastOffset;
        Vector3 upRayPos = transform.position + transform.up * raycastOffset;
        Vector3 downRayPos = transform.position - transform.up * raycastOffset;

        /*
        Debug.DrawRay(forwardRayPos, transform.forward * rayDistance, Color.red);
        Debug.DrawRay(leftRayPos, transform.forward * rayDistance, Color.red);
        Debug.DrawRay(rightRayPos, transform.forward * rayDistance, Color.red);
        Debug.DrawRay(upRayPos, transform.forward * rayDistance, Color.red);
        Debug.DrawRay(downRayPos, transform.forward * rayDistance, Color.red);
        */

        RaycastHit hit;
        bool obstacleDetected = false;
        newDirection = Vector3.zero;
        if (Physics.Raycast(forwardRayPos, transform.forward, out hit, rayDistance, obstaclesMask))
        {
            obstacleDetected = true;
        }
        if (Physics.Raycast(leftRayPos, transform.forward, out hit, rayDistance, obstaclesMask))
        {
            obstacleDetected = true;
            newDirection += Vector3.right;
        }
        if (Physics.Raycast(rightRayPos, transform.forward, out hit, rayDistance, obstaclesMask))
        {
            obstacleDetected = true;
            newDirection += Vector3.left;
        }
        if (Physics.Raycast(upRayPos, transform.forward, out hit, rayDistance, obstaclesMask))
        {
            obstacleDetected = true;
            newDirection += Vector3.down;
        }
        if (Physics.Raycast(downRayPos, transform.forward, out hit, rayDistance, obstaclesMask))
        {
            obstacleDetected = true;
            newDirection += Vector3.up;
        }
        return obstacleDetected;
    }

    private void AvoidObstacles(Vector3 newDirection)
    {
        Quaternion rotation;
        if (newDirection != Vector3.zero)
        {
            rotation = Quaternion.LookRotation(newDirection, transform.up);
        }
        else
        {
            float rand = Random.Range(0f, 1f);
            if (rand < 0.25)
            {
                rotation = Quaternion.LookRotation(Vector3.left, transform.up);
            }
            else if (rand < 0.5)
            {
                rotation = Quaternion.LookRotation(Vector3.right, transform.up);
            }
            else if (rand < 0.75)
            {
                rotation = Quaternion.LookRotation(Vector3.up, transform.up);
            }
            else
            {
                rotation = Quaternion.LookRotation(Vector3.down, transform.up);
            }
        }
        Turn(rotation);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<WaypointBehaviour>() != null)
        {
            currWaypointIndex++;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<DefencePointBehaviour>() != null)
        {
            References.defencePointObject.currBaseHealth -= 1;
            ourHealth.TakeDamage(maxHealth);
        }
    }

    public virtual void Die()
    {
        Destroy(gameObject);
    }
}