using UnityEngine;

namespace YGDR.MDV
{
    public class Context
    {
        StyleConverter          mStyleConverter;
        GUIStyle                mStyleGUI;
        MarkdownHandlerImages   mImages;
        MarkdownHandlerNavigate mNavigate;

        public Context( GUISkin skin, MarkdownHandlerImages images, MarkdownHandlerNavigate navigate )
        {
            mStyleConverter = new StyleConverter( skin );
            mImages         = images;
            mNavigate       = navigate;

            Apply( Style.Default );
        }

        public void    SelectPage( string path ) => mNavigate.SelectPage( path );
        public Texture FetchImage( string url )  => mImages.FetchImage( url );

        public float LineHeight  => mStyleGUI.lineHeight;
        public float MinWidth    => LineHeight * 2.0f;
        public float IndentSize  => LineHeight * 1.5f;

        public void     Reset()                        => Apply( Style.Default );
        public GUIStyle Apply( Style style )           { mStyleGUI = mStyleConverter.Apply( style ); return mStyleGUI; }
        public Vector2  CalcSize( GUIContent content ) => mStyleGUI.CalcSize( content );
    }

    public class Layout
    {
        Context        mContext;
        BlockContainer mDocument;

        public Layout( Context context, BlockContainer doc )
        {
            mContext  = context;
            mDocument = doc;
        }

        public float Height => mDocument.Rect.height;

        public Block Find( string id ) => mDocument.Find( id );

        public void Arrange( float maxWidth )
        {
            mContext.Reset();
            mDocument.Arrange( mContext, MarkdownViewer.Margin, maxWidth );
        }

        public void Draw()
        {
            mContext.Reset();
            mDocument.Draw( mContext );
        }
    }
}
