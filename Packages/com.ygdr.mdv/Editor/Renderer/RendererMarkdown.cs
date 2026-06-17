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


////////////////////////////////////////////////////////////////////////////////

using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace YGDR.MDV
{
    ////////////////////////////////////////////////////////////////////////////////
    /// <see cref="Markdig.Renderers.HtmlRenderer"/>
    /// <see cref="Markdig.Renderers.Normalize.NormalizeRenderer"/>

    public class RendererMarkdown : RendererBase
    {
        internal LayoutBuilder  Layout;
        internal Style          Style   = new Style();
        internal string         ToolTip = null;
        internal string         Link
        {
            get
            {
                return mLink;
            }

            set
            {
                mLink = value;
                Style.Link = !string.IsNullOrEmpty( mLink );
            }
        }

        public bool ConsumeSpace = false;
        public bool ConsumeNewLine = false;
        internal string Source = null;
        internal int    ConsumeChars = 0;

        private string mLink = null;

        internal void Text( string text )
        {
            if( ConsumeChars > 0 )
            {
                if( text.Length <= ConsumeChars ) { ConsumeChars -= text.Length; return; }
                text = text.Substring( ConsumeChars );
                ConsumeChars = 0;
            }
            Layout.Text( text, Style, Link, ToolTip );
        }


        //------------------------------------------------------------------------------

        public override object Render( MarkdownObject document )
        {
            Write( document );
            return this;
        }

        public RendererMarkdown( LayoutBuilder doc )
        {
            Layout = doc;

            ObjectRenderers.Add( new RendererBlockCode() );
            ObjectRenderers.Add( new RendererBlockList() );
            ObjectRenderers.Add( new RendererBlockHeading() );
            ObjectRenderers.Add( new RendererBlockHtml() );
            ObjectRenderers.Add( new RendererBlockParagraph() );
            ObjectRenderers.Add( new RendererBlockQuote() );
            ObjectRenderers.Add( new RendererBlockThematicBreak() );
            ObjectRenderers.Add( new RendererTable() );

            ObjectRenderers.Add( new RendererInlineLink() );
            ObjectRenderers.Add( new RendererInlineAutoLink() );
            ObjectRenderers.Add( new RendererInlineCode() );
            ObjectRenderers.Add( new RendererInlineDelimiter() );
            ObjectRenderers.Add( new RendererInlineEmphasis() );
            ObjectRenderers.Add( new RendererInlineLineBreak() );
            ObjectRenderers.Add( new RendererInlineHtml() );
            ObjectRenderers.Add( new RendererInlineHtmlEntity() );
            ObjectRenderers.Add( new RendererInlineLiteral() );
            ObjectRenderers.Add( new RendererInlineCheckbox() );
        }


        ////////////////////////////////////////////////////////////////////////////////

        /// <see cref="Markdig.Renderers.TextRendererBase.WriteLeafInline"/>

        internal void WriteLeafBlockInline( LeafBlock block )
        {
            var inline = block.Inline as Inline;

            while( inline != null )
            {
                Write( inline );
                inline = inline.NextSibling;
            }
        }

        /// <summary>
        /// Output child nodes as raw text
        /// </summary>
        /// <see cref="Markdig.Renderers.HtmlRenderer.WriteLeafRawLines"/>

        internal void WriteLeafRawLines( LeafBlock block )
        {
            if( block.Lines.Lines == null )
            {
                return;
            }

            var lines  = block.Lines;
            var slices = lines.Lines;

            for( int i = 0; i < lines.Count; i++ )
            {
                Text( slices[ i ].ToString() );
                Layout.NewLine();
            }
        }

        internal string GetContents( ContainerInline node )
        {
            if( node == null )
            {
                return string.Empty;
            }

            /// <see cref="Markdig.Renderers.RendererBase.WriteChildren(ContainerInline)"/>
            
            var inline  = node.FirstChild;
            var content = string.Empty;

            while( inline != null )
            {
                var lit = inline as LiteralInline;

                if( lit != null )
                {
                    content += lit.Content.ToString();
                }

                inline = inline.NextSibling;
            }

            return content;
        }

        //------------------------------------------------------------------------------

        internal void FinishBlock( bool space = false )
        {
            if( space && !ConsumeSpace )
            {
                Layout.Space();
            }
            else if ( !ConsumeNewLine )
            {
                Layout.NewLine();
            }
        }
    }
}
