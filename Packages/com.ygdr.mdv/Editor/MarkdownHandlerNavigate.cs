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
using System.IO;
using UnityEditor;
using UnityEngine;

namespace YGDR.MDV
{
    public class MarkdownHandlerNavigate
    {
        public MarkdownHistory History;
        public string          CurrentPath;

        public Action<float>      ScrollTo;
        public Func<string,Block> FindBlock;

        public void SelectPage( string url )
        {
            if( string.IsNullOrEmpty( url ) ) return;

            if( url.StartsWith( "#" ) )
            {
                var block = FindBlock( url.ToLower() );

                if( block != null )
                    ScrollTo( block.Rect.y );
                else
                    Debug.LogWarning( string.Format( "Unable to find section header {0}", url ) );

                return;
            }

            var newPath = url.StartsWith( "/" )
                ? url.Substring( 1 )
                : MarkdownUtils.PathCombine( Path.GetDirectoryName( CurrentPath ), url );

            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>( newPath );

            if( asset != null )
            {
                History.Add( newPath );
                Selection.activeObject = asset;
            }
            else
            {
                Debug.LogError( string.Format( "Could not find asset {0}", newPath ) );
            }
        }
    }
}
