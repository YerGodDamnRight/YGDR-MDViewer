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
