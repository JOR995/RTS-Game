using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseGen
{
    //Generates the 2d array of perlin noise float values using the passed parameters to aid randomise it
    public static float[,] GenerateNoise(int mapWidth, int mapHeight, float scale, int octaves, float persistance, int seed, Vector2 offest)
    {
        float[,] noise = new float[mapWidth, mapHeight];

        System.Random rand = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = rand.Next(-100000, 100000) + offest.x;
            float offestY = rand.Next(-100000, 100000) + offest.y;
            octaveOffsets[i] = new Vector2(offsetX, offestY);
        }

        if (scale <= 0) scale = 0.0001f;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {

                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistance;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noise[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noise[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noise[x, y]);
            }
        }
        return noise;
    }
}
