/****************************************************
 * File: ClothBody3d.cs
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

using Common.Geometry.Shapes;
using Common.Mathematics.LinearAlgebra;

using PositionBasedDynamics.Constraints;
using PositionBasedDynamics.Sources;

using System.Linq;

namespace PositionBasedDynamics.Bodies.Cloth
{
    public class ClothBody3d : Body3d
    {
        #region Instance Properties - Cloth Properties

        public double StretchStiffness { get; private set;  }
        public double BendStiffness { get; set; }

        public float BreakingThresholdAngle { get; private set; }
        public float DegradationThresholdAngle { get; private set; }
        private float RecuperationTimeStiffness { get; set; }
        private float StepStiffness { get; set; }
        private float GrowingTime { get; set; }

        public int[] Indices { get; private set; }

        #endregion

        public ClothBody3d(TrianglesFromGrid source, double radius, double mass, double stretchStiffness, double bendStiffness, float breakingThreshold, float degradationThreshold, float recuperationTimeStiffness, float stepStiffness, Matrix4x4d RTS, float growingTime)
            : base(source.NumParticles, radius, mass)
        {
			StretchStiffness = stretchStiffness;
            BendStiffness = bendStiffness;
            BreakingThresholdAngle = breakingThreshold;
            DegradationThresholdAngle = degradationThreshold;
            RecuperationTimeStiffness = recuperationTimeStiffness;
            StepStiffness = stepStiffness;
            GrowingTime = growingTime;

            CreateParticles(source, RTS);
			CreateConstraints(source.Rows, source.Columns);
        }
        
        private void CreateParticles(TrianglesFromGrid source, Matrix4x4d RTS)
        {
            for (int i = 0; i < NumParticles; i++)
            {
                Vector4d pos = RTS * source.Positions[i].xyz1;
                Positions[i] = new Vector3d(pos.x, pos.y, pos.z);
                PositionsOriginal[i] = new Vector3d(pos.x, pos.y, pos.z);
                Predicted[i] = Positions[i];

                //Debug.Log("[INFO] Created Positions[" + i + "]: " + Positions[i]);
            }

            int numIndices = source.NumIndices;
            Indices = new int[numIndices];

            for (int i = 0; i < numIndices; i++)
                Indices[i] = source.Indices[i];
        }

        private void CreateConstraints(int nRows, int nCols)
        {
            int height = nCols + 1;
            int width = nRows + 1;

            //Debug.Log("[INFO] Creating constraints...nCols: " + nCols + ", nRows: " + nRows + ", height: " + height + ", width: " + width);

            CreateDistanceConstraints(height, width);
            CreateBendingConstraints(nRows, nCols);
        }

        private void CreateBendingConstraints(int nRows, int nCols)
        {
            // Bending Vertical
            for (int i = 0; i <= nRows; i++)
            {
                for (int j = 0; j < nCols - 1; j++)
                {
                    int i0 = j * (nRows + 1) + i;
                    int i1 = (j + 1) * (nRows + 1) + i;
                    int i2 = (j + 2) * (nRows + 1) + i;
                    
                    //Debug.Log("[INFO] Adding Vertical Bending Constraint between particle " + i0 + ", " + i1 + " and " + i2);

                    Constraint3d newConstraint = new BendingConstraint3d(this, i0, i1, i2, BendStiffness, BreakingThresholdAngle, DegradationThresholdAngle, RecuperationTimeStiffness, StepStiffness, GrowingTime);
                    Constraints.Add(newConstraint);
                    
                    BendingConstraintsVertical.Add(newConstraint);   
                    TotalVerticalBendingConstraints++;
                }
            }

            // Bending Horizontal
            for (int i = 0; i < nRows - 1; i++)
            {
                for (int j = 0; j <= nCols; j++)
                {
                    int i0 = j * (nRows + 1) + i;
                    int i1 = j * (nRows + 1) + (i + 1);
                    int i2 = j * (nRows + 1) + (i + 2);

                    //Debug.Log("[INFO] Adding Horizontal Bending Constraint between particle " + i0 + ", " + i1 + " and " + i2);

                    Constraint3d newConstraint = new BendingConstraint3d(this, i0, i1, i2, BendStiffness, BreakingThresholdAngle, DegradationThresholdAngle, RecuperationTimeStiffness, StepStiffness, GrowingTime);
                    Constraints.Add(newConstraint);

                    BendingConstraintsHorizontal.Add(newConstraint);
                }
            }
        }

        private void CreateDistanceConstraints(int height, int width)
        {
            // Distance Horizontal
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < (width - 1); x++)
                {
                    //Debug.Log("[INFO] Adding Horizontal Distance Contraint between particle " + (y * width + x) + " and " + (y * width + x + 1));
                    Constraints.Add(new DistanceConstraint3d(this, y * width + x, y * width + x + 1, StretchStiffness));
                }
            }

            // Distance Vertical
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < (height - 1); y++)
                {
                    //Debug.Log("[INFO] Adding Vertical Distance Constraint between particle " + (y * width + x) + " and " + ((y + 1) * width + x));
                    Constraints.Add(new DistanceConstraint3d(this, y * width + x, (y + 1) * width + x, StretchStiffness));
                }
            }

            // Shearing distance constraint
            for (int y = 0; y < (height - 1); y++)
            {
                for (int x = 0; x < (width - 1); x++)
                {
                    //Debug.Log("[INFO] Adding Shearing Distance Constraint between particle " + (y * width + x) + " and " + ((y + 1) * width + x + 1));
                    //Debug.Log("[INFO] Adding Shearing Distance Constraint between particle " + ((y + 1) * width + x) + " and " + (y * width + x + 1));

                    Constraints.Add(new DistanceConstraint3d(this, y * width + x, (y + 1) * width + x + 1, StretchStiffness));
                    Constraints.Add(new DistanceConstraint3d(this, (y + 1) * width + x, y * width + x + 1, StretchStiffness));
                }
            }
        }
    }
}