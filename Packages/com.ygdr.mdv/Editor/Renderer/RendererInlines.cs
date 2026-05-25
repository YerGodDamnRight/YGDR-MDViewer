using System.Text.RegularExpressions;
using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using UnityEngine;

namespace YGDR.MDV
{
    // <a href="...">url</a> (auto-detected URL)
    public class RendererInlineAutoLink : MarkdownObjectRenderer<RendererMarkdown, AutolinkInline>
    {
        protected override void Write( RendererMarkdown renderer, AutolinkInline node )
        {
            renderer.Link = node.Url;
            renderer.Text( node.Url );
            renderer.Link = null;
        }
    }

    // <code>...</code>
    public class RendererInlineCode : MarkdownObjectRenderer<RendererMarkdown, CodeInline>
    {
        protected override void Write( RendererMarkdown renderer, CodeInline node )
        {
            var prevStyle = renderer.Style;
            renderer.Style.Fixed = true;
            renderer.Text( node.Content );
            renderer.Style = prevStyle;
        }
    }

    // Unmatched delimiter fallback (e.g. lone * or _)
    public class RendererInlineDelimiter : MarkdownObjectRenderer<RendererMarkdown, DelimiterInline>
    {
        protected override void Write( RendererMarkdown renderer, DelimiterInline node )
        {
            renderer.Text( node.ToLiteral() );
            renderer.WriteChildren( node );
        }
    }

    // <b><i>...</i></b> <del>...</del>
    public class RendererInlineEmphasis : MarkdownObjectRenderer<RendererMarkdown, EmphasisInline>
    {
        protected override void Write( RendererMarkdown renderer, EmphasisInline node )
        {
            if( node.DelimiterChar == '~' )
            {
                var prev = renderer.Style.Strikethrough;
                renderer.Style.Strikethrough = true;
                renderer.WriteChildren( node );
                renderer.Style.Strikethrough = prev;
            }
            else if( node.IsDouble )
            {
                var prev = renderer.Style.Bold;
                renderer.Style.Bold = true;
                renderer.WriteChildren( node );
                renderer.Style.Bold = prev;
            }
            else
            {
                var prev = renderer.Style.Italic;
                renderer.Style.Italic = true;
                renderer.WriteChildren( node );
                renderer.Style.Italic = prev;
            }
        }
    }

    // Inline HTML tag (e.g. <br>, <span>)
    public class RendererInlineHtml : MarkdownObjectRenderer<RendererMarkdown, HtmlInline>
    {
        protected override void Write( RendererMarkdown renderer, HtmlInline node )
        {
            var tag = node.Tag;

            if( tag.Equals( "<kbd>", System.StringComparison.OrdinalIgnoreCase ) )
            {
                renderer.Style.Kbd = true;
                return;
            }

            if( tag.Equals( "</kbd>", System.StringComparison.OrdinalIgnoreCase ) )
            {
                renderer.Style.Kbd = false;
                return;
            }

            renderer.Text( tag );
        }
    }

    // HTML entity (e.g. &amp; &lt;)
    public class RendererInlineHtmlEntity : MarkdownObjectRenderer<RendererMarkdown, HtmlEntityInline>
    {
        protected override void Write( RendererMarkdown renderer, HtmlEntityInline node )
        {
            renderer.Text( node.Transcoded.ToString() );
        }
    }

    // <br/> hard break or soft space
    public class RendererInlineLineBreak : MarkdownObjectRenderer<RendererMarkdown, LineBreakInline>
    {
        protected override void Write( RendererMarkdown renderer, LineBreakInline node )
        {
            if( node.IsHard )
                renderer.FinishBlock();
            else
                renderer.Text( " " );
        }
    }

    // <img src="..." /> or <a href="...">...</a>
    public class RendererInlineLink : MarkdownObjectRenderer<RendererMarkdown, LinkInline>
    {
        protected override void Write( RendererMarkdown renderer, LinkInline node )
        {
            var url = node.GetDynamicUrl != null ? node.GetDynamicUrl() : node.Url;

            if( node.IsImage )
            {
                ParseImageSize( node.Title, out string cleanTitle, out float overrideWidth, out float overrideHeight );
                renderer.Layout.Image( url, renderer.GetContents( node ), cleanTitle, overrideWidth, overrideHeight );
            }
            else
            {
                renderer.Link = url;

                if( !string.IsNullOrEmpty( node.Title ) )
                    renderer.ToolTip = node.Title;

                renderer.WriteChildren( node );

                renderer.ToolTip = null;
                renderer.Link    = null;
            }
        }

        static void ParseImageSize( string title, out string cleanTitle, out float width, out float height )
        {
            width      = 0f;
            height     = 0f;
            cleanTitle = title;

            if( string.IsNullOrEmpty( title ) ) return;

            var match = Regex.Match( title, @"\s*=(\d+)(?:x(\d+))?\s*$" );
            if( !match.Success ) return;

            width = float.Parse( match.Groups[ 1 ].Value );
            if( match.Groups[ 2 ].Success )
                height = float.Parse( match.Groups[ 2 ].Value );

            cleanTitle = title.Substring( 0, match.Index );
        }
    }

    // Plain text literal
    public class RendererInlineLiteral : MarkdownObjectRenderer<RendererMarkdown, LiteralInline>
    {
        protected override void Write( RendererMarkdown renderer, LiteralInline node )
        {
            renderer.Text( node.Content.ToString() );
        }
    }

    // Standalone [x] [ ] [-] outside list items
    public class CheckboxInline : LeafInline
    {
        public bool Checked { get; set; }
        public bool Blocked { get; set; }
    }

    public class CheckboxInlineParser : InlineParser
    {
        public CheckboxInlineParser()
        {
            OpeningCharacters = new[] { '[' };
        }

        public override bool Match( InlineProcessor processor, ref StringSlice slice )
        {
            var next      = slice.PeekChar( 1 );
            var afterNext = slice.PeekChar( 2 );

            if( afterNext != ']' ) return false;
            if( next != ' ' && next != 'x' && next != 'X' && next != '-' ) return false;

            Markdig.Syntax.Block block = processor.Block;
            while( block != null )
            {
                if( block is ListItemBlock ) return false;
                block = block.Parent;
            }

            var startPos = processor.GetSourcePosition( slice.Start, out int line, out int column );
            slice.Start += 3;

            processor.Inline = new CheckboxInline
            {
                Checked = next == 'x' || next == 'X',
                Blocked = next == '-',
                Span    = new SourceSpan( startPos, startPos + 2 ),
                Line    = line,
                Column  = column,
            };

            return true;
        }
    }

    public class CheckboxInlineExtension : IMarkdownExtension
    {
        public void Setup( MarkdownPipelineBuilder pipeline )
        {
            var linkParser = pipeline.InlineParsers.Find<LinkInlineParser>();
            var parser     = new CheckboxInlineParser();

            if( linkParser != null )
                pipeline.InlineParsers.Insert( pipeline.InlineParsers.IndexOf( linkParser ), parser );
            else
                pipeline.InlineParsers.Add( parser );
        }

        public void Setup( MarkdownPipeline pipeline, IMarkdownRenderer renderer ) { }
    }

    public class RendererInlineCheckbox : MarkdownObjectRenderer<RendererMarkdown, CheckboxInline>
    {
        protected override void Write( RendererMarkdown renderer, CheckboxInline node )
        {
            var prevStyle = renderer.Style;

            if( node.Checked )
            {
                renderer.Style.TokenColor = new Color( 0.2f, 0.75f, 0.2f );
                renderer.Text( "☑" );
            }
            else if( node.Blocked )
            {
                renderer.Style.TokenColor = new Color( 0.9f, 0.2f, 0.2f );
                renderer.Text( "☒" );
            }
            else
            {
                renderer.Text( "☐" );
            }

            renderer.Style = prevStyle;
        }
    }
}
