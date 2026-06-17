/*
    YGDR Markdown Viewer - View / Edit rendered markdsown in Unity
    Copyright (C) 2026  YerGodDamnRight

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/


using System.Text;
using UnityEngine;

namespace YGDR.MDV
{
    public class LayoutBuilder : IBuilder
    {
        Context        mContext;
        Style          mStyle;
        string         mLink;
        string         mTooltip;
        StringBuilder  mWord;
        float          mIndent;

        BlockContainer mDocument;
        BlockContainer mCurrentContainer;
        Block          mCurrentBlock;
        BlockContent   mCurrentContent;

        TableAlign[] mTableColumnAlignments;
        int          mTableCellIndex;

        Block CurrentBlock
        {
            get => mCurrentBlock;
            set
            {
                mCurrentBlock   = value;
                mCurrentContent = mCurrentBlock as BlockContent;
            }
        }

        public LayoutBuilder( Context context )
        {
            mContext          = context;
            mStyle            = new Style();
            mLink             = null;
            mTooltip          = null;
            mWord             = new StringBuilder( 1024 );
            mIndent           = 0.0f;
            mDocument         = new BlockContainer( mIndent );
            mCurrentContainer = mDocument;
            mCurrentBlock     = null;
            mCurrentContent   = null;
        }

        public Layout GetLayout() => new Layout( mContext, mDocument );

        public void Text( string text, Style style, string link, string tooltip )
        {
            if( mCurrentContent == null ) NewContentBlock();

            mContext.Apply( style );

            if( style.Size > 0 )
            {
                mCurrentContent.ID = mCurrentContent.ID == null
                    ? "#"
                    : mCurrentContent.ID + "-";

                mCurrentContent.ID += text.Trim().Replace( ' ', '-' ).ToLower();
            }

            mStyle   = style;
            mLink    = link;
            mTooltip = tooltip;

            for( var i = 0; i < text.Length; i++ )
            {
                var ch = text[ i ];

                if( ch == '\n' )
                {
                    AddWord();
                    NewLine();
                }
                else if( char.IsWhiteSpace( ch ) )
                {
                    mWord.Append( ' ' );
                    AddWord();
                }
                else
                {
                    mWord.Append( ch );
                }
            }

            AddWord();
        }

        public void Image( string url, string alt, string title, float overrideWidth = 0f, float overrideHeight = 0f )
        {
            var payload = new GUIContent();
            var content = new ContentImage( payload, mStyle, mLink );

            content.URL            = url;
            content.Alt            = alt;
            content.Tooltip        = title;
            content.OverrideWidth  = overrideWidth;
            content.OverrideHeight = overrideHeight;

            AddContent( content );
        }

        public void NewLine()
        {
            if( mCurrentContent != null && mCurrentContent.IsEmpty ) return;
            if( InTableRow ) mTableCellIndex++;
            NewContentBlock();
        }

        public void Space()
        {
            if( CurrentBlock is BlockSpace || CurrentBlock is BlockContainer ) return;
            AddBlock( new BlockSpace( mIndent ) );
        }

        public void HorizontalLine()
        {
            if( CurrentBlock is BlockLine ) return;
            AddBlock( new BlockLine( mIndent ) );
        }

        public void Indent()
        {
            NewLine();
            mIndent += mContext.IndentSize;
            if( mCurrentContent != null ) mCurrentContent.Indent = mIndent;
        }

        public void Outdent()
        {
            NewLine();
            mIndent = Mathf.Max( mIndent - mContext.IndentSize, 0.0f );
            if( mCurrentContent != null ) mCurrentContent.Indent = mIndent;
        }

        public void Prefix( string text, Style style )
        {
            mContext.Apply( style );
            if( mCurrentContent == null ) return;

            var payload = new GUIContent( text );
            var content = new ContentText( payload, style, null );
            content.Location.size = mContext.CalcSize( payload );
            mCurrentContent.Prefix( content );
        }

        public void StartBlock( bool quoted, AlertType alertType = AlertType.None )
        {
            Space();
            mCurrentContainer = AddBlock( new BlockContainer( mIndent ) { Highlight = true, Quoted = quoted, AlertType = alertType } );
            CurrentBlock = null;
        }

        public void EndBlock()
        {
            mCurrentContainer.RemoveTrailingSpace();
            mCurrentContainer = mCurrentContainer.Parent as BlockContainer ?? mDocument;
            CurrentBlock = null;
            Space();
        }

        public void StartCollapsible( string summary )
        {
            Space();
            var collapsible = new BlockCollapsible( mIndent ) { Summary = summary };
            AddBlock( collapsible );
            collapsible.Content.Parent = collapsible;
            mCurrentContainer          = collapsible.Content;
            CurrentBlock               = null;
        }

        public void EndCollapsible()
        {
            var parentCollapsible = mCurrentContainer.Parent as BlockCollapsible;
            if( parentCollapsible == null ) return;
            mCurrentContainer.RemoveTrailingSpace();
            mCurrentContainer = parentCollapsible.Parent as BlockContainer ?? mDocument;
            CurrentBlock      = null;
            Space();
        }

        public void StartTable( TableAlign[] columnAlignments )
        {
            Space();
            mTableColumnAlignments = columnAlignments;
            mCurrentContainer = AddBlock( new BlockContainer( mIndent ) { Quoted = false, Highlight = false } );
            CurrentBlock = null;
        }

        public void EndTable()
        {
            mCurrentContainer.RemoveTrailingSpace();
            mCurrentContainer = mCurrentContainer.Parent as BlockContainer ?? mDocument;
            CurrentBlock = null;
            Space();
        }

        public void StartTableRow( bool isHeader )
        {
            mTableCellIndex   = 0;
            mCurrentContainer = AddBlock( new BlockContainer( mIndent )
            {
                Horizontal    = true,
                IsTableHeader = isHeader,
                IsTableRow    = !isHeader
            } );
            CurrentBlock = null;
        }

        public void EndTableRow()
        {
            mCurrentContainer.RemoveTrailingSpace();
            mCurrentContainer = mCurrentContainer.Parent as BlockContainer ?? mDocument;
            CurrentBlock = null;
        }

        void AddContent( Content content )
        {
            if( mCurrentContent == null ) NewContentBlock();
            mCurrentContent.Add( content );
        }

        T AddBlock<T>( T block ) where T : Block
        {
            CurrentBlock = mCurrentContainer.Add( block );
            return block;
        }

        bool InTableRow => mCurrentContainer.IsTableRow || mCurrentContainer.IsTableHeader;

        static TextAnchor ToTextAnchor( TableAlign align )
        {
            switch( align )
            {
                case TableAlign.Center: return TextAnchor.UpperCenter;
                case TableAlign.Right:  return TextAnchor.UpperRight;
                default:                return TextAnchor.UpperLeft;
            }
        }

        void NewContentBlock()
        {
            var block = new BlockContent( mIndent );

            if( InTableRow && mTableColumnAlignments != null && mTableCellIndex < mTableColumnAlignments.Length )
                block.CellAlignment = ToTextAnchor( mTableColumnAlignments[ mTableCellIndex ] );

            AddBlock( block );
            mStyle.Clear();
            mContext.Apply( mStyle );
        }

        void AddWord()
        {
            if( mWord.Length == 0 ) return;

            var payload = new GUIContent( mWord.ToString(), mTooltip );
            var content = new ContentText( payload, mStyle, mLink );
            content.CalcSize( mContext );

            if( mStyle.Kbd )
                content.Location.width += Content.KbdPadding * 2f;

            AddContent( content );
            mWord.Length = 0;
        }
    }
}
