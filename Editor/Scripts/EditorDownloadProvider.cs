// Copyright 2020-2022 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace GLTFast.Editor {
    
    using Loading;

    class EditorDownloadProvider : IEditorDownloadProvider
    {

        public List<GltfAssetDependency> assetDependencies
        {
            get => innerAssetDependencies;
            set => innerAssetDependencies = value;
        }

        public List<GltfAssetDependency> innerAssetDependencies = new List<GltfAssetDependency>();
        private readonly GltfAssetDependency[] previousDependencies;

        public EditorDownloadProvider(GltfAssetDependency[] gltfAssetDependencies)
        {
            previousDependencies = gltfAssetDependencies ?? Array.Empty<GltfAssetDependency>();
        }

#pragma warning disable 1998
        public async  Task<IDownload> Request(Uri url) {
            var req = new SyncFileLoader(GetDependencyFromPreviousImport(url, GltfAssetDependency.Type.Buffer));
            return req;
        }

        public async Task<ITextureDownload> RequestTexture(Uri url,bool nonReadable) {
            var req = new SyncTextureLoader(GetDependencyFromPreviousImport(url, GltfAssetDependency.Type.Texture),nonReadable);
            return req;
        }

#pragma warning restore 1998

        Uri MakePathProjectRelative(Uri uri) {
            var projectPath = new Uri(Path.GetDirectoryName(Application.dataPath));
            return uri.MakeRelativeUri(projectPath);
        }

        private Uri GetDependencyFromPreviousImport(Uri url, GltfAssetDependency.Type type)
        {
            var previousDependency = previousDependencies.FirstOrDefault(d => d.originalUri == url.OriginalString);

            if (previousDependency.type == GltfAssetDependency.Type.Unknown)
            {
                var newDependency = new GltfAssetDependency
                {
                    originalUri = url.OriginalString,
                    type = type,
                };
                innerAssetDependencies.Add(newDependency);
                return new Uri(newDependency.originalUri, UriKind.Relative);
            }
            
            innerAssetDependencies.Add(previousDependency);
            return new Uri(previousDependency.assetPath, UriKind.Relative);
        }
    }

    class SyncFileLoader : IDownload {
        public SyncFileLoader(Uri url) {
            var path = url.OriginalString;
            if (File.Exists(path)) {
                data = File.ReadAllBytes(path);
            }
            else {
                error = $"Cannot find resource at path {path}";
            }
        }
        
        public object Current => null;
        public bool MoveNext() { return false; }
        public void Reset() {}
        
        public virtual bool success => data!=null;

        public string error { get; protected set; }
        public byte[] data { get; private set; }

        public string text => System.Text.Encoding.UTF8.GetString(data);

        public bool? isBinary {
            get {
                if (success) {
                    return GltfGlobals.IsGltfBinary(data);
                }
                return null;
            }
        }

        public virtual void Dispose() {
            data = null;
        }
    }
    
    class SyncTextureLoader : SyncFileLoader, ITextureDownload {
        
        public Texture2D texture { get; private set; }

        public override bool success => texture!=null;
        
        public SyncTextureLoader(Uri url, bool nonReadable)
            : base(url) {
            texture = AssetDatabase.LoadAssetAtPath<Texture2D>(url.OriginalString);
            if (texture == null) {
                error = $"Couldn't load texture at {url.OriginalString}";
            }
        }

        public override void Dispose() {
            base.Dispose();
            texture = null;
        }
    }
}
