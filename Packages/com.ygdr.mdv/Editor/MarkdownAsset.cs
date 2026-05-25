using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;


namespace YGDR.MDV
{
    public class MarkdownAsset : TextAsset { }

    [ScriptedImporter( 1, "markdown" )]
    public class MarkdownAssetImporter : ScriptedImporter
    {
        public override void OnImportAsset( AssetImportContext ctx )
        {
            var md = new MarkdownAsset();
            ctx.AddObjectToAsset( "main", md );
            ctx.SetMainObject( md );
        }
    }

    public static class MarkdownMenus
    {
        static string GetFilePath( string filename )
        {
            var path = AssetDatabase.GetAssetPath( Selection.activeObject );

            if( string.IsNullOrEmpty( path ) )
                path = "Assets";
            else if( !AssetDatabase.IsValidFolder( path ) )
                path = Path.GetDirectoryName( path );

            return AssetDatabase.GenerateUniqueAssetPath( path + "/" + filename );
        }

        [MenuItem( "Assets/Create/YGDR/Markdown" )]
        static void CreateMarkdown()
        {
            var filepath = GetFilePath( "NewMarkdown.md" );
            var writer   = File.CreateText( filepath );
            var template = EditorGUIUtility.Load( "MarkdownTemplate.md" ) as TextAsset;

            writer.Write( template != null ? template.text : "# Markdown\n" );
            writer.Close();

            AssetDatabase.ImportAsset( filepath );
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>( filepath );
        }
    }
}
