using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
    Copyright (c) 2017 Sloan Kelly

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

/// <summary>
/// Make buildings.
/// </summary>
class BuildingMaker : InfrastructureBehaviour
{
    public Material building;
    public static List<int> roofIndices;
    float baseHeight = 15;
    float roofHeight = 0;
    protected static List<int> roofTriangles;
    public static List<Vector3> roofVertices;
    protected static List<Vector3> vertices;
    protected static List<Vector2> uvs;
    protected bool hasErrors = false;

    /// <summary>
    /// Create the buildings.
    /// </summary>
    /// <returns></returns>
    IEnumerator Start()
    {
        // Wait until the map is ready
        while (!map.IsReady)
        {
            yield return null;
        }

        // Iterate through all the buildings in the 'ways' list
        foreach (var way in map.ways.FindAll((w) => { return w.IsBuilding && w.NodeIDs.Count > 1; }))
        {
            // Create the object
            CreateObject(way, building, "Building");
            yield return null;
        }
    }
    
    protected enum RoofType
    {
        /// <summary>
        /// Dome roof.
        /// </summary>
        dome,

        /// <summary>
        /// Flat roof.
        /// </summary>
        flat
    }

    /// <summary>
    /// Build the object using the data from the OsmWay instance.
    /// </summary>
    /// <param name="way">OsmWay instance</param>
    /// <param name="origin">The origin of the structure</param>
    /// <param name="vectors">The vectors (vertices) list</param>
    /// <param name="normals">The normals list</param>
    /// <param name="uvs">The UVs list</param>
    /// <param name="indices">The indices list</param>
    protected override void OnObjectCreated(OsmWay way, Vector3 origin, List<Vector3> vectors, List<Vector3> normals, List<Vector2> uvs, List<int> indices)
    {
        /*// Get the centre of the roof
        Vector3 oTop = new Vector3(0, way.Height, 0);

        // First vector is the middle point in the roof
        vectors.Add(oTop);
        normals.Add(Vector3.up);
        uvs.Add(new Vector2(0.5f, 0.5f));*/
        
        RoofType roofType = RoofType.flat;

        

        for (int i = 1; i < way.NodeIDs.Count; i++)
        {
            OsmNode p1 = map.nodes[way.NodeIDs[i - 1]];
            OsmNode p2 = map.nodes[way.NodeIDs[i]];

            Vector3 v1 = p1 - origin;
            Vector3 v2 = p2 - origin;
            Vector3 v3 = v1 + new Vector3(0, way.Height, 0);
            Vector3 v4 = v2 + new Vector3(0, way.Height, 0);

            vectors.Add(v1);
            vectors.Add(v2);
            vectors.Add(v3);
            vectors.Add(v4);

            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));

            normals.Add(-Vector3.forward);
            normals.Add(-Vector3.forward);
            normals.Add(-Vector3.forward);
            normals.Add(-Vector3.forward);

            int idx1, idx2, idx3, idx4;
            idx4 = vectors.Count - 1;
            idx3 = vectors.Count - 2;
            idx2 = vectors.Count - 3;
            idx1 = vectors.Count - 4;

            // first triangle v1, v3, v2
            indices.Add(idx1);
            indices.Add(idx3);
            indices.Add(idx2);

            // second         v3, v4, v2
            indices.Add(idx3);
            indices.Add(idx4);
            indices.Add(idx2);

            // // third          v2, v3, v1
            // indices.Add(idx2);
            // indices.Add(idx3);
            // indices.Add(idx1);
            //
            // // fourth         v2, v4, v3
            // indices.Add(idx2);
            // indices.Add(idx4);
            // indices.Add(idx3);

            /*// And now the roof triangles
            indices.Add(0);
            indices.Add(idx3);
            indices.Add(idx4);
            
            // Don't forget the upside down one!
            indices.Add(idx4);
            indices.Add(idx3);
            indices.Add(0);*/
            CreateHouseRoof(vectors, baseHeight, roofHeight, roofType, uvs);
        }
    }
    
    private void CreateHouseRoof(List<Vector3> baseVertices, float baseHeight, float roofHeight, RoofType roofType, List<Vector2> uvs)
    {
        float[] roofPoints = new float[baseVertices.Count * 2];

        if (roofVertices == null) roofVertices = new List<Vector3>(baseVertices.Count);
        else roofVertices.Clear();

        try
        {
            int countVertices = CreateHouseRoofVerticles(baseVertices, roofVertices, roofPoints, baseHeight);
            CreateHouseRoofTriangles(countVertices, roofVertices, roofType, roofPoints, baseHeight, roofHeight, ref roofTriangles);
            Debug.Log("kekw");

            if (roofTriangles.Count == 0)
            {
                hasErrors = true;
                return;
            }

            Vector3 side1 = roofVertices[roofTriangles[1]] - roofVertices[roofTriangles[0]]; 
            Debug.Log("side1" + side1);
            Vector3 side2 = roofVertices[roofTriangles[2]] - roofVertices[roofTriangles[0]];
            Debug.Log("side2" + side2);
            Vector3 perp = Vector3.Cross(side1, side2);
            Debug.Log("perp" + perp);

            bool reversed = perp.y < 0;
            if (reversed) roofTriangles.Reverse();

            float minX = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxZ = float.MinValue;

            for (int i = 0; i < roofVertices.Count; i++)
            {
                Vector3 v = roofVertices[i];
                if (v.x < minX) minX = v.x;
                if (v.z < minZ) minZ = v.z;
                if (v.x > maxX) maxX = v.x;
                if (v.z > maxZ) maxZ = v.z;
            }

            float offX = maxX - minX;
            float offZ = maxZ - minZ;

            for (int i = 0; i < roofVertices.Count; i++)
            {
                Vector3 v = roofVertices[i];
                uvs.Add(new Vector2((v.x - minX) / offX, (v.z - minZ) / offZ));
            }

            int triangleOffset = vertices.Count;
            for (int i = 0; i < roofTriangles.Count; i++) roofTriangles[i] += triangleOffset;

            vertices.AddRange(roofVertices);
        }
        catch (Exception)
        {
            Debug.Log(roofTriangles.Count + "   " + roofVertices.Count);
            hasErrors = true;
            throw;
        }
    }

    private static void CreateHouseRoofDome(float height, List<Vector3> vertices, List<int> triangles)
    {
        Vector3 roofTopPoint = Vector3.zero;
        roofTopPoint = vertices.Aggregate(roofTopPoint, (current, point) => current + point) / vertices.Count;
        roofTopPoint.y = height;
        int vIndex = vertices.Count;

        for (int i = 0; i < vertices.Count; i++)
        {
            int p1 = i;
            int p2 = i + 1;
            if (p2 >= vertices.Count) p2 -= vertices.Count;

            triangles.AddRange(new[] { p1, p2, vIndex });
        }

        vertices.Add(roofTopPoint);
    }

    private static void CreateHouseRoofTriangles(int countVertices, List<Vector3> vertices, RoofType roofType, float[] roofPoints, float baseHeight, float roofHeight, ref List<int> triangles)
    {
        if (roofType == RoofType.flat)
        {
            if (roofIndices == null) roofIndices = new List<int>(60);
            Debug.Log("lll");
            triangles.AddRange(OsmUtils.Triangulate(roofPoints, countVertices, roofIndices));
            
        }
        else if (roofType == RoofType.dome) CreateHouseRoofDome(baseHeight + roofHeight, vertices, triangles);
    }

    private static int CreateHouseRoofVerticles(List<Vector3> baseVertices, List<Vector3> verticles, float[] roofPoints, float baseHeight)
    {
        float topPoint = baseHeight;
        int countVertices = 0;

        for (int i = 0; i < baseVertices.Count; i++)
        {
            Vector3 p = baseVertices[i];
            float px = p.x;
            float pz = p.z;

            bool hasVerticle = false;

            /*for (int j = 0; j < countVertices * 2; j += 2)
            {
                if (Math.Abs(roofPoints[j] - px) < float.Epsilon && Math.Abs(roofPoints[j + 1] - pz) < float.Epsilon)
                {
                    hasVerticle = true;
                    break;
                }
            }*/

            if (!hasVerticle)
            {
                int cv2 = countVertices * 2;

                roofPoints[cv2] = px;
                roofPoints[cv2 + 1] = pz;
                verticles.Add(new Vector3(px, topPoint, pz));

                countVertices++;
            }
        }

        return countVertices;
    }
    
}

