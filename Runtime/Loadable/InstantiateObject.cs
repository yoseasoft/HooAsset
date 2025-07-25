/// -------------------------------------------------------------------------------
/// HooAsset Framework
///
/// Copyright (C) 2020 - 2022, Guangzhou Xinyuan Technology Co., Ltd.
/// Copyright (C) 2022 - 2023, Shanghai Bilibili Technology Co., Ltd.
/// Copyright (C) 2023 - 2024, Guangzhou Shiyue Network Technology Co., Ltd.
///
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
///
/// The above copyright notice and this permission notice shall be included in
/// all copies or substantial portions of the Software.
///
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
/// THE SOFTWARE.
/// -------------------------------------------------------------------------------

using System;
using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;

namespace HooAsset
{
    /// <summary>
    /// 实例化对象加载
    /// </summary>
    public sealed class InstantiateObject : Loadable, IEnumerator
    {
        /// <summary>
        /// 依赖的资源
        /// </summary>
        Asset _asset;

        /// <summary>
        /// 游戏对象
        /// </summary>
        public GameObject gameObject;

        /// <summary>
        /// 实例化完成回调
        /// </summary>
        public Action<InstantiateObject> completed;

        protected override void OnLoad()
        {
            _asset = AssetHandler.LoadAsync(address, typeof(GameObject));
            if (_asset != null)
                Status = LoadableStatus.Loading;
            else
                Finish("资源清单中找不到实例化对象所依赖的资源文件");
        }

        /// <summary>
        /// 依赖的资源加载完成
        /// </summary>
        void OnAssetLoaded()
        {
            if (_asset.HasError)
            {
                Finish($"实例化对象所依赖的资源加载失败, {_asset.Error}");
                return;
            }

            gameObject = Object.Instantiate(_asset.result) as GameObject;
            Finish();
        }

        protected override void OnLoadImmediately()
        {
            if (_asset == null)
                return;

            _asset.LoadImmediately();
            OnAssetLoaded();
        }

        protected override void OnUpdate()
        {
            if (Status != LoadableStatus.Loading)
                return;

            Progress = _asset.Progress;

            if (_asset.IsDone)
                OnAssetLoaded();
        }

        protected override void OnComplete()
        {
            if (completed == null)
                return;

            var func = completed;
            completed = null;
            func.Invoke(this);
        }

        /// <summary>
        /// 销毁实例化对象
        /// </summary>
        public void Destroy()
        {
            if (!IsDone)
            {
                Logger.Info($"InstantiateObject({address}), 未加载完成已被销毁");
                Finish();
                Status = LoadableStatus.LoadFailed;
                return;
            }

            if (gameObject)
                Object.DestroyImmediate(gameObject);

            Release();
        }

        protected override void OnUnused()
        {
            completed = null;
        }

        protected override void OnUnload()
        {
            _asset?.Release();
            _asset = null;
        }

        #region IEnumerator

        public object Current => null;

        public bool MoveNext()
        {
            return !IsDone;
        }

        public void Reset()
        {
        }

        #endregion
    }
}
