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


using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace YGDR.MDV
{
    internal static class SyntaxHighlighter
    {
        internal static readonly Color Keyword = new Color( 0.341f, 0.612f, 0.835f ); // #569CD6
        internal static readonly Color String_ = new Color( 0.808f, 0.569f, 0.471f ); // #CE9178
        internal static readonly Color Comment = new Color( 0.416f, 0.600f, 0.333f ); // #6A9955
        internal static readonly Color Number  = new Color( 0.710f, 0.808f, 0.659f ); // #B5CEA8
        internal static readonly Color Type_   = new Color( 0.306f, 0.788f, 0.690f ); // #4EC9B0

        static readonly HashSet<string> CSharpKeywords = new HashSet<string>
        {
            "abstract","as","base","bool","break","byte","case","catch","char","checked",
            "class","const","continue","decimal","default","delegate","do","double","else",
            "enum","event","explicit","extern","false","finally","fixed","float","for",
            "foreach","goto","if","implicit","in","int","interface","internal","is","lock",
            "long","namespace","new","null","object","operator","out","override","params",
            "private","protected","public","readonly","ref","return","sbyte","sealed",
            "short","sizeof","stackalloc","static","string","struct","switch","this",
            "throw","true","try","typeof","uint","ulong","unchecked","unsafe","ushort",
            "using","var","virtual","void","volatile","while","async","await","yield",
            "get","set","value","partial","where","nameof","when","record","init"
        };

        static readonly Regex CSharpPattern = new Regex(
            @"(?<comment>//[^\n]*)" +
            @"|(?<verbatim>@""[^""]*(?:""""[^""]*)*"")" +
            @"|(?<string>""[^""\n\\]*(?:\\.[^""\n\\]*)*"")" +
            @"|(?<char>'[^'\n\\](?:\\.[^'\\]*)*')" +
            @"|(?<number>\b\d+\.?\d*[fFdDmMLluU]*\b)" +
            @"|(?<word>[a-zA-Z_]\w*)",
            RegexOptions.Compiled );

        static readonly Regex JsonPattern = new Regex(
            @"(?<string>""[^""\\]*(?:\\.[^""\\]*)*"")" +
            @"|(?<number>-?\b\d+\.?\d*(?:[eE][+-]?\d+)?\b)" +
            @"|(?<keyword>\b(?:true|false|null)\b)",
            RegexOptions.Compiled );

        internal static bool IsSupported( string lang ) =>
            lang == "csharp" || lang == "cs" || lang == "json";

        internal static List<(string text, Color color)> TokenizeLine( string line, string lang )
        {
            var tokens = new List<(string, Color)>();
            if( string.IsNullOrEmpty( line ) ) return tokens;

            var pattern = lang == "json" ? JsonPattern : CSharpPattern;
            var pos     = 0;

            foreach( Match match in pattern.Matches( line ) )
            {
                if( match.Index > pos )
                    tokens.Add( ( line.Substring( pos, match.Index - pos ), Color.clear ) );

                tokens.Add( ( match.Value, GetColor( match, lang ) ) );
                pos = match.Index + match.Length;
            }

            if( pos < line.Length )
                tokens.Add( ( line.Substring( pos ), Color.clear ) );

            return tokens;
        }

        static Color GetColor( Match match, string lang )
        {
            if( lang == "json" )
            {
                if( match.Groups[ "string"  ].Success ) return String_;
                if( match.Groups[ "number"  ].Success ) return Number;
                if( match.Groups[ "keyword" ].Success ) return Keyword;
                return Color.clear;
            }

            if( match.Groups[ "comment"  ].Success ) return Comment;
            if( match.Groups[ "verbatim" ].Success ) return String_;
            if( match.Groups[ "string"   ].Success ) return String_;
            if( match.Groups[ "char"     ].Success ) return String_;
            if( match.Groups[ "number"   ].Success ) return Number;

            if( match.Groups[ "word" ].Success )
            {
                var word = match.Value;
                if( CSharpKeywords.Contains( word ) ) return Keyword;
                if( char.IsUpper( word[ 0 ] ) )       return Type_;
            }

            return Color.clear;
        }
    }
}
