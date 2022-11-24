using UnityEngine.AI;
using UnityEngine;

public class zombieManager : MonoBehaviour
{
    private NavMeshAgent zombieNavAgent;
    private int pathCounter;
    public int health;
    private Animator zombieAnimator;
    [SerializeField] private Transform[] pathPoints;
    
    void Start()
    {
        zombieNavAgent = GetComponent<NavMeshAgent>();
        zombieAnimator = GetComponent<Animator>();
        health = 2;
    }

    
    void Update()
    {
        if (zombieNavAgent.enabled)
        {
            if (!zombieNavAgent.pathPending && zombieNavAgent.remainingDistance <= zombieNavAgent.stoppingDistance
                                            && zombieNavAgent.velocity.sqrMagnitude == 0f)
            {
                if (pathCounter < pathPoints.Length - 1)
                {
                    pathCounter++;
                }
                else
                {
                    pathCounter = 0;
                }

                if (health == 2)
                    zombieNavAgent.SetDestination(pathPoints[pathCounter].position);

            }
        }
        else
        {
            zombieAnimator.SetBool("idle",true);
        }
        
        
    }
}
