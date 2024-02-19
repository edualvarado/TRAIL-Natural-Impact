using System;
using System.Collections.Generic;
using UnityEngine;


using Common.Mathematics.LinearAlgebra;

using PositionBasedDynamics.Bodies;

namespace PositionBasedDynamics.Collisions
{
    public class BodyCollisionExternal3d : CollisionExternal3d
    {
        private Body3d Body1 { get; set; }

        private int Particle1 { get; set; }

        private Rigidbody ExtBody { get; set; }

        public BodyCollisionExternal3d(Body3d body1, Rigidbody extBody)
        {
            Body1 = body1;
            ExtBody = extBody;
        }

        internal override void FindExternalContacts(IList<Body3d> bodies, IList<Rigidbody> externalBodies, List<CollisionContactExternal3d> externalContacts)
        {
            for (int j = 0; j < bodies.Count; j++)
            {
                for (int k = 0; k < externalBodies.Count; k++)
                {
                    Body3d body1 = bodies[j];
                    Rigidbody body2 = externalBodies[k];

                    int numParticles1 = body1.NumParticles;
                    double radius1 = body1.ParticleRadius;
                    
                    for (int x = 0; x < numParticles1; x++)
                    {

                        // if there is a contact of this particle with the external body, add it to the list
                        if (body1.IsContact[x])
                        {
                            //Debug.Log("[FindExternalContacts] Adding contact of Particle: " + x + ": " + body1.IsContact[x]);
                            //Debug.Log("[FindExternalContacts] Position Contact: " + body1.ExternalHit[x].point);

                            // It draws multiple rays
                            //Debug.DrawRay(body1.ExternalHit[x].point, body1.ExternalHit[x].normal, Color.cyan, 1f);

                            externalContacts.Add(new BodyBodyContactExternal3d(body1, x, body2));
                        }
                        else
                        {
                            //Debug.Log("[FindExternalContacts] x: " + x);
                            //Debug.Log("[FindExternalContacts] body1.IsContact[x]: " + body1.IsContact[x]);
                        }
                    }
                }
            }
        }
    }
}