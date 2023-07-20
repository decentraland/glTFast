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

using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GLTFast.Loading
{

    /// <summary>
    /// Used for wrapping <see cref="UniTask{IDownload}"/>
    /// </summary>
    abstract class TextureDownloadBase
    {
        public IDownload Download { get; protected set; }

        /// <summary>
        /// Executes the texture loading process and assigns the result to
        /// <see cref="Download"/>.
        /// </summary>
        /// <returns></returns>
        public abstract UniTask Load();
    }

    /// <summary>
    /// Used for wrapping <see cref="UniTask{IDownload}"/>
    /// </summary>
    class TextureDownload<T> : TextureDownloadBase where T : IDownload
    {
        UniTask<T> m_UniTask;

        public TextureDownload(UniTask<T> UniTask)
        {
            m_UniTask = UniTask;
        }

        public override async UniTask Load()
        {
            Download = await m_UniTask;
        }
    }
}
