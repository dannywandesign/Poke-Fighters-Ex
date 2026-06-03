using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
[ExecuteInEditMode]
public class DeformablePlatform : MonoBehaviour
{
    [Header("Grid Configuration")]
    [Tooltip("Number of segments along the X axis.")]
    public int gridWidth = 10;
    
    [Tooltip("Number of segments along the Z axis.")]
    public int gridLength = 10;
    
    [Tooltip("Default distance between grid points.")]
    public float spacing = 4f;

    // Stores the 3D position offsets of each vertex from its default grid coordinate.
    // Serialized so changes are saved in the scene.
    [HideInInspector]
    public Vector3[] vertexOffsets;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private Mesh mesh;

    private int prevWidth;
    private int prevLength;

    void OnEnable()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        InitializeOffsets();
        RebuildMesh();
    }

    void OnValidate()
    {
        // Keep grid dimensions positive
        if (gridWidth < 1) gridWidth = 1;
        if (gridLength < 1) gridLength = 1;
        if (spacing < 0.1f) spacing = 0.1f;

        InitializeOffsets();
        RebuildMesh();
    }

    public void InitializeOffsets()
    {
        int vertexCount = (gridWidth + 1) * (gridLength + 1);
        if (vertexOffsets == null || vertexOffsets.Length != vertexCount || prevWidth != gridWidth || prevLength != gridLength)
        {
            Vector3[] oldOffsets = vertexOffsets;
            vertexOffsets = new Vector3[vertexCount];
            
            // If the grid size changed, try to preserve previous offsets where possible
            if (oldOffsets != null)
            {
                int minWidth = Mathf.Min(prevWidth, gridWidth);
                int minLength = Mathf.Min(prevLength, gridLength);
                for (int z = 0; z <= minLength; z++)
                {
                    for (int x = 0; x <= minWidth; x++)
                    {
                        int oldIdx = z * (prevWidth + 1) + x;
                        int newIdx = z * (gridWidth + 1) + x;
                        if (oldIdx < oldOffsets.Length && newIdx < vertexOffsets.Length)
                        {
                            vertexOffsets[newIdx] = oldOffsets[oldIdx];
                        }
                    }
                }
            }

            prevWidth = gridWidth;
            prevLength = gridLength;
        }
    }

    [ContextMenu("Reset Platform")]
    public void ResetPlatform()
    {
        int vertexCount = (gridWidth + 1) * (gridLength + 1);
        vertexOffsets = new Vector3[vertexCount];
        RebuildMesh();
    }

    public void RebuildMesh()
    {
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "DeformablePlatformMesh";
        }

        int xVertices = gridWidth + 1;
        int zVertices = gridLength + 1;
        Vector3[] vertices = new Vector3[xVertices * zVertices];
        Vector2[] uv = new Vector2[vertices.Length];

        for (int z = 0; z < zVertices; z++)
        {
            for (int x = 0; x < xVertices; x++)
            {
                int index = z * xVertices + x;
                Vector3 defaultPos = new Vector3(x * spacing, 0f, z * spacing);
                Vector3 offset = (vertexOffsets != null && index < vertexOffsets.Length) ? vertexOffsets[index] : Vector3.zero;
                
                // Actual position = default grid position + custom 3D offset
                vertices[index] = defaultPos + offset;
                
                // Map UV coordinates (0 to 1) for textures based on default spacing
                uv[index] = new Vector2((float)x / gridWidth, (float)z / gridLength);
            }
        }

        // Generate triangles (two per grid square)
        int[] triangles = new int[gridWidth * gridLength * 6];
        int t = 0;
        for (int z = 0; z < gridLength; z++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                int current = z * xVertices + x;
                int next = current + 1;
                int rowNext = current + xVertices;
                int rowNextPlusOne = rowNext + 1;

                // Triangle 1
                triangles[t + 0] = current;
                triangles[t + 1] = rowNext;
                triangles[t + 2] = next;

                // Triangle 2
                triangles[t + 3] = next;
                triangles[t + 4] = rowNext;
                triangles[t + 5] = rowNextPlusOne;

                t += 6;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        if (meshFilter != null)
        {
            meshFilter.sharedMesh = mesh;
        }

        // Rebuild collider in edit mode so player can walk on it immediately
        if (meshCollider != null)
        {
            meshCollider.sharedMesh = null; // Force update
            meshCollider.sharedMesh = mesh;
        }
    }
}
