using System;
using System.Collections.Generic;
using UnityEngine;

using Common.Mathematics.LinearAlgebra;

using PositionBasedDynamics.Bodies;

namespace PositionBasedDynamics.Constraints
{

    public class BendingConstraintOriginal3d : Constraint3d
    {

        private double RestLength { get; set; }

        private double StiffnessOriginal { get; set; }

        // TEST
        private double Diff { get; set; }
        private double CurrentAngle { get; set; }
        private float CurrentAngleAlt { get; set; }
        private float ThresholdAngle { get; set; }

        private readonly int i0, i1, i2;

        internal BendingConstraintOriginal3d(Body3d body, int i0, int i1, int i2, double stiffness, float threshold) : base(body)
        {
            this.i0 = i0;
            this.i1 = i1;
            this.i2 = i2;
            
            StiffnessOriginal = stiffness;
            ThresholdAngle = threshold;

            Vector3d center = (Body.Positions[i0] + Body.Positions[i1] + Body.Positions[i2]) / 3.0;
            RestLength = (Body.Positions[i2] - center).Magnitude;
        }

        internal override void ConstrainPositions(double di)
        {
            Vector3d center = (Body.PositionsOriginal[i0] + Body.PositionsOriginal[i1] + Body.PositionsOriginal[i2]) / 3.0;
            Vector3d dirCenter = Body.PositionsOriginal[i2] - center;

            double distCenter = dirCenter.Magnitude;
            double diff = 1.0 - (RestLength / distCenter); // X
            Diff = diff;

            double mass = Body.ParticleMass;
            double w = mass + mass * 2.0f + mass;

            Vector3d dirForce = dirCenter * diff;

            Vector3d fa = StiffnessOriginal * (2.0 * mass / w) * dirForce * di;
            Body.Predicted[i0] += fa;

            Vector3d fb = StiffnessOriginal * (2.0 * mass / w) * dirForce * di;
            Body.Predicted[i1] += fb;

            Vector3d fc = -StiffnessOriginal * (4.0 * mass / w) * dirForce * di;
            Body.Predicted[i2] += fc;
        }
    }
}
