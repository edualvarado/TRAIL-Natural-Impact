/****************************************************
 * File: Solver3d.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/10/2020
   * Project: Foot2Trail
   * Last update: 21/02/2023
*****************************************************/

using UnityEngine;
using System.Collections.Generic;

using Common.Mathematics.LinearAlgebra;

using PositionBasedDynamics.Forces;
using PositionBasedDynamics.Constraints;
using PositionBasedDynamics.Bodies;
using PositionBasedDynamics.Collisions;
using PositionBasedDynamics.Bodies.Cloth;

using System.Collections;
using Unity.Mathematics;

using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace PositionBasedDynamics.Solvers
{
    public class Solver3d
    {
        #region Instances Fields
        
        public int SolverIterations { get; set; }

        public int CollisionIterations { get; set; }

        public double SleepThreshold { get; set; }

        public List<Body3d> Bodies { get; private set; }

        public List<Rigidbody> ExternalBodies { get; private set; }

        private List<ExternalForce3d> Forces { get; set; }

        private List<Collision3d> Collisions { get; set; }

        private List<CollisionExternal3d> ExternalCollisions { get; set; }

        #endregion

        public Solver3d(int numParticles)
        {
            SolverIterations = 4;
            CollisionIterations = 2;

            Forces = new List<ExternalForce3d>();
            
            Collisions = new List<Collision3d>();
            ExternalCollisions = new List<CollisionExternal3d>();
            
            Bodies = new List<Body3d>();
            ExternalBodies = new List<Rigidbody>();
        }

        #region Public Methods - Add

        public void AddForce(ExternalForce3d force)
        {
            if (Forces.Contains(force)) return;
            Forces.Add(force);
        }

        public void AddCollision(Collision3d collision)
        {
            if (Collisions.Contains(collision)) return;
            Collisions.Add(collision);
        }

        public void AddExternalCollision(CollisionExternal3d externalCollision)
        {
            if (ExternalCollisions.Contains(externalCollision)) return;
            ExternalCollisions.Add(externalCollision);
        }

        public void AddBody(Body3d body)
        {
            if (Bodies.Contains(body)) return;
            Bodies.Add(body);
        }

        public void AddExternalBody(Rigidbody body)
        {
            if (ExternalBodies.Contains(body)) return;
            ExternalBodies.Add(body);
        }

        #endregion
        
        public void StepPhysics(int i, List<ClothBody3d> plantsList, double dt, bool useJobsInside, bool usePenetrationDistance, float collisionFixedStep, bool activateRegeneration)
        {
            if (dt == 0.0) return;

            // CAREFUL WITH ACTIVATE SOLVER
            //if (plantsList[i].ActivateSolver)
            //{
            //    //Debug.Log("Solver for plant: " + i);

            //    AppyExternalForces(dt, false);

            //    EstimatePositions(dt, useJobsInside);

            //    UpdateBounds();

            //    ResolveCollisions();

            //    ResolveExternalCollisions(usePenetrationDistance, collisionFixedStep); // Custom 
            //}

            //Debug.Log("Solver for plant: " + i);

            AppyExternalForces(dt, false);

            EstimatePositions(dt, useJobsInside);

            UpdateBounds();

            ResolveCollisions();

            ResolveExternalCollisions(usePenetrationDistance, collisionFixedStep); // Custom 

            //---

            ConstrainPositions(); // Constraining the body is always done

            //---

            //if (plantsList[i].ActivateSolver)
            //{
            //    UpdateVelocities(dt, false);

            //    UpdatePositions(false);

            //    UpdateBounds();

            //    RemoveConstrainPositions(); // Custom
            //}

            UpdateVelocities(dt, false);

            UpdatePositions(false);

            UpdateBounds();
            
            //---

            RemoveConstrainPositions(); // Custom

            UpdateStiffness(); // Run to allow regeneration when no people is around 

            if(activateRegeneration)
                RecoverConstraintPositions(); // CUSTOM: Recover lost constraints over time
        }

        private void AppyExternalForces(double dt, bool useJobsInside)
        {
            if (!useJobsInside)
            {
                for (int j = 0; j < Bodies.Count; j++)
                {
                    Body3d body = Bodies[j];

                    for (int i = 0; i < body.NumParticles; i++)
                    {
                        body.Velocities[i] -= (body.Velocities[i] * body.Dampning) * dt;
                    }

                    for (int i = 0; i < Forces.Count; i++)
                    {
                        Forces[i].ApplyForce(dt, body);
                    }
                } 
            }
            else
            {
                // TODO
            }
        }

        private void EstimatePositions(double dt, bool useJobsInside)
        {
            if (!useJobsInside)
            {
                for (int j = 0; j < Bodies.Count; j++)
                {
                    Body3d body = Bodies[j];

                    for (int i = 0; i < body.NumParticles; i++)
                    {
                        body.Predicted[i] = body.Positions[i] + dt * body.Velocities[i];
                    }
                } 
            }
            else if (useJobsInside)
            {
                // 1. Version JOBS

                NativeArray<Body3dStruct> bodies = new NativeArray<Body3dStruct>(Bodies.Count, Allocator.TempJob);

                for (int i = 0; i < Bodies.Count; i++)
                {
                    bodies[i].SetPositions(Bodies[i].Positions, Allocator.TempJob);
                    bodies[i].SetVelocities(Bodies[i].Velocities, Allocator.TempJob);
                    bodies[i].SetPredicted(Bodies[i].Predicted, Allocator.TempJob);
                }

                var job = new EstimatePositionsJob
                {
                    dt = dt,
                    Bodies = bodies
                };

                JobHandle handle = job.Schedule(bodies.Length, 1);
                handle.Complete();

                for (int i = 0; i < Bodies.Count; i++)
                {
                    Bodies[i].Positions = bodies[i].Positions.ToArray();
                    Bodies[i].Velocities = bodies[i].Velocities.ToArray();
                    Bodies[i].Predicted = bodies[i].Predicted.ToArray();
                }

                bodies.Dispose();

                // 2. Version JOBS

                /*
                // 4. Create temporary arrays
                NativeArray<Vector3d> predictedPositionArray = new NativeArray<Vector3d>(Bodies[0].NumParticles, Allocator.TempJob);
                NativeArray<Vector3d> currentPositionArray = new NativeArray<Vector3d>(Bodies[0].NumParticles, Allocator.TempJob);
                NativeArray<Vector3d> currentVelocityArray = new NativeArray<Vector3d>(Bodies[0].NumParticles, Allocator.TempJob);

                // 5. Full the copy, native arrays
                for (int i = 0; i < Bodies[0].NumParticles; i++)
                {
                    predictedPositionArray[i] = Bodies[0].Predicted[i];
                    currentPositionArray[i] = Bodies[0].Positions[i];
                    currentVelocityArray[i] = Bodies[0].Velocities[i];
                }

                // 6. Tell job system to complete that particuar job
                JobHandle jobHandle = EstimatePositionsJobHandle(predictedPositionArray, currentPositionArray, currentVelocityArray, dt, Bodies[0]);
                jobHandle.Complete();

                // 7. Copy back the data
                for (int i = 0; i < Bodies[0].NumParticles; i++)
                {
                    Bodies[0].Predicted[i] = predictedPositionArray[i];
                    Bodies[0].Positions[i] = currentPositionArray[i];
                    Bodies[0].Velocities[i] = currentVelocityArray[i];
                }

                // 8. Dispose
                predictedPositionArray.Dispose();
                currentPositionArray.Dispose();
                currentVelocityArray.Dispose();
                */
            }
        }

        /*
        private JobHandle EstimatePositionsJobHandle(NativeArray<Vector3d> predictedPositionArray, NativeArray<Vector3d> currentPositionArray, NativeArray<Vector3d> currentVelocityArray, double dt, Body3d body)
        {
            // 2. Create instance of the struct passing the copied arguments
            EstimatePositionsJob job = new EstimatePositionsJob
            {
                predictedPositionArray = predictedPositionArray,
                currentPositionArray = currentPositionArray,
                currentVelocityArray = currentVelocityArray,
                dt = dt
            };

            // 3. Schedule job in the job system
            return job.Schedule(Bodies[0].NumParticles, 1);
        }
        */

        private void UpdateBounds()
        {
            for (int i = 0; i < Bodies.Count; i++)
            {
                Bodies[i].UpdateBounds();
            }
        }

        private void ResolveCollisions()
        {
            List<CollisionContact3d> contacts = new List<CollisionContact3d>();

            for (int i = 0; i < Collisions.Count; i++)
            {
                Collisions[i].FindContacts(Bodies, contacts);
            }

            double di = 1.0 / CollisionIterations;

            for(int i = 0; i < CollisionIterations; i++)
            {
                for (int j = 0; j < contacts.Count; j++)
                {
                    contacts[j].ResolveContact(di);
                }
            }
        }

        private void ResolveExternalCollisions(bool usePenetrationDistance, float collisionFixedStep)
        {
            List<CollisionContactExternal3d> externalContacts = new List<CollisionContactExternal3d>();

            for (int i = 0; i < ExternalCollisions.Count; i++)
            {
                ExternalCollisions[i].FindExternalContacts(Bodies, ExternalBodies, externalContacts);
            }

            double di = 1.0 / CollisionIterations;

            for (int i = 0; i < CollisionIterations; i++)
            {
                for (int j = 0; j < externalContacts.Count; j++)
                {
                    externalContacts[j].ResolveContactExternal(di, usePenetrationDistance, collisionFixedStep);
                }
            }
        }

        private void ConstrainPositions()
        {
            double di = 1.0 / SolverIterations;

            for (int i = 0; i < SolverIterations; i++)
            {
                for (int j = 0; j < Bodies.Count; j++)
                {
                    Bodies[j].ConstrainPositions(di);
                }
            }
        }

        private void UpdateVelocities(double dt, bool useJobsInside)
        {
            double invDt = 1.0 / dt;
            double threshold2 = SleepThreshold * dt;
            threshold2 *= threshold2;

            if (!useJobsInside)
            {
                for (int j = 0; j < Bodies.Count; j++)
                {
                    Body3d body = Bodies[j];

                    for (int i = 0; i < body.NumParticles; i++)
                    {
                        Vector3d d = body.Predicted[i] - body.Positions[i];
                        body.Velocities[i] = d * invDt;

                        double m = body.Velocities[i].SqrMagnitude;
                        if (m < threshold2)
                            body.Velocities[i] = Vector3d.Zero;
                    }
                } 
            }
            else if (useJobsInside)
            {
                // 4. Create temporary arrays
                NativeArray<Vector3d> predictedPositionArray = new NativeArray<Vector3d>(Bodies[0].NumParticles, Allocator.TempJob);
                NativeArray<Vector3d> currentPositionArray = new NativeArray<Vector3d>(Bodies[0].NumParticles, Allocator.TempJob);
                NativeArray<Vector3d> currentVelocityArray = new NativeArray<Vector3d>(Bodies[0].NumParticles, Allocator.TempJob);

                // 5. Full the copy, native arrays
                for (int i = 0; i < Bodies[0].NumParticles; i++)
                {
                    predictedPositionArray[i] = Bodies[0].Predicted[i];
                    currentPositionArray[i] = Bodies[0].Positions[i];
                    currentVelocityArray[i] = Bodies[0].Velocities[i];
                }

                // 6. Tell job system to complete that particuar job
                JobHandle jobHandle = UpdateVelocitiesJobHandle(predictedPositionArray, currentPositionArray, currentVelocityArray, dt, Bodies[0]);
                jobHandle.Complete();

                // 7. Copy back the data
                for (int i = 0; i < Bodies[0].NumParticles; i++)
                {
                    Bodies[0].Predicted[i] = predictedPositionArray[i];
                    Bodies[0].Positions[i] = currentPositionArray[i];
                    Bodies[0].Velocities[i] = currentVelocityArray[i];
                }

                // 8. Dispose
                predictedPositionArray.Dispose();
                currentPositionArray.Dispose();
                currentVelocityArray.Dispose();
            }
        }

        private JobHandle UpdateVelocitiesJobHandle(NativeArray<Vector3d> predictedPositionArray, NativeArray<Vector3d> currentPositionArray, NativeArray<Vector3d> currentVelocityArray, double dt, Body3d body)
        {
            // 2. Create instance of the struct passing the copied arguments
            UpdateVelocitiesJob job = new UpdateVelocitiesJob
            {
                predictedPositionArray = predictedPositionArray,
                currentPositionArray = currentPositionArray,
                currentVelocityArray = currentVelocityArray,
                dt = dt
            };

            // 3. Schedule job in the job system
            return job.Schedule(body.NumParticles, 1);
        }

        private void ConstrainVelocities()
        {
            for (int i = 0; i < Bodies.Count; i++)
            {
                Bodies[i].ConstrainVelocities();
            }
        }

        private void UpdatePositions(bool useJobsInside)
        {
            if (!useJobsInside)
            {
                for (int j = 0; j < Bodies.Count; j++)
                {
                    Body3d body = Bodies[j];

                    for (int i = 0; i < body.NumParticles; i++)
                    {
                        body.Positions[i] = body.Predicted[i];
                    }
                } 
            }
            else if (useJobsInside)
            {
                // 4. Create temporary arrays
                NativeArray<Vector3d> predictedPositionArray = new NativeArray<Vector3d>(Bodies[0].NumParticles, Allocator.TempJob);
                NativeArray<Vector3d> currentPositionArray = new NativeArray<Vector3d>(Bodies[0].NumParticles, Allocator.TempJob);
                NativeArray<Vector3d> currentVelocityArray = new NativeArray<Vector3d>(Bodies[0].NumParticles, Allocator.TempJob);

                // 5. Full the copy, native arrays
                for (int i = 0; i < Bodies[0].NumParticles; i++)
                {
                    predictedPositionArray[i] = Bodies[0].Predicted[i];
                    currentPositionArray[i] = Bodies[0].Positions[i];
                    currentVelocityArray[i] = Bodies[0].Velocities[i];
                }

                // 6. Tell job system to complete that particuar job
                JobHandle jobHandle = UpdatePositionsJobHandle(predictedPositionArray, currentPositionArray, Bodies[0]);
                jobHandle.Complete();

                // 7. Copy back the data
                for (int i = 0; i < Bodies[0].NumParticles; i++)
                {
                    Bodies[0].Predicted[i] = predictedPositionArray[i];
                    Bodies[0].Positions[i] = currentPositionArray[i];
                    Bodies[0].Velocities[i] = currentVelocityArray[i];
                }

                // 8. Dispose
                predictedPositionArray.Dispose();
                currentPositionArray.Dispose();
                currentVelocityArray.Dispose();
            }
        }

        private JobHandle UpdatePositionsJobHandle(NativeArray<Vector3d> predictedPositionArray, NativeArray<Vector3d> currentPositionArray, Body3d body)
        {
            // 2. Create instance of the struct passing the copied arguments
            UpdatePositionsJob job = new UpdatePositionsJob
            {
                predictedPositionArray = predictedPositionArray,
                currentPositionArray = currentPositionArray
            };

            // 3. Schedule job in the job system
            return job.Schedule(body.NumParticles, 1);
        }

        // Custom
        private void RemoveConstrainPositions()
        {
            for (int i = 0; i < Bodies.Count; i++)
            {
                Bodies[i].RemoveVerticalBendingConstrainPositions();
            }
        }

        // Custom
        private void UpdateStiffness()
        {
            for (int j = 0; j < Bodies.Count; j++)
            {
                Bodies[j].DegradateVerticalPlant();
            }
        }

        // Custom
        private void RecoverConstraintPositions()
        {
            for (int j = 0; j < Bodies.Count; j++)
            {
                Bodies[j].RecoverVerticalBendingConstrainPositions();
            }
        }
    }

    // --- Newer Jobs

    [BurstCompile]
    struct EstimatePositionsJob : IJobParallelFor
    {
        public double dt;
        public NativeArray<Body3dStruct> Bodies;

        public void Execute(int j)
        {
            Body3dStruct body = Bodies[j];

            for (int i = 0; i < body.NumParticles; i++)
            {
                body.Predicted[i] = body.Positions[i] + dt * body.Velocities[i];
            }

            Bodies[j] = body;
        }
    }

    // --- Older Jobs

    //public struct EstimatePositionsJob : IJobParallelFor
    //{
    //    public NativeArray<Vector3d> predictedPositionArray;
    //    public NativeArray<Vector3d> currentPositionArray;
    //    public NativeArray<Vector3d> currentVelocityArray;
    //    [ReadOnly] public double dt;
        
    //    public void Execute(int index)
    //    {
    //        predictedPositionArray[index] = currentPositionArray[index] + (float)dt * currentVelocityArray[index];
    //    }
    //}

    public struct UpdateVelocitiesJob : IJobParallelFor
    {
        public NativeArray<Vector3d> predictedPositionArray;
        public NativeArray<Vector3d> currentPositionArray;
        public NativeArray<Vector3d> currentVelocityArray;
        [ReadOnly] public double dt;

        public double invDt;
        double threshold2;
        
        public void Execute(int index)
        {
            invDt = 1.0 / dt;
            threshold2 = 1f * dt;
            threshold2 *= threshold2;

            Vector3d d = predictedPositionArray[index] - currentPositionArray[index];
            currentVelocityArray[index] = d * invDt;

            double m = currentVelocityArray[index].SqrMagnitude;
            if (m < threshold2)
                currentVelocityArray[index] = Vector3d.Zero;
        }
    }

    public struct UpdatePositionsJob : IJobParallelFor
    {
        public NativeArray<Vector3d> predictedPositionArray;
        public NativeArray<Vector3d> currentPositionArray;

        public void Execute(int index)
        {
            currentPositionArray[index] = predictedPositionArray[index];
        }
    }
}