using System;
using System.Collections.Generic;
using UnityEngine;

using Common.Mathematics.LinearAlgebra;

using PositionBasedDynamics.Bodies;

namespace PositionBasedDynamics.Collisions
{
    internal class BodyBodyContactExternal3d : CollisionContactExternal3d
    {
        private Body3d Body1;
        private Rigidbody Body2;

        private int i1;
        //private int i2;

        private double Diameter, Diameter2;
        private double mass1, mass2;

        private Vector3d delta;

        public double Mass1
        {
            get => mass1;
            set => mass1 = value;
        }
        
        internal BodyBodyContactExternal3d(Body3d body1, int i1, Rigidbody body2)
        {
            Body1 = body1;
            this.i1 = i1;

            Body2 = body2;

            //Diameter = Body1.ParticleRadius + Body1.ParticleRadius;
            //Diameter2 = Diameter * Diameter;

            // TODO FIX
            double sum = Body1.ParticleMass + Body2.mass;
            mass1 = Body1.ParticleMass / sum;
            mass2 = Body2.mass / sum;
        }
        
        internal override void ResolveContactExternal(double di, bool usePenetrationDistance, float collisionFixedStep)
        {
            Vector3d normal = new Vector3d(Body1.ExternalHit[i1].normal.x, Body1.ExternalHit[i1].normal.y, Body1.ExternalHit[i1].normal.z);

            double sqLen = normal.SqrMagnitude;

            if (Body1.IsContact[i1] && sqLen > 1e-9)
            {
                double len = Math.Sqrt(sqLen);
                normal /= len;

                // Two options: fixed step or penetration distance
                if (!usePenetrationDistance)
                {
                    // 1. Fixed Step
                    // From 0.001 to 0.0001
                    delta = di * (collisionFixedStep) * normal; // Using step size as penetration distance  
                }
                else
                {
                    // 2. Penetration Distance
                    delta = di * (Body1.PenetrationDistance[i1] * Time.fixedDeltaTime) * normal;
                }

                //delta = di * (0.001f) * normal; // For feet only 

                Body1.Predicted[i1] += delta * 1.0f; // mass1
                Body1.Positions[i1] += delta * 1.0f; // mass1

                //Body2.Predicted[i1] -= delta * Mass2;
                //Body2.Positions[i1] -= delta * Mass2;
            }
        }

        internal override double PrintMass()
        {
            return mass1;
        }
    }
}