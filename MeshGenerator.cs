using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator 
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        MeshData meshData = new MeshData(width, height);
        int vertexIndex = 0;
         //Rounds the height map values that are within the different ranges
         //This allows for a flatter terrain with step height levels to be generated
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (heightMap[x,y] < 0.23f)
                {
                    heightMap[x, y] += 0.15f;
                }
                else if (heightMap[x,y] > 0.23f && heightMap[x,y] < 0.3f)
                {
                    heightMap[x, y] = 0.4f;
                }
                 else if(heightMap[x, y] > 0.3f && heightMap[x, y] < 0.6f)
                {
                    heightMap[x, y] = 0.43f;
                }
                else if (heightMap[x, y] > 0.6f && heightMap[x, y] < 0.7f)
                {
                    heightMap[x, y] = 0.47f;
                }
                else if (heightMap[x, y] > 0.7f)
                {
                    heightMap[x, y] -= 0.1f;
                }

                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x,y]) * heightMultiplier, topLeftZ - y);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                if (x < width - 1 && y < height - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + width + 1, vertexIndex + width);
                    meshData.AddTriangle(vertexIndex + width + 1, vertexIndex, vertexIndex + 1);
                }

                //GenerateObjects(heightCurve.Evaluate(heightMap[x,y]) * heightMultiplier, heightMap[x,y], width, height);
                vertexIndex++;
            }
        }

        return meshData;
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public Vector2[] uvs;
    public int[] triangles;

    int triangleIndex;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh()
        {
            vertices = vertices,
            triangles = triangles,
            uv = uvs
        };
        mesh.RecalculateNormals();
        return mesh;
    }
}

