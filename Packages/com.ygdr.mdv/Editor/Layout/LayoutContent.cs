using System.Text.RegularExpressions;
using UnityEngine;

namespace YGDR.MDV
{
    public abstract class Content
    {
        public const float KbdPadding = 3f;

        public Rect       Location;
        public Style      Style;
        public GUIContent Payload;
        public string     Link;

        public float Width  => Location.width;
        public float Height => Location.height;

        public Content( GUIContent payload, Style style, string link )
        {
            Payload = payload;
            Style   = style;
            Link    = link;
        }

        public void CalcSize( Context context )
        {
            Location.size = context.CalcSize( Payload );
        }

        public virtual void Draw( Context context )
        {
            var guiStyle = context.Apply( Style );

            if( Style.Kbd )
            {
                var bg     = UnityEditor.EditorGUIUtility.isProSkin ? new Color( 0.22f, 0.22f, 0.25f ) : new Color( 0.78f, 0.78f, 0.82f );
                var border = UnityEditor.EditorGUIUtility.isProSkin ? new Color( 0.45f, 0.45f, 0.50f ) : new Color( 0.50f, 0.50f, 0.55f );
                UnityEditor.EditorGUI.DrawRect( Location, bg );
                UnityEditor.EditorGUI.DrawRect( new Rect( Location.x,            Location.y,            Location.width, 1f ), border );
                UnityEditor.EditorGUI.DrawRect( new Rect( Location.x,            Location.yMax - 1f,    Location.width, 1f ), border );
                UnityEditor.EditorGUI.DrawRect( new Rect( Location.x,            Location.y,            1f, Location.height ), border );
                UnityEditor.EditorGUI.DrawRect( new Rect( Location.xMax - 1f,    Location.y,            1f, Location.height ), border );
                var textRect = new Rect( Location.x + KbdPadding, Location.y, Location.width - KbdPadding * 2f, Location.height );
                GUI.Label( textRect, Payload, guiStyle );
                return;
            }

            if( string.IsNullOrEmpty( Link ) )
            {
                GUI.Label( Location, Payload, guiStyle );
            }
            else
            {
#if UNITY_EDITOR
                UnityEditor.EditorGUIUtility.AddCursorRect( Location, UnityEditor.MouseCursor.Link );
#endif
                if( GUI.Button( Location, Payload, guiStyle ) )
                {
                    if( Regex.IsMatch( Link, @"^\w+:", RegexOptions.Singleline ) )
                        Application.OpenURL( Link );
                    else
                        context.SelectPage( Link );
                }
            }

            if( Style.Strikethrough )
            {
                var strikeRect = new Rect( Location.x, Location.y + Location.height * 0.5f, Location.width, 1f );
#if UNITY_EDITOR
                UnityEditor.EditorGUI.DrawRect( strikeRect, guiStyle.normal.textColor );
#else
                GUI.DrawTexture( strikeRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, guiStyle.normal.textColor, 0, 0 );
#endif
            }
        }

        public virtual void Update( Context context ) { }
    }

    public class ContentImage : Content
    {
        public string URL;
        public string Alt;
        public string Tooltip;
        public float  OverrideWidth;
        public float  OverrideHeight;

        public ContentImage( GUIContent payload, Style style, string link )
            : base( payload, style, link ) { }

        public override void Draw( Context context )
        {
            if( Payload.image != null )
            {
                GUI.DrawTexture( Location, Payload.image, ScaleMode.StretchToFill );

                if( !string.IsNullOrEmpty( Link ) )
                {
#if UNITY_EDITOR
                    UnityEditor.EditorGUIUtility.AddCursorRect( Location, UnityEditor.MouseCursor.Link );
#endif
                    if( GUI.Button( Location, GUIContent.none, GUIStyle.none ) )
                    {
                        if( Regex.IsMatch( Link, @"^\w+:", RegexOptions.Singleline ) )
                            Application.OpenURL( Link );
                        else
                            context.SelectPage( Link );
                    }
                }

                if( !string.IsNullOrEmpty( Tooltip ) )
                    GUI.Label( Location, new GUIContent( string.Empty, Tooltip ), GUIStyle.none );
            }
            else
            {
                base.Draw( context );
            }
        }

        public override void Update( Context context )
        {
            Payload.image = context.FetchImage( URL );
            Payload.text  = null;

            if( Payload.image == null )
            {
                context.Apply( Style );
                var text     = !string.IsNullOrEmpty( Alt ) ? Alt : URL;
                Payload.text = string.Format( "[{0}]", text );
            }

            if( Payload.image != null && ( OverrideWidth > 0f || OverrideHeight > 0f ) )
            {
                float naturalWidth  = Payload.image.width;
                float naturalHeight = Payload.image.height;

                if( OverrideWidth > 0f && OverrideHeight > 0f )
                {
                    Location.width  = OverrideWidth;
                    Location.height = OverrideHeight;
                }
                else if( OverrideWidth > 0f && naturalHeight > 0f )
                {
                    Location.width  = OverrideWidth;
                    Location.height = OverrideWidth * naturalHeight / naturalWidth;
                }
                else if( OverrideHeight > 0f && naturalWidth > 0f )
                {
                    Location.width  = OverrideHeight * naturalWidth / naturalHeight;
                    Location.height = OverrideHeight;
                }
            }
            else
            {
                Location.size = context.CalcSize( Payload );
            }
        }

    }

    public class ContentText : Content
    {
        public ContentText( GUIContent payload, Style style, string link )
            : base( payload, style, link ) { }
    }
}
