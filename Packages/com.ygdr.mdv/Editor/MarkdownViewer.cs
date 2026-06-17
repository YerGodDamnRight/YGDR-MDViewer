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
using System.Collections.Generic;
using System.IO;
using Markdig;
using UnityEditor;
using UnityEngine;
using Markdig.Extensions.JiraLinks;
using Markdig.Extensions.Tables;

namespace YGDR.MDV
{
    public class MarkdownViewer
    {
        public static readonly Vector2 Margin = new Vector2( 6.0f, 4.0f );

        private GUISkin                 mSkin            = null;
        private string                  mText            = string.Empty;
        private string                  mCurrentPath     = string.Empty;
        private MarkdownHandlerImages   mHandlerImages   = new MarkdownHandlerImages();
        private MarkdownHandlerNavigate mHandlerNavigate = new MarkdownHandlerNavigate();

        private Func<float> mViewWidthProvider = () => EditorGUIUtility.currentViewWidth;

        private Layout mLayout  = null;
        public  bool   Editable = true;
        private bool   mRaw     = false;
        private string mEditedText = null;

        public  float? PendingScrollY    = null;
        public  float  ViewportHeight    = 400f;
        public  List<List<Rect>> SearchMatches = null;
        public  int              SearchMatchIndex = -1;

        private string    mAnchorTarget     = null;
        private int       mAnchorHoldFrames = 0;
        private const int AnchorHoldFrameCount = 8;
        public  bool      AnchorScrolling => mAnchorHoldFrames > 0;

        private static MarkdownHistory mHistory = new MarkdownHistory();

        // Raw mode API consumed by MDVWindow UITK panel
        public bool   IsRaw          => mRaw;
        public string CurrentRawText => mEditedText ?? mText;

        public void CommitRawEdit( string text )
        {
            mEditedText = ( text == mText ) ? null : text;
        }

        public void TrySave() => SaveFile();

        public void ExitRawMode()
        {
            if( !mRaw ) return;
            if( mEditedText != null )
            {
                mLayout = ParseDocument( mEditedText );
                SaveFile();
            }
            mRaw = false;
        }

        public MarkdownViewer( GUISkin skin, string path, string content )
        {
            mSkin        = skin;
            mCurrentPath = path;
            mText        = content;

            mHistory.OnOpen( mCurrentPath );
            mLayout = ParseDocument( mText );

            mHandlerImages.CurrentPath   = mCurrentPath;

            mHandlerNavigate.CurrentPath = mCurrentPath;
            mHandlerNavigate.History     = mHistory;
            mHandlerNavigate.FindBlock   = ( id ) => mLayout.Find( id );
            mHandlerNavigate.ScrollTo    = ( pos ) => PendingScrollY = pos;
        }

        public MarkdownViewer( GUISkin skin, string path, string content, Func<float> viewWidthProvider ) : this( skin, path, content )
        {
            mViewWidthProvider = viewWidthProvider ?? throw new ArgumentNullException( nameof( viewWidthProvider ) );
        }

        public bool Update() => mHandlerImages.Update();

        public List<List<Rect>> FindMatches( string query ) => mLayout?.FindMatches( query ) ?? new List<List<Rect>>();

        public void ScrollToAnchor( string anchor )
        {
            if( string.IsNullOrEmpty( anchor ) ) return;
            if( !anchor.StartsWith( "#" ) ) anchor = "#" + anchor;

            mAnchorTarget     = anchor.ToLower();
            mAnchorHoldFrames = AnchorHoldFrameCount;
        }

        public void TickAnchorScroll()
        {
            if( mAnchorHoldFrames <= 0 ) return;

            var block = mLayout.Find( mAnchorTarget );
            if( block != null ) PendingScrollY = block.Rect.y;

            if( --mAnchorHoldFrames <= 0 ) mAnchorTarget = null;
        }

        Layout ParseDocument( string source )
        {
            var context  = new Context( mSkin, mHandlerImages, mHandlerNavigate );
            var builder  = new LayoutBuilder( context );
            var renderer = new RendererMarkdown( builder ) { Source = source };

            var pipelineBuilder = new MarkdownPipelineBuilder()
                .UseAutoLinks()
                .UseTaskLists()
                .Use( new CheckboxInlineExtension() )
                .UseEmphasisExtras( Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Strikethrough )
                .UseSoftlineBreakAsHardlineBreak();

            if( !string.IsNullOrEmpty( MarkdownPreferences.JIRA ) )
            {
                pipelineBuilder.UseJiraLinks( new JiraLinkOptions( MarkdownPreferences.JIRA ) );
            }

            if( MarkdownPreferences.PipedTables )
            {
                pipelineBuilder.UsePipeTables( new PipeTableOptions
                {
                    RequireHeaderSeparator = MarkdownPreferences.PipedTablesRequireHeaderSeparator
                } );
            }

            var pipeline = pipelineBuilder.Build();
            pipeline.Setup( renderer );

            var doc = Markdown.Parse( source, pipeline );
            renderer.Render( doc );

            return builder.GetLayout();
        }

        void SaveFile()
        {
            if( mEditedText == null ) return;

            File.WriteAllText( mCurrentPath, mEditedText );
            mText       = mEditedText;
            mEditedText = null;
            AssetDatabase.ImportAsset( mCurrentPath );
        }

        private void ClearBackground( float height )
        {
            var rectFullScreen = new Rect( 0.0f, 0.0f, Screen.width, Mathf.Max( height, Screen.height ) );
            GUI.DrawTexture( rectFullScreen, mSkin.window.normal.background, ScaleMode.StretchToFill, false );
        }

        public void Draw()
        {
            GUI.skin    = mSkin;
            GUI.enabled = true;

            var contentWidth = mViewWidthProvider() - mSkin.verticalScrollbar.fixedWidth - 2.0f * Margin.x;

            if( mRaw )
            {
                // UITK panel covers full window — IMGUI draws background only to prevent artifacts
                ClearBackground( ViewportHeight );
                return;
            }

            ClearBackground( mLayout.Height );
            DrawMarkdown( contentWidth );
            DrawToolbar( contentWidth );
        }

        void DrawToolbar( float contentWidth )
        {
            var style = GUI.skin.button;
            var size  = style.fixedHeight;
            var btn   = new Rect( Margin.x + contentWidth - size, Margin.y, size, size );

            if( Editable && GUI.Button( btn, string.Empty, GUI.skin.GetStyle( mRaw ? "btnRaw" : "btnFile" ) ) )
            {
                if( mRaw && mEditedText != null )
                {
                    mLayout = ParseDocument( mEditedText );
                    SaveFile();
                }

                mRaw = !mRaw;
            }

            if( !mRaw )
            {
                if( mHistory.CanForward )
                {
                    btn.x -= size;

                    if( GUI.Button( btn, string.Empty, GUI.skin.GetStyle( "btnForward" ) ) )
                    {
                        Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>( mHistory.Forward() );
                    }
                }

                if( mHistory.CanBack )
                {
                    btn.x -= size;

                    if( GUI.Button( btn, string.Empty, GUI.skin.GetStyle( "btnBack" ) ) )
                    {
                        Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>( mHistory.Back() );
                    }
                }
            }
        }

        void DrawSearchHighlights()
        {
            if( SearchMatches == null || SearchMatches.Count == 0 ) return;

            var normal  = new Color( 1f, 0.85f, 0f, 0.35f );
            var current = new Color( 1f, 0.55f, 0f, 0.65f );

            for( var i = 0; i < SearchMatches.Count; i++ )
            {
                var color = i == SearchMatchIndex ? current : normal;
                foreach( var r in SearchMatches[ i ] )
                    UnityEditor.EditorGUI.DrawRect( r, color );
            }
        }

        void DrawMarkdown( float width )
        {
            switch( Event.current.type )
            {
                case EventType.Ignore:
                    break;

                case EventType.ContextClick:
                    var menu = new GenericMenu();
                    menu.AddItem( new GUIContent( "View Source" ), false, () => mRaw = !mRaw );
                    menu.ShowAsContext();
                    break;

                case EventType.Layout:
                    mLayout.Arrange( width );
                    GUILayout.Space( mLayout.Height );
                    break;

                default:
                    DrawSearchHighlights();
                    mLayout.Draw();
                    break;
            }
        }
    }
}
