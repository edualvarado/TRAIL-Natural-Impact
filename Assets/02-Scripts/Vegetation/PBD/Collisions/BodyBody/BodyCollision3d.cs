using System;
using System.Collections.Generic;

using Common.Mathematics.LinearAlgebra;

using PositionBasedDynamics.Bodies;

namespace PositionBasedDynamics.Collisions
{
    public class BodyCollision3d : Collision3d
    {
        private Body3d Body1 { get; set; }
        
        private int Particle1 { get; set; }

        private Body3d Body2 { get; set; }

        private int Particle2 { get; set; }

        public BodyCollision3d(Body3d body1, Body3d body2)
        {
            Body1 = body1;

            Body2 = body2;
        }

        internal override void FindContacts(IList<Body3d> bodies, List<CollisionContact3d> contacts)
        {

            for (int j = 0; j < bodies.Count; j++)
            {
                for (int k = 0; k < bodies.Count; k++)
                {
                    Body3d body1 = bodies[j];
                    Body3d body2 = bodies[k];

                    if (k != j)
                    {
                        int numParticles1 = body1.NumParticles;
                        double radius1 = body1.ParticleRadius;

                        int numParticles2 = body2.NumParticles;
                        double radius2 = body2.ParticleRadius;
                        
                        for (int x = 0; x < numParticles1; x++)
                        {
                            for (int y = 0; y < numParticles2; y++)
                            {
                                Vector3d distanceParticle = body1.Predicted[x] - body2.Predicted[y];

                                double d = Vector3d.Dot(distanceParticle, body1.Predicted[x]) + 0f - radius1;

                                if (d < 0.0)
                                    contacts.Add(new BodyBodyContact3d(body1, x, body2, y)); 
                            }

                        }
                    } 
                }
            }

        }

    }
}