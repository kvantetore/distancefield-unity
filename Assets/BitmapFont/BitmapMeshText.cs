using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class BitmapMeshText : MonoBehaviour
{
    public BitmapFont Font;
    public string Text;
    public Mesh GeneratedMesh;

    private string renderedText;

    // Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Font == null)
        {
            return;
        }

        Vector3 renderSize = new Vector3(1, 1, 1);

        if (renderedText != Text)
        {
            Vector3[] quadVerts = new Vector3[4];
            quadVerts[0] = new Vector3(0, 0);
            quadVerts[1] = new Vector3(0, 1);
            quadVerts[2] = new Vector3(1, 1);
            quadVerts[3] = new Vector3(1, 0);

            Vector2[] quadUvs = new Vector2[4];
            quadUvs[0] = new Vector2(0, 0);
            quadUvs[1] = new Vector2(0, 1);
            quadUvs[2] = new Vector2(1, 1);
            quadUvs[3] = new Vector2(1, 0);

            int[] quadTriangles = { 
                    0, 1, 2,
                    2, 3, 0
            };

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
            Vector3 curPos = new Vector3(0,0,0);
            Vector3 scale = renderSize / Font.Size;

            for (int idx = 0; idx < Text.Length; idx++)
            {
                char c = Text[idx];
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
            GeneratedMesh = new Mesh();
            GeneratedMesh.vertices = vertices.ToArray();
            GeneratedMesh.uv = uvs.ToArray();
            GeneratedMesh.subMeshCount = submeshCount;
            for (int i = 0; i < submeshCount; i++)
            {
                GeneratedMesh.SetTriangles(Triangles[i].ToArray(), i);
            }
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            meshFilter.mesh = GeneratedMesh;

            MeshRenderer rndr = GetComponent<MeshRenderer>();

            Material[] mats = new Material[submeshCount];
            for (int i = 0; i < submeshCount; i++)
            {
                Material mat = Font.GetPageMaterial(i);
                Debug.Log(mat.shader.name);
                mats[i] = mat;
            }
            renderer.materials = mats;

            renderedText = Text;
        }
    }
}
