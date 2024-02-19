using System;
using System.Collections.Generic;
using UnityEngine;

using Common.Mathematics.LinearAlgebra;
using Common.Geometry.Shapes;

namespace PositionBasedDynamics.Sources
{

    public class TetrahedronsFromBounds : TetrahedronSource
    {

        public Box3d Bounds { get; private set; }

        public TetrahedronsFromBounds(double radius, Box3d bounds)
            : base(radius)
        {
            Bounds = bounds;
            CreateParticles();
            CreateEdges();
        }

        private void CreateParticles()
        {
            int numX = (int)(Bounds.Width / Diameter);
            int numY = (int)(Bounds.Height / Diameter);
            int numZ = (int)(Bounds.Depth / Diameter);
            
            //Debug.Log("[INFO] numX: " + numX);
            //Debug.Log("[INFO] numY: " + numY);
            //Debug.Log("[INFO] numZ: " + numZ); // If numZ is 1, does not create edges

            if (numX != 1 && numY != 1 && numZ != 1)
            {
                //Debug.Log("Making Tetrahedron");
                
                Positions = new Vector3d[numX * numY * numZ];

                for (int z = 0; z < numZ; z++)
                {
                    for (int y = 0; y < numY; y++)
                    {
                        for (int x = 0; x < numX; x++)
                        {
                            Vector3d pos = new Vector3d();
                            pos.x = Diameter * x + Bounds.Min.x + Spacing;
                            pos.y = Diameter * y + Bounds.Min.y + Spacing;
                            pos.z = Diameter * z + Bounds.Min.z + Spacing;

                            //Debug.Log("[INFO] x + y * numX + z * numX * numY " + (x + y * numX + z * numX * numY));

                            Positions[x + y * numX + z * numX * numY] = pos;
                        }
                    }
                }

                Indices = new List<int>(); // Here is the error - does not enter

                for (int z = 0; z < numZ - 1; z++) // Before - 1
                {
                    for (int y = 0; y < numY - 1; y++)
                    {
                        for (int x = 0; x < numX - 1; x++)
                        {
                            int p0 = x + y * numX + z * numY * numX;
                            int p1 = (x + 1) + y * numX + z * numY * numX;

                            int p3 = x + y * numX + (z + 1) * numY * numX;
                            int p2 = (x + 1) + y * numX + (z + 1) * numY * numX;

                            int p7 = x + (y + 1) * numX + (z + 1) * numY * numX;
                            int p6 = (x + 1) + (y + 1) * numX + (z + 1) * numY * numX;

                            int p4 = x + (y + 1) * numX + z * numY * numX;
                            int p5 = (x + 1) + (y + 1) * numX + z * numY * numX;

                            // Ensure that neighboring tetras are sharing faces
                            if ((x + y + z) % 2 == 1)
                            {
                                Indices.Add(p2); Indices.Add(p1); Indices.Add(p6); Indices.Add(p3);
                                Indices.Add(p6); Indices.Add(p3); Indices.Add(p4); Indices.Add(p7);
                                Indices.Add(p4); Indices.Add(p1); Indices.Add(p6); Indices.Add(p5);
                                Indices.Add(p3); Indices.Add(p1); Indices.Add(p4); Indices.Add(p0);
                                Indices.Add(p6); Indices.Add(p1); Indices.Add(p4); Indices.Add(p3);
                            }
                            else
                            {
                                Indices.Add(p0); Indices.Add(p2); Indices.Add(p5); Indices.Add(p1);
                                Indices.Add(p7); Indices.Add(p2); Indices.Add(p0); Indices.Add(p3);
                                Indices.Add(p5); Indices.Add(p2); Indices.Add(p7); Indices.Add(p6);
                                Indices.Add(p7); Indices.Add(p0); Indices.Add(p5); Indices.Add(p4);
                                Indices.Add(p0); Indices.Add(p2); Indices.Add(p7); Indices.Add(p5);
                            }
                        }
                    }
                } 
            }
            else
            {
                //Debug.Log("ERROR - NOT making Tetrahedron - Making 2D Plane with triangle meshes");
                
                int rows = (int)(1f / Diameter); // width
                int columns = (int)(1f / Diameter); // height
                
                //Debug.Log("[INFO] rows: " + rows);
                //Debug.Log("[INFO] columns: " + columns);
                
                Positions = new Vector3d[(rows + 1) * (columns + 1)];
                Indices = new int[rows * columns * 2 * 3];

                double dx = 1f / rows; // width
                double dy = 1f / columns; // height

                int index = 0;
                for (int j = 0; j <= columns; j++)
                {
                    for (int i = 0; i <= rows; i++)
                    {
                        double x = dx * i;
                        double z = dy * j;

                        Vector3d pos = new Vector3d(x - 1f / 2, 0, z - 1f / 2);

                        Positions[index] = pos;
                        index++;
                    }
                }

                index = 0;
                for (int i = 0; i < columns; i++)
                {
                    for (int j = 0; j < rows; j++)
                    {
                        int i0 = i * (rows + 1) + j;
                        int i1 = i0 + 1;
                        int i2 = i0 + (rows + 1);
                        int i3 = i2 + 1;

                        if ((j + i) % 2 != 0)
                        {
                            Indices[index++] = i0;
                            Indices[index++] = i2;
                            Indices[index++] = i1;
                            Indices[index++] = i1;
                            Indices[index++] = i2;
                            Indices[index++] = i3;
                        }
                        else
                        {
                            Indices[index++] = i0;
                            Indices[index++] = i2;
                            Indices[index++] = i3;
                            Indices[index++] = i0;
                            Indices[index++] = i3;
                            Indices[index++] = i1;
                        }
                    }
                }
            }
        }
    }
}