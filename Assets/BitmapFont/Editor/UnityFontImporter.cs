using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

/* Class: UnityFontImporter
 * 
 * Reads a TTF font (that's already imported by Unity) and generates an 
 * Signed Distance Field (SDF) from the unity-generated font texture, and
 * replaces the shader with an SDF shader (BitmapFont/Outline)
 * 
 * Due to the way Unity packs the characters in the texture, the result isn't
 * all that great, as it leaves very little space for the SDF to spread, which 
 * leads to bleeding between characters and abrupt cutoffs around certain glyphs
 */
public class UnityFontImporter
{
    //TODO: Expose parameters to user at import time

    /* Constant: DistanceFieldScaleFactor
     * 
     * The scale factor between the generated distance field and the original 
     * outline texture. DistanceField.size = Outline.size / DistanceFieldScaleFactor
     */
    static int DistanceFieldScaleFactor = 8;

    /* Constant: InputTextureChannel
     * 
     * Which channel from the original outline texture to use when generating the distance
     * field
     */
    static DistanceField.TextureChannel InputTextureChannel = DistanceField.TextureChannel.ALPHA;

    [MenuItem("Assets/BitmapFont/Convert Unity TTF to SDF", validate = true)]
    static bool Validate()
    {
        return Selection.GetFiltered(typeof(Font), SelectionMode.DeepAssets).Length > 0;
    }


    [MenuItem("Assets/BitmapFont/Convert Unity TTF to SDF")]
    static void Import()
    {
        foreach (Object o in Selection.GetFiltered(typeof(Font), SelectionMode.DeepAssets))
        {
            string path = AssetDatabase.GetAssetPath(o.GetInstanceID());
            string basePath = path.Substring(0, path.LastIndexOf("."));

            Font fnt = (Font)o;
            Texture2D tex = (Texture2D)fnt.material.mainTexture;

            //Create distance field from texture
            Texture2D distanceField = DistanceField.CreateDistanceFieldTexture(tex, InputTextureChannel, tex.width / DistanceFieldScaleFactor);
            //Save distance field as png
            byte[] pngData = distanceField.EncodeToPNG();
            string outputPath = basePath + "_dist.png";
            System.IO.File.WriteAllBytes(outputPath, pngData);
            AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceSynchronousImport);

            //Set correct texture format
            TextureImporter texImp = (TextureImporter)TextureImporter.GetAtPath(outputPath);
            texImp.textureType = TextureImporterType.Advanced;
            texImp.isReadable = true;
            texImp.textureFormat = TextureImporterFormat.Alpha8;
            AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceSynchronousImport);

            Material mat = new Material(Shader.Find("BitmapFont/Outline"));
            mat.mainTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(outputPath, typeof(Texture2D));
            AssetDatabase.CreateAsset(mat, basePath + "_dist.mat");
        }
    }

}
