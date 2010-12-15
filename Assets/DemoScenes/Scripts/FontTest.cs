using UnityEngine;
using System.Collections;


public class FontTest : MonoBehaviour
{
    //Assign font objects in the scene to each of these fonts
    public BitmapFont font1;
    public BitmapFont font2;
    public BitmapFont font3;
    public string str;

    public void OnGUI()
    {
        str = GUILayout.TextField(str);

        if (Event.current.type == EventType.Repaint)
        {
            //shadowed font
            font1.Render(new Vector2(50, 20), str, 32);
            font1.Render(new Vector2(50, 50), str, 80);
            font1.Render(new Vector2(50, 100), str, 256);

            //outlined font
            font2.Render(new Vector2(50, 300), str, Screen.width / 2.5f);

            //without distance field
            font3.Render(new Vector2(50, 300 + Screen.width / 3.5f), str, Screen.width / 2.5f);

            //Incremental rendering of characters from different fonts
            Vector2 pos = new Vector2(50, 650);
            pos = font2.Render(pos, "a", 256);
            pos = font1.Render(pos, "a", 256);
        }
    }
}
