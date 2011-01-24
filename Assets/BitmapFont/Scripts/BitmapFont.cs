using UnityEngine;
using System.Collections.Generic;

/* Class: BitmapChar
 * 
 * Holds information about a single character in the bitmap font
 */
[System.Serializable]
public class BitmapChar
{
    public int Id;
    public Vector2 Position;
    public Vector2 Size;
    public Vector2 Offset;
    public int Page;
    public float XAdvance;
}

[System.Serializable]
public class BitmapCharKerning
{
    public int FirstChar;
    public int SecondChar;
    public float Amount;
}

/* Class: BitmapFont
 * 
 * Holds the font textures and information.
 */
public class BitmapFont : MonoBehaviour
{
    //Base color for the font
    public Color Color = Color.white;

    /* Variables: AlphaMin, AlphaMax
     * 
     * Upper and lower thresholds in the distance field
     * for rendering the base color. The font is blurred
     * between the min and max values.
     * 
     * Increasing the gap makes the font more blurred, reducing
     * the gap creates a more crisp font.
     */
    public float AlphaMin = 0.49f;
    public float AlphaMax = 0.51f;

    //Shadow/outline color
    public Color ShadowColor = Color.black;

    //Upper and lower thresholds in the distance field for the shadow
    public float ShadowAlphaMin = 0.28f;
    public float ShadowAlphaMax = 0.49f;

    //UV-offset for the shadow/outline, use to create a dropshadow effect
    public Vector2 ShadowOffset = new Vector2(-0.05f, 0.08f);

    //These values are imported from the font file
    public float Size;
    public float LineHeight;
    public float Base;
    public float ScaleW;
    public float ScaleH;
    public BitmapChar[] Chars;
    public Rect[] PageOffsets;
    public Texture2D PageAtlas;
    public BitmapCharKerning[] Kernings;

    private Material pageMaterial;
    private Dictionary<int, Material> fontMaterials = new Dictionary<int, Material>();

    public BitmapChar GetBitmapChar(int c)
    {
        foreach (BitmapChar bitmapChar in Chars)
        {
            if (c == bitmapChar.Id)
            {
                return bitmapChar;
            }
        }
        Debug.LogWarning("Could not find bitmap character for unicode char " + c);
        return Chars[0];
    }

    public Rect GetUVRect(int c)
    {
        BitmapChar bitmapChar = GetBitmapChar(c);
        return GetUVRect(bitmapChar);
    }

    public Rect GetUVRect(BitmapChar bitmapChar)
    {
        //Convert positions/scale from AngleCode-format (pixels, top left origin) to uv format (0-1, bottom left origin)
        Vector2 scaledSize = new Vector2(bitmapChar.Size.x / ScaleW, bitmapChar.Size.y / ScaleH);
        Vector2 scaledPos = new Vector2(bitmapChar.Position.x / ScaleW, bitmapChar.Position.y / ScaleH);
        Vector2 uvCharPos = new Vector2(scaledPos.x, 1 - (scaledPos.y + scaledSize.y));

        //Scale and translate according to page atlas
        Rect offset = PageOffsets[bitmapChar.Page];
        uvCharPos = new Vector2(uvCharPos.x * offset.width + offset.xMin, uvCharPos.y * offset.height + offset.yMin);
        scaledSize = new Vector2(scaledSize.x * offset.width, scaledSize.y * offset.height);

        return new Rect(uvCharPos.x, uvCharPos.y, scaledSize.x, scaledSize.y);
    }


    public Material CreateFontMaterial()
    {
        return new Material(Shader.Find("BitmapFont/Outline"));
    }

    public void UpdateFontMaterial(Material fontMaterial)
    {
        //Forward parameters to shader
        fontMaterial.color = Color;
        fontMaterial.mainTexture = PageAtlas;
        fontMaterial.SetFloat("_AlphaMin", AlphaMin);
        fontMaterial.SetFloat("_AlphaMax", AlphaMax);
        fontMaterial.SetColor("_ShadowColor", ShadowColor);
        fontMaterial.SetFloat("_ShadowAlphaMin", ShadowAlphaMin);
        fontMaterial.SetFloat("_ShadowAlphaMax", ShadowAlphaMax);
        fontMaterial.SetFloat("_ShadowOffsetU", ShadowOffset.x);
        fontMaterial.SetFloat("_ShadowOffsetV", ShadowOffset.y);
    }

    /* Method: GetPageMaterial
     * 
     * Returns a material with the right texture 
     * for the given page.
     */
    public Material GetPageMaterial(int page)
    {
        if (pageMaterial == null)
        {
            pageMaterial = CreateFontMaterial();
        }

        UpdateFontMaterial(pageMaterial);
        return pageMaterial;
    }

    /* Method: GetCharacterMaterial
     * 
     * Returns a material with the right texture, offset and scale
     * to render the given character within the (0,1) x (0,1) uv space
     */
    public Material GetCharacterMaterial(int c)
    {
        //If material doesn't exist for this character, create it
        if (!fontMaterials.ContainsKey(c) || fontMaterials[c] == null)
        {
            Material fontMaterial = CreateFontMaterial();
            BitmapChar bitmapChar = GetBitmapChar(c);

            Rect uvRect = GetUVRect(bitmapChar);
            fontMaterial.mainTextureScale = new Vector2(uvRect.width, uvRect.height); // xy
            fontMaterial.mainTextureOffset = new Vector2(uvRect.xMin, uvRect.yMin); // zw

            //Cache material for this character
            fontMaterials[c] = fontMaterial;
        }

        //Update material to get the current parameters
        Material mat = fontMaterials[c];
        UpdateFontMaterial(mat);
        return mat;
    }

    public float GetKerning(char first, char second)
    {
        if (Kernings != null)
        {
            foreach (BitmapCharKerning krn in Kernings)
            {
                if (krn.FirstChar == (int)first && krn.SecondChar == (int)second)
                {
                    return krn.Amount;
                }
            }
        }
        return 0;
    }

    public Vector2 CalculateSize(string str, Vector2 renderSize)
    {
        Vector2 curPos = new Vector2(0, renderSize.y);
        Vector2 scale = renderSize / Size;

        for (int idx = 0; idx < str.Length; idx++)
        {
            char c = str[idx];
            BitmapChar charInfo = GetBitmapChar((int)c);

            float krn = 0;
            if (idx < str.Length - 1)
            {
                krn = GetKerning(c, str[idx + 1]);
            }
            curPos.x += (charInfo.XAdvance + krn) * scale.x;
        }

        return curPos;
    }

    public Vector2 CalculateSize(string str, float renderSize)
    {
        return CalculateSize(str, new Vector2(renderSize, renderSize));
    }

    public Vector2 Render(Vector2 position, string str, Vector2 renderSize)
    {
        Vector2 curPos = position;
        Vector2 scale = renderSize / Size;

        for (int idx=0; idx<str.Length; idx++)
        {
            char c = str[idx];
            Material mat = GetCharacterMaterial((int)c);
            BitmapChar charInfo = GetBitmapChar((int)c);

            Vector2 scaledSize = Vector2.Scale(charInfo.Size, scale);
            Vector2 scaledOffset = Vector2.Scale(charInfo.Offset, scale);

            Graphics.DrawTexture(new Rect((int)(curPos.x + scaledOffset.x), (int)(curPos.y + scaledOffset.y), (int)scaledSize.x, (int)scaledSize.y), mat.mainTexture, mat);
            //Graphics.DrawTexture(new Rect((int)(curPos.x), (int)(curPos.y), (int)scaledSize.x, (int)renderSize.y), mat.mainTexture, mat);

            float krn = 0;
            if (idx < str.Length - 1)
            {
                krn = GetKerning(c, str[idx + 1]);
            }
            curPos.x += (charInfo.XAdvance + krn) * scale.x;
        }

        return curPos;
    }

    public Vector2 Render(Vector2 position, string str, float renderSize)
    {
        return Render(position, str, new Vector2(renderSize, renderSize));
    }
}

