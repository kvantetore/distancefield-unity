using UnityEngine;
using UnityEditor;

/* Class: DistanceField
 * 
 * Creates a distance field texture from an outline texture.
 * 
 * It is currently using a brute force algorithm taken from Joakim Hårsmanns C
 * implementation https://bitbucket.org/harsman/distfield. It is quite slow for large images
 * and requires a rather high  * resolution input image to give satisfactory results. 
 */
class DistanceField
{
    /* Constant: SearchRadius
     * 
     * The radius to search for an outline. Increasing this leads to a
     * more blurred distance field which takes a longer time to compute
     */
    static int SearchRadius = 20;  

    public enum TextureChannel
    {
        RED = 0,
        GREEN = 1,
        BLUE = 2,
        ALPHA = 3
    }

    public static Texture2D CreateDistanceFieldTexture(Texture2D inputTexture, TextureChannel channel, int outSize)
    {
        //Extract channel from input texture
        byte[] inputBuffer = GetTextureChannel(inputTexture, channel);

        //Create distance field
        byte[] outputBuffer = render(inputBuffer, inputTexture.width, inputTexture.height, outSize);

        //Put distance field into output texture
        Texture2D outputTexture = new Texture2D(outSize, outSize, TextureFormat.ARGB32, false);
        SetTextureChannel(outputTexture, TextureChannel.ALPHA, outputBuffer);

        return outputTexture;
    }

    private static byte[] GetTextureChannel(Texture2D tex, TextureChannel channel)
    {
        Color[] pixels = tex.GetPixels();
        byte[] channelData = new byte[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            channelData[i] = (byte)(255*pixels[i][(int)channel]);
        }

        return channelData;
    }

    private static void SetTextureChannel(Texture2D tex, TextureChannel channel, byte[] channelData)
    {
        if (tex.height * tex.width != channelData.Length)
        {
            throw new System.Exception("Invalid length of channel data");
        }

        //Convert channel to array of pixels
        Color[] pixels = new Color[channelData.Length];
        for (int i = 0; i < pixels.Length; i++)
        {
            Color pix = new Color();
            pix[(int)channel] = channelData[i] / 255f;
            pixels[i] = pix;
        }

        //Update texture
        tex.SetPixels(pixels);
        tex.Apply();
    }


    /*
     * mindist() and render() shamelessly stolen from Joakim Hårsmanns distfield calculator
     * https://bitbucket.org/harsman/distfield
     */

    static float mindist(byte[] buffer, int w, int h, int x, int y, int r, float maxdist)
    {
        int i, j, startx, starty, stopx, stopy;
        bool hit;
        float d, dx, dy;
        float mind = maxdist;
        byte p;

        p = buffer[y * w + x];
        bool outside = (p == 0);

        startx = Mathf.Max(0, x - r);
        starty = Mathf.Max(0, y - r);
        stopx = Mathf.Min(w, x + r);
        stopy = Mathf.Min(h, y + r);

        for (i = starty; i < stopy; i++)
        {
            for (j = startx; j < stopx; j++)
            {
                p = buffer[i * w + j];
                dx = j - x;
                dy = i - y;
                d = dx * dx + dy * dy;
                hit = (p != 0) == outside;
                if (hit && (d < mind))
                {
                    mind = d;
                }
                if (d > maxdist)
                    Debug.LogWarning("Too big\n");
            }
        }

        if (outside)
            return Mathf.Sqrt(mind);
        else
            return -Mathf.Sqrt(mind);
    }

    static byte[] render(byte[] input, int w, int h, int outsize)
    {
        byte[] output = new byte[outsize * outsize];
        int x, y, ix, iy;
        float d;
        byte di;
        int sx = w / outsize;
        int sy = h / outsize;
        /* No sense of searching further with only 8-bits of output
         * precision
         */
        int r = SearchRadius;
        float maxsq = 2 * r * r;
        float max = Mathf.Sqrt(maxsq);

        for (y = 0; y < outsize; y++)
        {
            if (EditorUtility.DisplayCancelableProgressBar("Creating Distance Field", "", y / (float)(outsize-1)))
            {
                EditorUtility.ClearProgressBar();
                throw new System.Exception("Canceled");
            }
            for (x = 0; x < outsize; x++)
            {
                ix = (x * sx) + (sx / 2);
                iy = (y * sy) + (sy / 2);
                d = mindist(input, w, h, ix, iy, r, maxsq);
                di = (byte)(127.5f + 127.5f * (-d / max));
                {
                    //Debug.Log("Distance is " + d);
                }
                output[y * outsize + x] = di;
            }
        }
        EditorUtility.ClearProgressBar();

        return output;
    }

}