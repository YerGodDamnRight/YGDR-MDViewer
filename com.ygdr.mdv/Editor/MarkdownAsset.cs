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
