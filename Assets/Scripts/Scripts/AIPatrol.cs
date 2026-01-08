using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIPatrol : MonoBehaviour
{
    enum States {IDLE, PATROL, CHASE}
    States currentState;

    NavMeshAgent agent;

    [SerializeField] float sightRange, attackRange;
    List<Vector3> Nodes = new List<Vector3>();
    int destinationNode = 0;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        currentState = States.IDLE;
        StartCoroutine(SM());
        CollectNodes();
    }

    IEnumerator SM()
    {
        while (true)
        {
            yield return StartCoroutine(currentState.ToString());
        }
    }

    IEnumerator IDLE()
    {
        //ENTER STATE, RUN FIRST LOGIC
        Debug.Log("Just sitting here...");
        //EXECUTE STATE BEHAVIOUR
        while (currentState == States.IDLE)
        {
            yield return new WaitForSeconds(1.0f);

            Debug.Log("waiting...");

            yield return new WaitForSeconds(2.0f);

            Debug.Log("...boooooooooooored");
            currentState = States.PATROL;
            yield return null;

        }
        //EXIT THE STATE

    }
    IEnumerator PATROL()
    {
        //ENTER STATE, RUN FIRST LOGIC
        Debug.Log("guess I'll go to work then...");

        //EXECUTE STATE BEHAVIOUR
        while (currentState == States.PATROL)
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                yield return new WaitForSeconds(2.0f);
                GoToNextNode();
            }
            yield return null;
        }
        //EXIT THE STATE

    }


    // Update is called once per frame
    void Update()
    {
        
    }
    void GoToNextNode()
    {
        //check if there are no nodes
        if (Nodes.Count == 0)
        {
            return;
        }
        //set destination to current node
        agent.destination = Nodes[destinationNode];
        //choose the next node in the list and loop back to zero if max is reached.
        destinationNode = (destinationNode + 1) % Nodes.Count;

    }
    void CollectNodes()
    {
        foreach (Transform waypoint in GetComponentsInChildren<Transform>())
        {
            if (waypoint.CompareTag("Waypoint"))
            {
                Nodes.Add(waypoint.position);
                Destroy(waypoint.gameObject);
            }
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        foreach (Transform waypoint in GetComponentsInChildren<Transform>())
        {
            if (waypoint.CompareTag("Waypoint"))
            {
                Gizmos.DrawSphere(waypoint.position, 0.3f);
            }
        }
    }
}
