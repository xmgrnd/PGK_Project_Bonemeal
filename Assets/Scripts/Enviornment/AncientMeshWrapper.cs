using UnityEngine;
using UnityEngine.InputSystem;

// This script deforms a mesh to give it an ancient, weathered stone appearance.
// It offsets each vertex by a random amount or based on Perlin noise.
[RequireComponent(typeof(MeshFilter))]
public class AncientMeshWarper : MonoBehaviour
{
    [Header("Warp Settings")]
    [Range(0f, 0.5f)] public float warpStrength = 0.05f;
    public float noiseFrequency = 2.0f;
    public bool warpOnStart = true;

    private Mesh _originalMesh;
    private Mesh _deformedMesh;

    void Start()
    {
        // We create a unique instance of the mesh to avoid modifying the original asset
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        _originalMesh = meshFilter.sharedMesh;
        _deformedMesh = Instantiate(_originalMesh);
        meshFilter.mesh = _deformedMesh;

        if (warpOnStart)
        {
            ApplyAncientWarp();
        }
    }

    void Update()
    {
        // New Input System: Press 'G' to re-generate the weathered look [cite: 2025-12-25]
        if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
        {
            ApplyAncientWarp();
        }
    }

    public void ApplyAncientWarp()
    {
        Vector3[] vertices = _originalMesh.vertices;
        Vector3[] normals = _originalMesh.normals;
        Vector3[] newVertices = new Vector3[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            // Calculate a pseudo-random offset based on the vertex position (Perlin noise)
            float noiseX = Mathf.PerlinNoise(vertices[i].y * noiseFrequency, vertices[i].z * noiseFrequency);
            float noiseY = Mathf.PerlinNoise(vertices[i].x * noiseFrequency, vertices[i].z * noiseFrequency);
            float noiseZ = Mathf.PerlinNoise(vertices[i].x * noiseFrequency, vertices[i].y * noiseFrequency);

            Vector3 offset = new Vector3(noiseX - 0.5f, noiseY - 0.5f, noiseZ - 0.5f) * warpStrength;
            
            // Apply the offset to the vertex position
            newVertices[i] = vertices[i] + offset;
        }

        // Update the mesh data
        _deformedMesh.vertices = newVertices;
        _deformedMesh.RecalculateNormals(); // Crucial for stone lighting and shadows
        _deformedMesh.RecalculateBounds();

        Debug.Log("<color=grey>Ancient Warper:</color> Mesh 'entrance' has been weathered.");
    }
}