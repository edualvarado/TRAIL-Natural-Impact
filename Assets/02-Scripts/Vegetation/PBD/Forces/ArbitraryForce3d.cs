using System;
using System.Collections.Generic;

using Common.Mathematics.LinearAlgebra;

using PositionBasedDynamics.Bodies;

namespace PositionBasedDynamics.Forces
{

    public class ArbitraryForce3d : ExternalForce3d
    {

        public Vector3d ArbitraryForce { get; set; }

        public ArbitraryForce3d(Vector3d arbForce)
        {
            ArbitraryForce = arbForce;
        }

        public override void ApplyForce(double dt, Body3d body)
        {
            int len = body.NumParticles;
            for (int i = 0; i < len; i++)
            {
                body.Velocities[i] += dt * ArbitraryForce;
            }

        }
    }

}
