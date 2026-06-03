using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DeformablePlatform))]
public class DeformablePlatformEditor : Editor
{
    private int selectedVertexIndex = -1;

    public override void OnInspectorGUI()
    {
        // Draw the default inspector fields (grid size, spacing)
        DrawDefaultInspector();

        DeformablePlatform platform = (DeformablePlatform)target;

        // Selected Vertex Details
        if (selectedVertexIndex >= 0 && platform.vertexOffsets != null && selectedVertexIndex < platform.vertexOffsets.Length)
        {
            GUILayout.Space(15);
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"Selected Vertex: #{selectedVertexIndex}", EditorStyles.boldLabel);
            
            int xVertices = platform.gridWidth + 1;
            int z = selectedVertexIndex / xVertices;
            int x = selectedVertexIndex % xVertices;
            
            Vector3 offset = platform.vertexOffsets[selectedVertexIndex];
            Vector3 defaultPos = new Vector3(x * platform.spacing, 0f, z * platform.spacing);
            Vector3 currentPos = defaultPos + offset;

            GUILayout.Label($"Grid Position: (Col: {x}, Row: {z})");
            GUILayout.Label($"Local Position: {currentPos.ToString("F2")}");
            GUILayout.Label($"Offset from Grid: {offset.ToString("F2")}");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset Vertex to Default"))
            {
                Undo.RecordObject(platform, "Reset Vertex Position");
                platform.vertexOffsets[selectedVertexIndex] = Vector3.zero;
                platform.RebuildMesh();
                EditorUtility.SetDirty(platform);
            }
            if (GUILayout.Button("Deselect"))
            {
                selectedVertexIndex = -1;
                Repaint();
            }
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
        }

        GUILayout.Space(15);
        GUILayout.Label("Global Controls", EditorStyles.boldLabel);

        if (GUILayout.Button("Reset All Vertices to Flat", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Reset Platform?", "Are you sure you want to reset all vertex offsets back to 0?", "Yes", "Cancel"))
            {
                Undo.RecordObject(platform, "Reset Platform");
                platform.ResetPlatform();
                selectedVertexIndex = -1;
            }
        }
        
        GUILayout.Space(5);
        EditorGUILayout.HelpBox("INSTRUCTIONS:\n1. Click a vertex dot in the Scene View to select it.\n2. Use the 3D arrows to move the vertex in any direction (X, Y, Z).\n3. Pull the X/Z arrows to deform the shape/width of the platform, and the Y arrow to make slopes.", MessageType.Info);
    }

    private void OnSceneGUI()
    {
        DeformablePlatform platform = (DeformablePlatform)target;

        if (platform.vertexOffsets == null)
            return;

        int xVertices = platform.gridWidth + 1;
        int zVertices = platform.gridLength + 1;
        
        // Safety check to ensure array size matches grid size
        if (platform.vertexOffsets.Length != xVertices * zVertices)
        {
            platform.InitializeOffsets();
        }

        bool changed = false;

        // 1. Draw a light grid outline in the Scene View to visualize the platform's shape
        Handles.color = new Color(0.2f, 0.8f, 1f, 0.4f);
        for (int z = 0; z < zVertices; z++)
        {
            for (int x = 0; x < xVertices; x++)
            {
                int index = z * xVertices + x;
                if (index >= platform.vertexOffsets.Length) continue;

                Vector3 localPos = new Vector3(x * platform.spacing, 0f, z * platform.spacing) + platform.vertexOffsets[index];
                Vector3 worldPos = platform.transform.TransformPoint(localPos);

                // Draw wire link to adjacent vertex on X axis
                if (x < platform.gridWidth)
                {
                    int nextX = index + 1;
                    if (nextX < platform.vertexOffsets.Length)
                    {
                        Vector3 localNextX = new Vector3((x + 1) * platform.spacing, 0f, z * platform.spacing) + platform.vertexOffsets[nextX];
                        Handles.DrawLine(worldPos, platform.transform.TransformPoint(localNextX));
                    }
                }

                // Draw wire link to adjacent vertex on Z axis
                if (z < platform.gridLength)
                {
                    int nextZ = index + xVertices;
                    if (nextZ < platform.vertexOffsets.Length)
                    {
                        Vector3 localNextZ = new Vector3(x * platform.spacing, 0f, (z + 1) * platform.spacing) + platform.vertexOffsets[nextZ];
                        Handles.DrawLine(worldPos, platform.transform.TransformPoint(localNextZ));
                    }
                }
            }
        }

        // 2. Draw selection dots at all vertices
        for (int z = 0; z < zVertices; z++)
        {
            for (int x = 0; x < xVertices; x++)
            {
                int index = z * xVertices + x;
                if (index >= platform.vertexOffsets.Length) continue;

                Vector3 localPos = new Vector3(x * platform.spacing, 0f, z * platform.spacing) + platform.vertexOffsets[index];
                Vector3 worldPos = platform.transform.TransformPoint(localPos);

                float handleSize = HandleUtility.GetHandleSize(worldPos) * 0.08f;
                
                if (index == selectedVertexIndex)
                {
                    Handles.color = Color.yellow;
                }
                else
                {
                    Handles.color = Color.cyan;
                }

                // Draw a button handle. Clicking it selects this vertex.
                if (Handles.Button(worldPos, Quaternion.identity, handleSize, handleSize, Handles.DotHandleCap))
                {
                    selectedVertexIndex = index;
                    Repaint(); // Forces inspector updates
                }
            }
        }

        // 3. Draw standard 3D Position Handle (arrows) for the SELECTED vertex
        if (selectedVertexIndex >= 0 && selectedVertexIndex < platform.vertexOffsets.Length)
        {
            int z = selectedVertexIndex / xVertices;
            int x = selectedVertexIndex % xVertices;

            Vector3 defaultLocalPos = new Vector3(x * platform.spacing, 0f, z * platform.spacing);
            Vector3 currentLocalPos = defaultLocalPos + platform.vertexOffsets[selectedVertexIndex];
            Vector3 worldPos = platform.transform.TransformPoint(currentLocalPos);

            EditorGUI.BeginChangeCheck();
            
            // Draw standard 3D position handle aligned with the platform transform rotation
            Vector3 newWorldPos = Handles.PositionHandle(worldPos, platform.transform.rotation);
            
            if (EditorGUI.EndChangeCheck())
            {
                // Record undo command
                Undo.RecordObject(platform, "Move Platform Vertex");

                // Calculate the new local offset from the default grid position
                Vector3 localNewPos = platform.transform.InverseTransformPoint(newWorldPos);
                platform.vertexOffsets[selectedVertexIndex] = localNewPos - defaultLocalPos;
                
                changed = true;
            }
        }

        // Rebuild mesh and collider if any handles were moved
        if (changed)
        {
            platform.RebuildMesh();
            EditorUtility.SetDirty(platform);
        }
    }
}
