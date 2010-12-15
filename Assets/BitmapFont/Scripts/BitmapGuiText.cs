using UnityEngine;
using System.Collections;

public enum TextOrigin
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    Center
}

public enum TextUnits
{
    Pixels,
    ScreenNormalized,
}

[ExecuteInEditMode()]
public class BitmapGuiText : MonoBehaviour
{
    public BitmapFont Font;
    public string Text;
    public TextAnchor Anchor;
    public TextOrigin Origin;
    public TextUnits PositionUnits;
    public TextUnits ScaleUnits;
    public bool KeepAspectRatio = true;

    public void OnGUI()
    {
        if (Event.current.type == EventType.Repaint)
        {
            if (Font != null && Text != null)
            {
                //Convert scale to pixels
                Vector2 scale = new Vector2(transform.lossyScale.x, transform.lossyScale.y);
                if (ScaleUnits == TextUnits.ScreenNormalized)
                {
                    scale.x *= Screen.width;
                    scale.y *= Screen.height;
                }
                if (KeepAspectRatio)
                {
                    scale.y = scale.x;
                }

                //Calculate bounding box of rendered text
                Vector2 size = Font.CalculateSize(Text, scale);

                //Convert position to pixels
                Vector2 pos = new Vector2(transform.position.x, transform.position.y);
                if (PositionUnits == TextUnits.ScreenNormalized)
                {
                    pos.x *= Screen.width;
                    pos.y *= Screen.height;
                }

                //Default origin is top left
                if (Origin == TextOrigin.BottomLeft || Origin == TextOrigin.BottomRight)
                {
                    pos.y = Screen.height - pos.y;
                }
                if (Origin == TextOrigin.TopRight || Origin == TextOrigin.BottomRight)
                {
                    pos.x = Screen.width - pos.x;
                }
                if (Origin == TextOrigin.Center)
                {
                    pos.x = pos.x + Screen.width / 2;
                    pos.y = pos.y + Screen.height / 2;
                }


                Vector2 offset = new Vector2(0,0);
                if (Anchor == TextAnchor.LowerCenter || Anchor == TextAnchor.LowerLeft || Anchor == TextAnchor.LowerRight)
                {
                    offset.y = size.y;
                }
                if (Anchor == TextAnchor.MiddleCenter || Anchor == TextAnchor.MiddleLeft || Anchor == TextAnchor.MiddleRight)
                {
                    offset.y = size.y / 2;
                }
                if (Anchor == TextAnchor.LowerRight || Anchor == TextAnchor.MiddleRight || Anchor == TextAnchor.UpperRight)
                {
                    offset.x = size.x;
                }
                if (Anchor == TextAnchor.LowerCenter || Anchor == TextAnchor.MiddleCenter || Anchor == TextAnchor.UpperCenter)
                {
                    offset.x = size.x / 2;
                }

                Font.Render(pos - offset, Text, scale);
            }
        }
    }
}
