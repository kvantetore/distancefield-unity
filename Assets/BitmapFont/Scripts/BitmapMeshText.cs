using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode()]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class BitmapMeshText : MonoBehaviour
{
    public BitmapFont Font;
    public string Text;
    public TextAnchor Anchor;

    private string renderedText;

    #region Quad Parameters

    private Vector3[] quadVerts = 
    {
        new Vector3(0, 0),
        new Vector3(0, 1),
        new Vector3(1, 1),
        new Vector3(1, 0)
    };

    private Vector2[] quadUvs = 
    {
        new Vector2(0, 0),
        new Vector2(0, 1),
        new Vector2(1, 1),
        new Vector2(1, 0)
    };

    private int[] quadTriangles = 
    { 
        0, 1, 2,
        2, 3, 0
    };

    #endregion

    // Update is called once per frame
    void Update()
    {
        if (Font == null)
        {
            return;
        }

        Vector3 renderSize = new Vector3(1, 1, 1);
        Vector2 renderSize2 = new Vector2(renderSize.x, renderSize.y);
        Vector3 position = new Vector3(0, 0, 0);

        if (renderedText != Text)
        {
            //Calculate bounding box of rendered text
            Vector2 bounds = Font.CalculateSize(Text, renderSize2);

            Vector3 offset = new Vector3(0, 0);
            if (Anchor == TextAnchor.UpperCenter || Anchor == TextAnchor.UpperLeft || Anchor == TextAnchor.UpperRight)
            {
                offset.y = bounds.y;
            }
            if (Anchor == TextAnchor.MiddleCenter || Anchor == TextAnchor.MiddleLeft || Anchor == TextAnchor.MiddleRight)
            {
                offset.y = bounds.y / 2;
            }
            if (Anchor == TextAnchor.UpperRight || Anchor == TextAnchor.MiddleRight || Anchor == TextAnchor.LowerRight)
            {
                offset.x = bounds.x;
            }
            if (Anchor == TextAnchor.UpperCenter || Anchor == TextAnchor.MiddleCenter || Anchor == TextAnchor.LowerCenter)
            {
                offset.x = bounds.x / 2;
            }


            //Replace mesh
            Mesh mesh = GenerateLineMesh(position - offset, Text, renderSize);
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter.sharedMesh != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(meshFilter.sharedMesh);
                }
                else
                {
                    DestroyImmediate(meshFilter.sharedMesh);
                }
            }
            meshFilter.mesh = mesh;
            
            //Get materials for each texture page
            Material[] mats = new Material[mesh.subMeshCount];
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                mats[i] = Font.GetPageMaterial(i);
            }
            renderer.materials = mats;

            renderedText = Text;
        }
    }

    private Mesh GenerateLineMesh(Vector3 position, string str, Vector3 renderSize)
    {
        //Set up mesh structures
        int submeshCount = Font.Pages.Length;
        List<int>[] Triangles = new List<int>[submeshCount];
        for (int i = 0; i < submeshCount; i++)
        {
            Triangles[i] = new List<int>();
        }
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        //Keep track of position
        Vector3 curPos = position;
        Vector3 scale = renderSize / Font.Size;

        for (int idx = 0; idx < str.Length; idx++)
        {
            char c = str[idx];
            BitmapChar charInfo = Font.GetBitmapChar((int)c);
            int vertIndex = vertices.Count;

            //Set up uvs
            Rect uvRect = Font.GetUVRect(charInfo);
            Vector2 uvScale = new Vector2(uvRect.width, uvRect.height);
            Vector2 uvOffset = new Vector2(uvRect.x, uvRect.y);
            for (int i = 0; i < quadUvs.Length; i++)
            {
                uvs.Add(Vector2.Scale(quadUvs[i], uvScale) + uvOffset);
            }

            //Set up verts
            Vector3 vertSize = Vector2.Scale(charInfo.Size, scale);
            Vector3 vertOffset = Vector2.Scale(charInfo.Offset, scale);
            vertOffset.y = renderSize.y - (vertOffset.y + vertSize.y);  // change offset from top to bottom
            for (int i = 0; i < quadVerts.Length; i++)
            {
                Vector3 vert = Vector3.Scale(quadVerts[i], vertSize) + curPos + vertOffset;
                vertices.Add(vert);
            }

            //Set up triangles
            for (int i = 0; i < quadTriangles.Length; i++)
            {
                Triangles[charInfo.Page].Add(quadTriangles[i] + vertIndex);
            }

            //Advance cursor
            float krn = 0;
            if (idx < Text.Length - 1)
            {
                krn = Font.GetKerning(c, Text[idx + 1]);
            }
            curPos.x += (charInfo.XAdvance + krn) * scale.x;
        }

        //Assign verts, uvs, tris and materials to mesh
        Mesh mesh= new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.subMeshCount = submeshCount;
        for (int i = 0; i < submeshCount; i++)
        {
            mesh.SetTriangles(Triangles[i].ToArray(), i);
        }

        return mesh;
    }
}
