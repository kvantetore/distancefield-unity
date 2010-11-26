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
    public Texture2D[] Pages;

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

    public Material GetMaterial(int c)
    {
        //Recreate materials every time. This is probably horribly slow,
        //but it allows updating the values in realtime
        if (fontMaterials.ContainsKey(c))
        {
            Object.Destroy(fontMaterials[c]);
            fontMaterials.Remove(c);
        }

        if (!fontMaterials.ContainsKey(c))
        {
            BitmapChar bitmapChar = GetBitmapChar(c);
            Material fontMaterial = new Material(Shader.Find("BitmapFont/Outline"));

            //Convert positions/scale from AngleCode-format (pixels, top left origin) to uv format (0-1, bottom left origin)
            Vector2 scaledSize = new Vector2(bitmapChar.Size.x / ScaleW, bitmapChar.Size.y / ScaleH);
            Vector2 scaledPos = new Vector2(bitmapChar.Position.x / ScaleW, bitmapChar.Position.y / ScaleH);
            Vector2 uvCharPos = new Vector2(scaledPos.x, 1-(scaledPos.y + scaledSize.y));

            fontMaterial.mainTexture = Pages[bitmapChar.Page];
            fontMaterial.mainTextureScale = scaledSize; // xy
            fontMaterial.mainTextureOffset = uvCharPos; // zw

            //Forward parameters to shader
            fontMaterial.color = Color;
            fontMaterial.SetFloat("_AlphaMin", AlphaMin);
            fontMaterial.SetFloat("_AlphaMax", AlphaMax);
            fontMaterial.SetColor("_ShadowColor", ShadowColor);
            fontMaterial.SetFloat("_ShadowAlphaMin", ShadowAlphaMin);
            fontMaterial.SetFloat("_ShadowAlphaMax", ShadowAlphaMax);
            fontMaterial.SetFloat("_ShadowOffsetU", ShadowOffset.x);
            fontMaterial.SetFloat("_ShadowOffsetV", ShadowOffset.y);
            
            //Cache material for this character
            fontMaterials[c] = fontMaterial;
        }

        return fontMaterials[c];
    }


    public Vector2 Render(Vector2 position, string str, float renderSize)
    {
        Vector2 curPos = position;
        float scale = renderSize / Size;

        foreach (char c in str)
        {
            Material mat = GetMaterial((int)c);
            BitmapChar charInfo = GetBitmapChar((int)c);

            Vector2 scaledSize = charInfo.Size * scale;
            Vector2 scaledOffset = charInfo.Offset * scale;

            Graphics.DrawTexture(new Rect((int)(curPos.x + scaledOffset.x), (int)(curPos.y + scaledOffset.y), (int)scaledSize.x, (int)scaledSize.y), mat.mainTexture, mat);

            curPos.x += charInfo.XAdvance * scale;
        }

        return curPos;
    }

}

