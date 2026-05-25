using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace YGDR.MDV
{
    public class MarkdownHandlerImages
    {
        public string CurrentPath;

        Texture                    mPlaceholder      = null;
        List<ImageRequest>         mActiveRequests   = new List<ImageRequest>();
        Dictionary<string,Texture> mTextureCache     = new Dictionary<string, Texture>();
        List<AnimatedTexture>      mAnimatedTextures = new List<AnimatedTexture>();

        class AnimatedTexture
        {
            public string          URL          = string.Empty;
            public int             CurrentFrame = 0;
            public double          FrameTime    = 0.0f;
            public List<Texture2D> Textures     = new List<Texture2D>();
            public List<float>     Times        = new List<float>();

            public AnimatedTexture( string url )
            {
                URL       = url;
                FrameTime = EditorApplication.timeSinceStartup;
            }

            public void Add( Texture2D tex, float delay )
            {
                Textures.Add( tex );
                Times.Add( delay );
            }

            public bool Update()
            {
                var span = EditorApplication.timeSinceStartup - FrameTime;

                if( span < Times[ CurrentFrame ] ) return false;

                FrameTime    = EditorApplication.timeSinceStartup;
                CurrentFrame = ( CurrentFrame + 1 ) % Textures.Count;

                return true;
            }
        }

        class ImageRequest
        {
            public string          URL;
            public UnityWebRequest Request;
            public bool            IsGif;

            public ImageRequest( string url )
            {
                URL = url;

                if( url.EndsWith( ".gif", StringComparison.OrdinalIgnoreCase ) )
                {
                    IsGif   = true;
                    Request = UnityWebRequest.Get( url );
                }
                else
                {
                    IsGif   = false;
                    Request = UnityWebRequestTexture.GetTexture( url );
                }

                Request.SendWebRequest();
            }

            public AnimatedTexture GetAnimatedTexture()
            {
                var decoder = new YGDR.GIF.Decoder( Request.downloadHandler.data );
                var img     = decoder.NextImage();
                var anim    = new AnimatedTexture( URL );

                while( img != null )
                {
                    anim.Add( img.CreateTexture(), img.Delay / 1000.0f );
                    img = decoder.NextImage();
                }

                return anim;
            }

            public Texture GetTexture()
            {
                var handler = Request.downloadHandler as DownloadHandlerTexture;
                return handler != null ? handler.texture : null;
            }
        }

        private string RemapURL( string url )
        {
            if( Regex.IsMatch( url, @"^\w+:", RegexOptions.Singleline ) ) return url;

            var projectDir = Path.GetDirectoryName( Application.dataPath );

            if( url.StartsWith( "/" ) )
                return string.Format( "file:///{0}{1}", projectDir, url );

            var assetDir = Path.GetDirectoryName( Path.GetFullPath( CurrentPath ) );
            return "file:///" + MarkdownUtils.PathNormalise( string.Format( "{0}/{1}", assetDir, url ) );
        }

        public Texture FetchImage( string url )
        {
            url = RemapURL( url );

            if( mTextureCache.TryGetValue( url, out var tex ) ) return tex;

            if( mPlaceholder == null )
            {
                var style = GUI.skin.GetStyle( "btnPlaceholder" );
                mPlaceholder = style != null ? style.normal.background : null;
            }

            mActiveRequests.Add( new ImageRequest( url ) );
            mTextureCache[ url ] = mPlaceholder;

            return mPlaceholder;
        }

        public bool UpdateRequests()
        {
            var req = mActiveRequests.Find( r => r.Request.isDone );

            if( req == null ) return false;

            if( req.Request.result == UnityWebRequest.Result.ProtocolError )
            {
                Debug.LogError( string.Format( "HTTP Error: {0} - {1} {2}", req.URL, req.Request.responseCode, req.Request.error ) );
                mTextureCache[ req.URL ] = null;
            }
            else if( req.Request.result == UnityWebRequest.Result.ConnectionError )
            {
                Debug.LogError( string.Format( "Network Error: {0} - {1}", req.URL, req.Request.error ) );
                mTextureCache[ req.URL ] = null;
            }
            else if( req.IsGif )
            {
                var anim = req.GetAnimatedTexture();

                if( anim != null && anim.Textures.Count > 0 )
                {
                    mTextureCache[ req.URL ] = anim.Textures[ 0 ];

                    if( anim.Textures.Count > 1 )
                        mAnimatedTextures.Add( anim );
                }
            }
            else
            {
                mTextureCache[ req.URL ] = req.GetTexture();
            }

            mActiveRequests.Remove( req );
            return true;
        }

        public bool UpdateAnimations()
        {
            var updated = false;

            foreach( var anim in mAnimatedTextures )
            {
                if( anim.Update() )
                {
                    mTextureCache[ anim.URL ] = anim.Textures[ anim.CurrentFrame ];
                    updated = true;
                }
            }

            return updated;
        }

        public bool Update() => UpdateRequests() || UpdateAnimations();
    }
}
