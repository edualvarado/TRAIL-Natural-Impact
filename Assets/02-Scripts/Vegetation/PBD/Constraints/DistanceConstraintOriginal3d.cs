using System;
using System.Collections.Generic;
using UnityEngine;

using Common.Mathematics.LinearAlgebra;

using PositionBasedDynamics.Bodies;

namespace PositionBasedDynamics.Constraints
{

    public class DistanceConstraintOriginal3d : Constraint3d
    {

        private double RestLength;

        private double CompressionStiffnessOriginal;

        private double StretchStiffnessOriginal;

        private readonly int i0;

        internal DistanceConstraintOriginal3d(Body3d body, int i0, double stiffness) : base(body)
        {
            this.i0 = i0;

            CompressionStiffnessOriginal = stiffness;
            StretchStiffnessOriginal = stiffness;
            RestLength = Body.PositionsOriginal[i0].Magnitude;
        }

        internal override void ConstrainPositions(double di)
        {
            double mass = Body.ParticleMass;
            double invMass = 1.0 / mass;
            double sum = mass / 2.0; // Initially was mass * 2, shouldn't be / 2?

            Vector3d n = Body.Predicted[i0];
            double d = n.Magnitude;
            n.Normalize();

            Debug.Log("d: " + d + " RestLength: " + RestLength);

            Vector3d corr;
            if (d < RestLength)
                corr = CompressionStiffnessOriginal * n * (d - RestLength) * sum;
            else
                corr = StretchStiffnessOriginal * n * (d - RestLength) * sum;

            Body.Predicted[i0] += invMass * corr * di;

        }

    }

}
