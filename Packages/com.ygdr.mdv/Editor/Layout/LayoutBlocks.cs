using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YGDR.MDV
{
    public enum AlertType { None, Note, Tip, Important, Warning, Caution }

    public abstract class Block
    {
        public string ID     = null;
        public Rect   Rect   = new Rect();
        public Block  Parent = null;
        public float  Indent = 0.0f;

        public abstract void Arrange( Context context, Vector2 anchor, float maxWidth );
        public abstract void Draw( Context context );

        public Block( float indent ) { Indent = indent; }

        public virtual Block Find( string id )
            => id.Equals( ID, StringComparison.Ordinal ) ? this : null;
    }

    public class BlockContainer : Block
    {
        public bool      Quoted        = false;
        public bool      Highlight     = false;
        public bool      Horizontal    = false;
        public bool      IsTableRow    = false;
        public bool      IsTableHeader = false;
        public AlertType AlertType     = AlertType.None;

        static Color GetAlertAccentColor( AlertType type )
        {
            switch( type )
            {
                case AlertType.Note:      return new Color( 0.12f, 0.41f, 0.92f );
                case AlertType.Tip:       return new Color( 0.14f, 0.52f, 0.21f );
                case AlertType.Important: return new Color( 0.54f, 0.34f, 0.90f );
                case AlertType.Warning:   return new Color( 0.62f, 0.41f, 0.01f );
                case AlertType.Caution:   return new Color( 0.81f, 0.13f, 0.18f );
                default:                  return Color.gray;
            }
        }

        List<Block> mBlocks = new List<Block>();

        public BlockContainer( float indent ) : base( indent ) { }

        public Block Add( Block block )
        {
            block.Parent = this;
            mBlocks.Add( block );
            return block;
        }

        public override Block Find( string id )
        {
            if( id.Equals( ID, StringComparison.Ordinal ) ) return this;

            foreach( var block in mBlocks )
            {
                var match = block.Find( id );
                if( match != null ) return match;
            }

            return null;
        }

        public override void Arrange( Context context, Vector2 pos, float maxWidth )
        {
            Rect.position = new Vector2( pos.x + Indent, pos.y );
            Rect.width    = maxWidth - Indent - context.IndentSize;

            var paddingBottom   = 0.0f;
            var paddingVertical = 0.0f;

            if( Highlight || IsTableHeader || IsTableRow )
            {
                GUIStyle style;

                if( Highlight )
                    style = GUI.skin.GetStyle( Quoted ? "blockquote" : "blockcode" );
                else
                    style = GUI.skin.GetStyle( IsTableHeader ? "th" : "tr" );

                pos.x         += style.padding.left;
                pos.y         += style.padding.top;
                maxWidth      -= style.padding.horizontal;
                paddingBottom  = style.padding.bottom;
                paddingVertical = style.padding.vertical;
            }

            if( Horizontal )
            {
                Rect.height = 0;
                maxWidth    = mBlocks.Count == 0 ? maxWidth : maxWidth / mBlocks.Count;

                foreach( var block in mBlocks )
                {
                    block.Arrange( context, pos, maxWidth );
                    pos.x      += block.Rect.width;
                    Rect.height = Mathf.Max( Rect.height, block.Rect.height );
                }

                Rect.height += paddingVertical;
            }
            else
            {
                foreach( var block in mBlocks )
                {
                    block.Arrange( context, pos, maxWidth );
                    pos.y += block.Rect.height;
                }

                Rect.height = pos.y - Rect.position.y + paddingBottom;
            }
        }

        public override void Draw( Context context )
        {
            if( Highlight && !Quoted )
            {
                GUI.Box( Rect, string.Empty, GUI.skin.GetStyle( "blockcode" ) );
            }
            else if( Highlight && Quoted && AlertType != AlertType.None )
            {
                var accent = GetAlertAccentColor( AlertType );
                EditorGUI.DrawRect( Rect, new Color( accent.r, accent.g, accent.b, 0.08f ) );
            }
            else if( IsTableHeader )
            {
                GUI.Box( Rect, string.Empty, GUI.skin.GetStyle( "th" ) );
            }
            else if( IsTableRow )
            {
                var parentBlock = Parent as BlockContainer;
                var style = parentBlock != null && parentBlock.mBlocks.IndexOf( this ) % 2 != 0 ? "trl" : "tr";
                GUI.Box( Rect, string.Empty, GUI.skin.GetStyle( style ) );
            }

            mBlocks.ForEach( block => block.Draw( context ) );

            if( Highlight && Quoted )
            {
                if( AlertType != AlertType.None )
                {
                    var accent = GetAlertAccentColor( AlertType );
                    EditorGUI.DrawRect( new Rect( Rect.x, Rect.y, 8f, Rect.height ), accent );
                }
                else
                {
                    GUI.Box( Rect, string.Empty, GUI.skin.GetStyle( "blockquote" ) );
                }
            }
        }

        public void RemoveTrailingSpace()
        {
            if( mBlocks.Count > 0 && mBlocks[ mBlocks.Count - 1 ] is BlockSpace )
                mBlocks.RemoveAt( mBlocks.Count - 1 );
        }
    }

    public class BlockContent : Block
    {
        Content       mPrefix  = null;
        List<Content> mContent = new List<Content>();

        public bool       IsEmpty       => mContent.Count == 0;
        public TextAnchor CellAlignment = TextAnchor.UpperLeft;

        public BlockContent( float indent ) : base( indent ) { }

        public void Add( Content content )    { mContent.Add( content ); }
        public void Prefix( Content content ) { mPrefix = content; }

        public override void Arrange( Context context, Vector2 pos, float maxWidth )
        {
            var origin = pos;

            pos.x    += Indent;
            maxWidth  = Mathf.Max( maxWidth - Indent, context.MinWidth );

            Rect.position = pos;

            if( mPrefix != null )
            {
                mPrefix.Location.x = pos.x - context.IndentSize * 0.5f;
                mPrefix.Location.y = pos.y;
            }

            if( mContent.Count == 0 )
            {
                Rect.width  = 0.0f;
                Rect.height = 0.0f;
                return;
            }

            mContent.ForEach( c => c.Update( context ) );

            var rowWidth   = mContent[ 0 ].Width;
            var rowHeight  = mContent[ 0 ].Height;
            var startIndex = 0;

            for( var i = 1; i < mContent.Count; i++ )
            {
                var content = mContent[ i ];

                if( rowWidth + content.Width > maxWidth )
                {
                    LayoutRow( pos, startIndex, i, rowHeight, maxWidth );
                    pos.y += rowHeight;

                    startIndex = i;
                    rowWidth   = content.Width;
                    rowHeight  = content.Height;
                }
                else
                {
                    rowWidth  += content.Width;
                    rowHeight  = Mathf.Max( rowHeight, content.Height );
                }
            }

            if( startIndex < mContent.Count )
            {
                LayoutRow( pos, startIndex, mContent.Count, rowHeight, maxWidth );
                pos.y += rowHeight;
            }

            Rect.width  = maxWidth;
            Rect.height = pos.y - origin.y;
        }

        void LayoutRow( Vector2 pos, int from, int until, float rowHeight, float maxWidth )
        {
            if( CellAlignment != TextAnchor.UpperLeft )
            {
                var rowContentWidth = 0f;
                for( var i = from; i < until; i++ )
                    rowContentWidth += mContent[ i ].Width;

                if( CellAlignment == TextAnchor.UpperCenter )
                    pos.x += Mathf.Max( 0f, ( maxWidth - rowContentWidth ) / 2f );
                else if( CellAlignment == TextAnchor.UpperRight )
                    pos.x += Mathf.Max( 0f, maxWidth - rowContentWidth );
            }

            for( var i = from; i < until; i++ )
            {
                var content = mContent[ i ];
                content.Location.x = pos.x;
                content.Location.y = pos.y + rowHeight - content.Height;
                pos.x += content.Width;
            }
        }

        public override void Draw( Context context )
        {
            mContent.ForEach( c => c.Draw( context ) );
            mPrefix?.Draw( context );
        }
    }

    public class BlockLine : Block
    {
        public BlockLine( float indent ) : base( indent ) { }

        public override void Arrange( Context context, Vector2 pos, float maxWidth )
        {
            Rect.position = pos;
            Rect.width    = maxWidth;
            Rect.height   = 10.0f;
        }

        public override void Draw( Context context )
        {
            var rect = new Rect( Rect.position.x, Rect.center.y, Rect.width, 1.0f );
            GUI.Label( rect, string.Empty, GUI.skin.GetStyle( "hr" ) );
        }
    }

    public class BlockSpace : Block
    {
        public BlockSpace( float indent ) : base( indent ) { }

        public override void Arrange( Context context, Vector2 pos, float maxWidth )
        {
            Rect.position = pos;
            Rect.width    = 1.0f;
            Rect.height   = context.LineHeight * 0.75f;
        }

        public override void Draw( Context context ) { }
    }

    public class BlockCollapsible : Block
    {
        public string         Summary;
        public BlockContainer Content;

        static readonly Dictionary<string, bool> sOpenState = new Dictionary<string, bool>();

        bool IsOpen
        {
            get => sOpenState.TryGetValue( Summary, out var open ) && open;
            set => sOpenState[ Summary ] = value;
        }

        Rect mToggleRect;

        public BlockCollapsible( float indent ) : base( indent )
        {
            Content = new BlockContainer( indent );
        }

        public override void Arrange( Context context, Vector2 pos, float maxWidth )
        {
            var toggleHeight = context.LineHeight * 1.5f;

            mToggleRect   = new Rect( pos.x + Indent, pos.y, maxWidth - Indent, toggleHeight );
            Rect.position = new Vector2( pos.x + Indent, pos.y );
            Rect.width    = maxWidth - Indent;

            if( IsOpen )
            {
                Content.Arrange( context, new Vector2( pos.x, pos.y + toggleHeight ), maxWidth );
                Rect.height = toggleHeight + Content.Rect.height;
            }
            else
            {
                Rect.height = toggleHeight;
            }
        }

        public override void Draw( Context context )
        {
            var isOpen = IsOpen;
            var label  = ( isOpen ? "▼  " : "▶  " ) + Summary;

            EditorGUI.DrawRect( mToggleRect, new Color( 0.5f, 0.5f, 0.5f, 0.08f ) );
            GUI.Label( mToggleRect, label, EditorStyles.boldLabel );
            EditorGUIUtility.AddCursorRect( mToggleRect, MouseCursor.Link );

            if( Event.current.type == EventType.MouseDown && mToggleRect.Contains( Event.current.mousePosition ) )
            {
                IsOpen = !isOpen;
                Event.current.Use();
                GUI.changed = true;
            }

            if( isOpen ) Content.Draw( context );
        }
    }
}
