/****************************************************
 * File: AIDestinationSetter.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/10/2020
   * Project: Foot2Trail
   * Last update: 21/02/2023
*****************************************************/

using UnityEngine;
using UnityEngine.AI;

namespace MxMGameplay
{
    public class AIDestinationSetter : MonoBehaviour
    {
        #region Instance Fields

        [Header("Periodic Straight/Random")]
        [SerializeField] public GameObject myTerrain;
        [SerializeField] private Transform target = null;
        [SerializeField] private float frequencyChangeTarget = 5f;
        [SerializeField] private float radius = 1f;
        public bool straightLine;
        public Vector3 direction;
        public int counterPasses = 0;
        
        [Header("Circle")]
        public bool circle;
        public float radiusCircle = 1.0f; // Radius of the circle
        public float speedCircle = 1.0f; // Speed of movement (in radians per second)
        public Vector3 directionCircle = Vector3.forward; // Direction of movement (in the plane perpendicular to this vector)
        public Vector3 centerCircle;
        public float startAngle = 0.0f; // Starting angle (in radians)

        private float angle = 0.0f; // Current angle (in radians)

        private static int index = 0;
        private static Vector3[] directions = new Vector3[4];

        #endregion

        #region Read-only & Static Fields

        private NavMeshAgent m_navAgent;
        private float m_moveTimer = 0f;
        
        #endregion

        private void Start()
        {
            m_navAgent = GetComponent<NavMeshAgent>();
            m_navAgent.SetDestination(target.position);

            angle = startAngle * Mathf.Deg2Rad;

            // Initialize the directions array
            directions[0] = new Vector3(direction.x, 0f, direction.z);
            directions[1] = new Vector3(direction.x, 0f, -direction.z);
            directions[2] = new Vector3(-direction.x, 0f, direction.z);
            directions[3] = new Vector3(-direction.x, 0f, -direction.z);

        }

        public void Update()
        {
            m_moveTimer += Time.deltaTime;

            if (m_navAgent && target)
            {
                if (!circle)
                {
                    if (m_moveTimer > frequencyChangeTarget)
                    {
                        Vector3 destination = RandomNavmeshLocation(radius);

                        target.position = destination;
                        m_navAgent.SetDestination(destination);

                        m_moveTimer = 0f;
                        counterPasses++;
                    } 
                }
                else
                {
                    Vector3 destination = RandomNavmeshLocation(radiusCircle);
                    
                    target.position = destination;
                    m_navAgent.SetDestination(destination);

                    m_moveTimer = 0f;
                }
            }
        }

        public Vector3 RandomNavmeshLocation(float radius)
        {
            Vector3 randomDirection = Vector3.zero;
            if (!straightLine && !circle)
            {
                randomDirection = new Vector3(5f, 1f, 5f) + Random.insideUnitSphere * radius; // TODO: Corresponding height in the terrain in that point
            }
            else if (straightLine && !circle)
            {
                //randomDirection = new Vector3(5 + directionAlt.x, 1f, 5 + direction.z);
                //direction = -direction;

                // ---

                Vector3 dir = directions[index];
                randomDirection = new Vector3(5 + dir.x, 1f, 5 + dir.z);
                index = (index + 1) % 4;
            }
            else if(!straightLine && circle)
            {
                Vector3 newPos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
                Vector3 newPosAlt = centerCircle + newPos;


                Quaternion rotation = Quaternion.AngleAxis(Vector3.SignedAngle(Vector3.forward, directionCircle, Vector3.up), Vector3.up);

                randomDirection = rotation * (newPosAlt - centerCircle) + centerCircle;

                // Update angle for next frame
                angle += speedCircle * Time.deltaTime;
            }
            else if (straightLine && circle)
            {
                Vector3 newPos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
                Vector3 newPosAlt = centerCircle + newPos;


                Quaternion rotation = Quaternion.AngleAxis(Vector3.SignedAngle(Vector3.forward, directionCircle, Vector3.up), Vector3.up);

                randomDirection = rotation * (newPosAlt - centerCircle) + centerCircle;

                // Update angle for next frame
                angle += speedCircle * Time.deltaTime;
            }

            NavMeshHit hit;
            Vector3 finalPosition = Vector3.zero;
            if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
            {
                finalPosition = hit.position;
            }
            return finalPosition;
        }
    }
}
