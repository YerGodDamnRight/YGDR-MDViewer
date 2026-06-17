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

        public List<List<Rect>> FindMatches( string query )
        {
            var results = new List<List<Rect>>();
            if( !string.IsNullOrEmpty( query ) )
                mDocument.CollectMatches( query, results, mContext );
            return results;
        }

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
