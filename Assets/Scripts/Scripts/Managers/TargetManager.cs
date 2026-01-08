using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    public Target[] targets;

    public static TargetManager Instance;

    void Awake()
    {
        Instance = this;
        
    }

    // Start is called before the first frame update
    void Start()
    {
        targets = transform.GetComponentsInChildren<Target>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDrawGizmos()
    {
        foreach (Target target in transform.GetComponentsInChildren<Target>())
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(target.transform.position, 1);
        }
    }
}
