using System;
using System.Collections.Generic;
using UnityEngine;

using Common.Mathematics.LinearAlgebra;

using PositionBasedDynamics.Bodies;
using System.Collections;

namespace PositionBasedDynamics.Constraints
{

    public class BendingConstraint3d : Constraint3d
    {
        #region Read-only & Static Fields

        private double RestLength { get; set; }
        public double Stiffness { get; set; }
        public double MinStiffness { get; set; }
        public double MaxStiffness { get; set; }
        private bool[] MinStiffnessUpdated { get; set; }

        private double Diff { get; set; }
        private float CurrentAngle { get; set; }
        private float BreakingThresholdAngle { get; set; }
        private float DegradationThresholdAngle { get; set; }
        private bool[] StiffnessUpdated { get; set; }

        private float RecuperationTimeStiffness { get; set; }
        private float StepStiffness { get; set; }

        private float GrowingTime { get; set; }


        private readonly int i0, i1, i2;

        // TEST
        public float timer;
        // TEST
        public double CurrentBodyStiffness { get; set; }
        public double StiffnessRatio { get; set; }

        #endregion

        #region Instance Properties

        public override int[] ParticleConstrained { get; set; }
        public override Body3d BodyConstrained { get; set; }
        public override double CurrentStiffness { get; set; }

        #endregion

        internal BendingConstraint3d(Body3d body, int i0, int i1, int i2, double stiffness, float breakingThreshold, float degradationThreshold, float recuperationTimeStiffness, float stepStiffness, float growingTime) : base(body)
        {
            this.i0 = i0;
            this.i1 = i1;
            this.i2 = i2;

            Stiffness = stiffness;
            CurrentStiffness = Stiffness;

            MaxStiffness = Stiffness;
            MinStiffness = Stiffness;

            BreakingThresholdAngle = breakingThreshold;
            DegradationThresholdAngle = degradationThreshold;

            ParticleConstrained = new int[3];
            ParticleConstrained[0] = i0;
            ParticleConstrained[1] = i1;
            ParticleConstrained[2] = i2;

            BodyConstrained = body;

            StiffnessUpdated = new bool[body.NumParticles];
            MinStiffnessUpdated = new bool[body.NumParticles];

            RecuperationTimeStiffness = recuperationTimeStiffness;
            StepStiffness = stepStiffness;

            GrowingTime = growingTime;

            Vector3d center = (Body.Positions[i0] + Body.Positions[i1] + Body.Positions[i2]) / 3.0;
            RestLength = (Body.Positions[i2] - center).Magnitude;
        }

        #region Overriding Methods

        internal override void ConstrainPositions(double di)
        {
            // Update CurrentStiffness to access from VegetationCreator
            CurrentStiffness = Stiffness;

            Vector3d center = (Body.Predicted[i0] + Body.Predicted[i1] + Body.Predicted[i2]) / 3.0;
            Vector3d dirCenter = Body.Predicted[i2] - center;

            double distCenter = dirCenter.Magnitude;
            Diff = 1.0 - (RestLength / distCenter); ;
            CurrentAngle = Vector3.Angle((Body.Predicted[i0] - Body.Predicted[i1]).ToVector3(), (Body.Predicted[i2] - Body.Predicted[i1]).ToVector3()); ;

            double mass = Body.ParticleMass;
            double w = mass + mass * 2.0f + mass;

            Vector3d dirForce = dirCenter * Diff;

            Vector3d fa = Stiffness * (2.0 * mass / w) * dirForce * di;
            Body.Predicted[i0] += fa;
            Vector3d fb = Stiffness * (2.0 * mass / w) * dirForce * di;
            Body.Predicted[i1] += fb;
            Vector3d fc = -Stiffness * (4.0 * mass / w) * dirForce * di;
            Body.Predicted[i2] += fc;
        }

        internal override void RemoveVerticalConstrainPositions()
        {
            if ((180f - CurrentAngle) > BreakingThresholdAngle)
            {
                //Debug.Log("Remove between: " + i0 + ", " + i1 + " and " + i2);
                //Debug.Log("Arcos(diff): " + (Math.Acos(Diff) * 180.0 / Math.PI));
                //Debug.Log("currentAngleAlt : " + (180f - CurrentAngle));

                // TEST - Setting stiffness to zero if it is broken, before adding to the list
                Stiffness = 0;
                CurrentStiffness = 0;
    
                // Add to list of broken constraints
                Body.BrokenConstraints.Add(this);

                // Remove from general list
                Body.IsBroken[i1] = true;
                Body.Constraints.Remove(this);

                // Break only vertical bending constraints
                Body.BendingConstraintsVertical.Remove(this);
                Body.BrokenVerticalBendingConstraints++;
            }
        }

        internal override void RecoverVerticalConstrainPositions(List<Constraint3d> BrokenConstraints)
        {
            // TEST - Setting stiffness to zero if it is broken, before adding to the list
            //Debug.Log("this.CurrentStiffness BEFORE: " + CurrentStiffness);
            Stiffness = 0;
            CurrentStiffness = 0;
            //Debug.Log("this.CurrentStiffness AFTER: " + CurrentStiffness);

            // TEST - Iterate over all living remaining constraints and estimate the current stiffness of the plant - Not needed if you take it from VegetationCreator.cs
            //CurrentBodyStiffness = 0f;
            //for (int k = 0; k < Body.BendingConstraintsVertical.Count; k++)
            //{
            //    Debug.Log("SUMMING (" + k + "): " + (float)Body.BendingConstraintsVertical[k].CurrentStiffness);
            //    CurrentBodyStiffness += (float)Body.BendingConstraintsVertical[k].CurrentStiffness;
            //}

            CurrentBodyStiffness = VegetationCreator.currentStiffnessProxis[Body.Id];
            //Debug.Log("CurrentBodyStiffness from VegetationCreator.cs: " + CurrentBodyStiffness);

            // Get stiffness ratio with respect to remaining living constraints
            StiffnessRatio = CurrentBodyStiffness / ((float)Body.BendingConstraintsVertical.Count * 1f); // TEST ------ WARNING!!!!!!!!! HARDCODING MAX STIFFNESS HERE FOR TESTING
            //Debug.Log("CurrentBodyStiffness: " + CurrentBodyStiffness + " / Body.BendingConstraintsVertical.Count * 1f: " + Body.BendingConstraintsVertical.Count + " = StiffnessRatio: " + StiffnessRatio);

            if (StiffnessRatio > 0.99) // Point from which constraints begin to recover
            {
                // Remove the broken constraints from its list
                BrokenConstraints.Remove(this);

                // It is not broken anymore - adding it to the contraints list
                Body.IsBroken[i1] = false;

                // Recover constraints
                Body.Constraints.Add(this);
                Body.BendingConstraintsVertical.Add(this);
                Body.BrokenVerticalBendingConstraints--;

                // ----
                
                /*
                timer += Time.deltaTime;

                // How much each constraints needs to recover!
                if (timer > GrowingTime)
                {
                    //Debug.Log("BEFORE BrokenConstraints.Count: " + BrokenConstraints.Count);

                    // Remove the broken constraints from its list
                    BrokenConstraints.Remove(this);

                    // It is not broken anymore - adding it to the contraints list
                    Body.IsBroken[i1] = false;

                    // OPTION 1
                    
                    Body.Constraints.Add(this);
                    // Update value used to estimate living ratio
                    Body.BendingConstraintsVertical.Add(this);
                    Body.BrokenVerticalBendingConstraints--;

                    timer = 0.0f;
                } 
                */
            }
        }

        internal override void ModifyStepWiseDegradation()
        {
            //Debug.Log("[INFO] ModifyStiffness() called");
            
            //Debug.Log("i0 in contact: " + Body.IsContact[i0]);
            //Debug.Log("i1 in contact: " + Body.IsContact[i1]);
            //Debug.Log("i2 in contact: " + Body.IsContact[i2]);

            if (((180f - CurrentAngle) > DegradationThresholdAngle) && Body.IsContact[i0] && !MinStiffnessUpdated[i0])
            {
                //Debug.Log("[INFO] CONTACT! Set MinStiffness: " + MinStiffness);

                MinStiffness = Stiffness;
                MinStiffnessUpdated[i0] = true;
            }

            // Only enter if it is the first time we touch the particle in the constraint
            if (((180f - CurrentAngle) > DegradationThresholdAngle) && Body.IsContact[i0] && !StiffnessUpdated[i0])
            {
                //Debug.Log("[INFO] Contact! Stiffness Before: " + Stiffness + " an MinStiffness " + MinStiffness);

                // Performs step-wise degradation
                if (Stiffness >= StepStiffness)
                {
                    Stiffness -= StepStiffness;

                    // Update minimum value
                    if (Stiffness < MinStiffness)
                    {
                        MinStiffness = Stiffness;
                    }
                }

                //Debug.Log("[INFO] Contact! Stiffness After: " + Stiffness + " an MinStiffness " + MinStiffness);

                StiffnessUpdated[i0] = true;               
            }

            if (!Body.IsContact[i0])
            {
                StiffnessUpdated[i0] = false;
                MinStiffnessUpdated[i0] = false;

                // OPTION 1
                float difference = (float)MaxStiffness - (float)Stiffness;
                float increasePerSecond = difference / RecuperationTimeStiffness;

                if (Stiffness < (float)MaxStiffness)
                {
                    Stiffness += increasePerSecond * Time.deltaTime;
                }
            }
        }

        #endregion
    }
}
