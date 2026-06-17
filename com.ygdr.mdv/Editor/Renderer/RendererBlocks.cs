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


using System;
using System.Linq;
using System.Text;
using Markdig.Extensions.Tables;
using Markdig.Extensions.TaskLists;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using UnityEngine;

namespace YGDR.MDV
{
    // <pre><code>...</code></pre>
    public class RendererBlockCode : MarkdownObjectRenderer<RendererMarkdown, CodeBlock>
    {
        protected override void Write( RendererMarkdown renderer, CodeBlock block )
        {
            var fenced = block as FencedCodeBlock;
            var lang   = fenced?.Info?.Trim().ToLower();

            var prevStyle = renderer.Style;
            renderer.Style.Fixed = true;
            renderer.Style.Block = true;

            renderer.Layout.StartBlock( false );

            if( lang != null && SyntaxHighlighter.IsSupported( lang ) )
                WriteHighlighted( renderer, block, lang );
            else
                renderer.WriteLeafRawLines( block );

            renderer.Layout.EndBlock();

            renderer.Style = prevStyle;
            renderer.FinishBlock( true );
        }

        static void WriteHighlighted( RendererMarkdown renderer, CodeBlock block, string lang )
        {
            if( block.Lines.Lines == null ) return;

            var lines  = block.Lines;
            var slices = lines.Lines;

            for( var i = 0; i < lines.Count; i++ )
            {
                var tokens = SyntaxHighlighter.TokenizeLine( slices[ i ].ToString(), lang );

                foreach( var (text, color) in tokens )
                {
                    renderer.Style.TokenColor = color;
                    renderer.Text( text );
                }

                renderer.Style.TokenColor = Color.clear;
                renderer.Layout.NewLine();
            }
        }
    }

    // <h1>...</h1>
    public class RendererBlockHeading : MarkdownObjectRenderer<RendererMarkdown, HeadingBlock>
    {
        protected override void Write( RendererMarkdown renderer, HeadingBlock block )
        {
            var prevSize = renderer.Style.Size;
            renderer.Style.Size = block.Level;
            renderer.WriteLeafBlockInline( block );
            renderer.Style.Size = prevSize;

            if( block.Level == 1 )
            {
                renderer.Layout.HorizontalLine();
                renderer.FinishBlock( true );
            }
            else
            {
                renderer.FinishBlock();
            }
        }
    }

    // HTML block (stripped when Preferences.StripHTML)
    public class RendererBlockHtml : MarkdownObjectRenderer<RendererMarkdown, HtmlBlock>
    {
        protected override void Write( RendererMarkdown renderer, HtmlBlock block )
        {
            var rawHtml = GetRawContent( block ).Trim();

            if( rawHtml.StartsWith( "<details", StringComparison.OrdinalIgnoreCase ) )
            {
                renderer.Layout.StartCollapsible( ExtractSummary( rawHtml ) ?? "Details" );
                return;
            }

            if( rawHtml.StartsWith( "</details", StringComparison.OrdinalIgnoreCase ) )
            {
                renderer.Layout.EndCollapsible();
                return;
            }

            if( !MarkdownPreferences.StripHTML )
            {
                renderer.WriteLeafRawLines( block );
                renderer.FinishBlock();
            }
        }

        static string GetRawContent( HtmlBlock block )
        {
            if( block.Lines.Lines == null ) return string.Empty;
            var sb = new StringBuilder();
            for( var i = 0; i < block.Lines.Count; i++ )
                sb.AppendLine( block.Lines.Lines[ i ].ToString() );
            return sb.ToString();
        }

        static string ExtractSummary( string html )
        {
            const string startTag = "<summary>";
            const string endTag   = "</summary>";
            var start = html.IndexOf( startTag, StringComparison.OrdinalIgnoreCase );
            if( start < 0 ) return null;
            start += startTag.Length;
            var end = html.IndexOf( endTag, start, StringComparison.OrdinalIgnoreCase );
            if( end < 0 ) return null;
            return html.Substring( start, end - start ).Trim();
        }
    }

    // <ul><li>...</li></ul>
    public class RendererBlockList : MarkdownObjectRenderer<RendererMarkdown, ListBlock>
    {
        protected override void Write( RendererMarkdown renderer, ListBlock block )
        {
            var layout = renderer.Layout;

            layout.Space();
            layout.Indent();

            var prevConsumeSpace = renderer.ConsumeSpace;
            renderer.ConsumeSpace = true;

            for( var i = 0; i < block.Count; i++ )
            {
                var listItem  = block[ i ] as ListItemBlock;
                var taskList  = GetTaskList( listItem );
                var prefixStyle = renderer.Style;

                var isBlocked = taskList == null && IsBlockedItem( listItem, renderer.Source );

                string prefix;
                if( taskList != null )
                {
                    if( taskList.Checked )
                    {
                        prefix = "☑";
                        prefixStyle.TokenColor = new Color( 0.2f, 0.75f, 0.2f );
                    }
                    else
                    {
                        prefix = "☐";
                    }
                }
                else if( isBlocked )
                {
                    prefix = "☒";
                    prefixStyle.TokenColor = new Color( 0.9f, 0.2f, 0.2f );
                }
                else if( block.IsOrdered )
                    prefix = (i + 1).ToString() + ".";
                else
                {
                    prefix = "•";
                    prefixStyle.Bold = true;
                }

                layout.Prefix( prefix, prefixStyle );
                if( isBlocked ) renderer.ConsumeChars = 3;
                renderer.WriteChildren( listItem );
            }

            renderer.ConsumeSpace = prevConsumeSpace;
            layout.Outdent();
            layout.Space();
        }

        static TaskList GetTaskList( ListItemBlock item )
        {
            if( item == null || item.Count == 0 ) return null;
            return (item[ 0 ] as ParagraphBlock)?.Inline?.FirstChild as TaskList;
        }

        static int FindBlockedBracket( ListItemBlock item, string source )
        {
            if( source == null || item == null ) return -1;
            int start = item.Span.Start;
            while( start < source.Length && source[ start ] != '[' ) start++;
            if( start + 2 >= source.Length ) return -1;
            return source[ start ] == '[' && source[ start + 1 ] == '-' && source[ start + 2 ] == ']' ? start : -1;
        }

        static bool IsBlockedItem( ListItemBlock item, string source ) => FindBlockedBracket( item, source ) >= 0;

    }

    // <p>...</p>
    public class RendererBlockParagraph : MarkdownObjectRenderer<RendererMarkdown, ParagraphBlock>
    {
        protected override void Write( RendererMarkdown renderer, ParagraphBlock block )
        {
            renderer.WriteLeafBlockInline( block );
            renderer.FinishBlock( true );
        }
    }

    // <blockquote>...</blockquote>
    public class RendererBlockQuote : MarkdownObjectRenderer<RendererMarkdown, QuoteBlock>
    {
        protected override void Write( RendererMarkdown renderer, QuoteBlock block )
        {
            var alertType = DetectAlertType( renderer, block );

            var prevConsumeSpace = renderer.ConsumeSpace;
            renderer.ConsumeSpace = false;

            renderer.Layout.StartBlock( true, alertType );
            renderer.WriteChildren( block );
            renderer.Layout.EndBlock();

            renderer.ConsumeSpace = prevConsumeSpace;
            renderer.FinishBlock( true );
        }

        static AlertType DetectAlertType( RendererMarkdown renderer, QuoteBlock block )
        {
            if( block.Count == 0 ) return AlertType.None;

            var firstParagraph = block[ 0 ] as ParagraphBlock;
            if( firstParagraph == null ) return AlertType.None;

            var text = renderer.GetContents( firstParagraph.Inline );

            if( text.StartsWith( "[!NOTE]" ) )      return AlertType.Note;
            if( text.StartsWith( "[!TIP]" ) )       return AlertType.Tip;
            if( text.StartsWith( "[!IMPORTANT]" ) ) return AlertType.Important;
            if( text.StartsWith( "[!WARNING]" ) )   return AlertType.Warning;
            if( text.StartsWith( "[!CAUTION]" ) )   return AlertType.Caution;

            return AlertType.None;
        }
    }

    // <hr/>
    public class RendererBlockThematicBreak : MarkdownObjectRenderer<RendererMarkdown, ThematicBreakBlock>
    {
        protected override void Write( RendererMarkdown renderer, ThematicBreakBlock block )
        {
            renderer.Layout.HorizontalLine();
            renderer.FinishBlock();
        }
    }

    // <table>...</table>
    public class RendererTable : MarkdownObjectRenderer<RendererMarkdown, Table>
    {
        protected override void Write( RendererMarkdown renderer, Table table )
        {
            var layout = renderer.Layout;

            if( table.Count == 0 ) return;

            var numCols          = ( table[ 0 ] as TableRow ).Count( c => ( c as TableCell ).Count > 0 );
            var columnAlignments = table.ColumnDefinitions
                .Select( cd => cd.Alignment == TableColumnAlign.Center ? TableAlign.Center
                             : cd.Alignment == TableColumnAlign.Right  ? TableAlign.Right
                             : TableAlign.Left )
                .ToArray();

            layout.StartTable( columnAlignments );

            foreach( TableRow row in table )
            {
                if( row == null ) continue;

                layout.StartTableRow( row.IsHeader );

                var prevConsumeSpace  = renderer.ConsumeSpace;
                renderer.ConsumeSpace = true;

                var numCells = Mathf.Min( numCols, row.Count );

                for( var cellIndex = 0; cellIndex < numCells; cellIndex++ )
                {
                    var cell = row[ cellIndex ] as TableCell;

                    if( cell == null || cell.Count == 0 ) continue;

                    if( cell[ 0 ].Span.IsEmpty )
                    {
                        renderer.Write( new LiteralInline( " " ) );
                        if( cellIndex != row.Count - 1 ) layout.NewLine();
                    }
                    else
                    {
                        var prevConsumeNewLine = renderer.ConsumeNewLine;
                        if( cellIndex == numCols - 1 ) renderer.ConsumeNewLine = true;

                        renderer.Write( new LiteralInline( " " ) );
                        renderer.WriteChildren( cell );

                        renderer.ConsumeNewLine = prevConsumeNewLine;
                    }
                }

                renderer.ConsumeSpace = prevConsumeSpace;
                layout.EndTableRow();
            }

            layout.EndTable();
        }
    }
}
