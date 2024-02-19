/****************************************************
 * File: Body3d.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/10/2020
   * Project: Foot2Trail
   * Last update: 21/02/2023
*****************************************************/

// - [X] Checked!

using System;
using System.Collections.Generic;
using UnityEngine;
using Common.Mathematics.LinearAlgebra;
using Common.Geometry.Shapes;
using PositionBasedDynamics.Constraints;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace PositionBasedDynamics.Bodies
{

    // TEST JOBS
    public struct Body3dStruct
    {
        public int NumParticles;
        public NativeArray<Vector3d> Positions;
        public NativeArray<Vector3d> Velocities;
        public NativeArray<Vector3d> Predicted;


        public void SetPositions(Vector3d[] positions, Allocator allocator)
        {
            if (Positions.IsCreated)
            {
                Positions.Dispose();
            }

            Positions = new NativeArray<Vector3d>(positions, allocator);
        }

        public void SetVelocities(Vector3d[] velocities, Allocator allocator)
        {
            if (Velocities.IsCreated)
            {
                Velocities.Dispose();
            }

            Velocities = new NativeArray<Vector3d>(velocities, allocator);
        }

        public void SetPredicted(Vector3d[] predicted, Allocator allocator)
        {
            if (Predicted.IsCreated)
            {
                Predicted.Dispose();
            }

            Predicted = new NativeArray<Vector3d>(predicted, allocator);
        }
    }

    public abstract class Body3d
    {
        #region Instance Properties - Body Properties

        public int NumParticles { get { return Positions.Length; } }
        public int NumConstraints { get { return Constraints.Count; } }
        public double ParticleRadius { get; protected set; }
        public double ParticleDiameter { get { return ParticleRadius * 2.0; } }
        public double ParticleMass { get; protected set; }
        public double Dampning { get; set; }
        public int Id { get; set; }

        #endregion

        #region Instance Properties - Body State

        public Vector3d[] Positions { get; set; } // REMOVED PRIVATE SET TEST     
        public Vector3d[] PositionsOriginal { get; private set; } // TEST
        public Vector3d[] Predicted { get; set; } // REMOVED PRIVATE SET TEST     
        public Vector3d[] Velocities { get; set; } // REMOVED PRIVATE SET TEST     

        #endregion

        #region Instance Properties - Body Contacts

        public bool[] IsContact { get; set; }
        public ContactPoint[] ExternalHit { get; set; }
        public float[] PenetrationDistance { get; set; }

        #endregion

        #region Instance Properties - Body Others

        public bool ActivateSolver { get; set; } // TEST
        public bool[] IsBroken { get; set; } // TEST
        public bool[] IsStatic { get; set; } // TEST
        public float TotalVerticalBendingConstraints { get; set; } // TEST
        public float BrokenVerticalBendingConstraints { get; set; } // TEST

        #endregion

        #region Instance Properties - Body Bounds

        public Box3d Bounds { get; private set; }
        public Box3d StaticBounds { get; set; }

        #endregion

        #region Instance Properties - Body Constraints
        
        public List<Constraint3d> Constraints { get; private set; }
        public List<Constraint3d> BendingConstraintsHorizontal { get; private set; } // TEST
        public List<Constraint3d> BendingConstraintsVertical { get; private set; } // TEST
        private List<StaticConstraint3d> StaticConstraints { get; set; }

        // TEST
        public List<Constraint3d> BrokenConstraints { get; private set; }
        // TEST
        public float timer;

        #endregion

        public Body3d(int numParticles, double radius, double mass)
        {
            //Debug.Log("[INFO] Creating Body3d");
            
            Positions = new Vector3d[numParticles];
            PositionsOriginal = new Vector3d[numParticles];
            Predicted = new Vector3d[numParticles];
            Velocities = new Vector3d[numParticles];
            
            IsContact = new bool[numParticles];
            ExternalHit = new ContactPoint[numParticles];
            PenetrationDistance = new float[numParticles];
            
            IsBroken = new bool[numParticles];
            IsStatic = new bool[numParticles];
            
            Constraints = new List<Constraint3d>();
            BrokenConstraints = new List<Constraint3d>(); // TEST
            BendingConstraintsHorizontal = new List<Constraint3d>();
            BendingConstraintsVertical = new List<Constraint3d>();
            StaticConstraints = new List<StaticConstraint3d>();
            
            TotalVerticalBendingConstraints = 0;
            BrokenVerticalBendingConstraints = 0;

            ParticleRadius = radius;
            ParticleMass = mass;
            Dampning = 1;

            if (ParticleMass <= 0)
                throw new ArgumentException("Particles mass <= 0");

            if (ParticleRadius <= 0)
                throw new ArgumentException("Particles radius <= 0");
        }

        #region Internal Methods

        internal void ConstrainPositions(double di)
        {
            for (int i = 0; i < Constraints.Count; i++)
            {
                Constraints[i].ConstrainPositions(di);
            }

            for (int i = 0; i < StaticConstraints.Count; i++)
            {
                StaticConstraints[i].ConstrainPositions(di);
            }
        }

        internal void ConstrainVelocities()
        {

            for (int i = 0; i < Constraints.Count; i++)
            {
                Constraints[i].ConstrainVelocities();
            }

            for (int i = 0; i < StaticConstraints.Count; i++)
            {
                StaticConstraints[i].ConstrainVelocities();
            }

        }

        internal void RemoveVerticalBendingConstrainPositions()
        {
            for (int i = 0; i < BendingConstraintsVertical.Count; i++)
            {
                BendingConstraintsVertical[i].RemoveVerticalConstrainPositions();
            }
        }

        internal void DegradateVerticalPlant()
        {
            for (int i = 0; i < BendingConstraintsVertical.Count; i++)
            {
                BendingConstraintsVertical[i].ModifyStepWiseDegradation();
            }
        }
        
        internal void RecoverVerticalBendingConstrainPositions()
        {
            // If there are broken constraints, we try to recover the first one.
            if (BrokenConstraints.Count > 0)
            {
                BrokenConstraints[0].RecoverVerticalConstrainPositions(BrokenConstraints);
            }
        }
            
        #endregion

        #region Public Methods

        public void RandomizePositions(System.Random rnd, double amount)
        {
            for(int i = 0; i < NumParticles; i++)
            {
                double rx = rnd.NextDouble() * 2.0 - 1.0;
                double ry = rnd.NextDouble() * 2.0 - 1.0;
                double rz = rnd.NextDouble() * 2.0 - 1.0;

                Positions[i] += new Vector3d(rx, ry, rz) * amount;
            }
        }

        public void RandomizeConstraintOrder(System.Random rnd)
        {
            int count = Constraints.Count;
            if (count <= 1) return;

            List<Constraint3d> tmp = new List<Constraint3d>();

            while (tmp.Count != count)
            {
                int i = rnd.Next(0, Constraints.Count - 1);

                tmp.Add(Constraints[i]);
                Constraints.RemoveAt(i);
            }

            Constraints = tmp;
        }

        public void MarkAsStatic(Box3d bounds)
        {
            for (int i = 0; i < NumParticles; i++)
            {
                if (bounds.Contains(Positions[i]))
                {
                    IsStatic[i] = true;
                    StaticConstraints.Add(new StaticConstraint3d(this, i));
                }
            }
        }

        public void UpdateBounds()
        {
            Vector3d min = new Vector3d(double.PositiveInfinity);
            Vector3d max = new Vector3d(double.NegativeInfinity);

            for (int i = 0; i < NumParticles; i++)
            {
                min.Min(Positions[i]);
                max.Max(Positions[i]);
            }

            min -= ParticleRadius;
            max += ParticleRadius;

            Bounds = new Box3d(min, max);
        }

        #endregion
    }
}