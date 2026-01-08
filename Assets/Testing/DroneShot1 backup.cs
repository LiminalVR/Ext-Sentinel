using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class DroneShot1_backup : MonoBehaviour
{
    public GameObject cannonball;
    public GameObject[] barrellEnd;
    private float timer;
    public float timeBetweenShots;
    public GameObject player;
    private EnemyCannonBall cb;
    private int targetPicker;
    public Target[] targets;
    private int barrelPicker;
    private BoxCollider boxCollider;
    private int lastNumber;
    //[HideInInspector]
    public int currentNumber;
    public bool bossShip;
    [HideInInspector]
    public bool reverseShot;

    private void Awake()
    {
        boxCollider = GetComponentInChildren<BoxCollider>();
    }

    private void Start()
    {
        targets = TargetManager.Instance.targets;
        player = Cannon.Player;
    }

    // Update is called once per frame
    private void Update()
    {
        RaycastHit hit;
        timer += Time.deltaTime;
        if (!bossShip)
        {
            if (timer >= timeBetweenShots)
            {
                barrelPicker = GetRandom(0, barrellEnd.Length);

                if (currentNumber != barrelPicker)
                {
                    currentNumber = barrelPicker;

                    if (Physics.Raycast(barrellEnd[currentNumber].transform.position, player.transform.position - barrellEnd[currentNumber].transform.position, out hit, 500f))
                    {
                        if (hit.collider.CompareTag("Player"))
                        {
                            GameObject returnedGameObject = PoolManager.current.GetPooledObject(cannonball.name);
                            if (returnedGameObject == null) return;
                            cb = returnedGameObject.GetComponent<EnemyCannonBall>();
                            cb.startPos = barrellEnd[currentNumber].transform.position;
                            cb.transform.position = barrellEnd[currentNumber].transform.position;
                            cb.transform.rotation = barrellEnd[currentNumber].transform.rotation;
                            targetPicker = Random.Range(0, targets.Length);
                            cb.targetPos = targets[targetPicker].transform.position;
                            returnedGameObject.SetActive(true);
                            //cb.audioSource.Play();
                            timer = 0f;
                        }
                    }
                }
            }
        }

        if(bossShip)
        {
            if (timer >= timeBetweenShots)
            {
                GameObject returnedGameObject = PoolManager.current.GetPooledObject(cannonball.name);
                if (returnedGameObject == null) return;
                cb = returnedGameObject.GetComponent<EnemyCannonBall>();
                cb.startPos = barrellEnd[currentNumber].transform.position;
                cb.transform.position = barrellEnd[currentNumber].transform.position;
                cb.transform.rotation = barrellEnd[currentNumber].transform.rotation;
                targetPicker = Random.Range(0, targets.Length);
                cb.targetPos = targets[targetPicker].transform.position;
                returnedGameObject.SetActive(true);
                //cb.audioSource.Play();
                timer = 0f;
                if (currentNumber >= 0 && currentNumber < 2)
                {
                    currentNumber++;
                }
                else if(currentNumber >= 2)
                {
                    currentNumber = 0;
                }
            }
        }
    } 



    private void TimeBetweenShotsIncrease()
    {
        timeBetweenShots = 0.25f;
    }

    private void TimeBetweenShotsIncrease2()
    {
        timeBetweenShots = 0.1f;
    }

    private int GetRandom(int min, int max)
    {
        int rand = Random.Range(min, max);
        while (rand == lastNumber)
            rand = Random.Range(min, max);
        lastNumber = rand;
        return rand;
    }
}
