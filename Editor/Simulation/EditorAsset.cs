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
using System.IO;
using UnityEngine;
using UnityEditor;

namespace HooAsset.Editor.Simulation
{
    /// <summary>
    /// 编辑器下资源加载
    /// </summary>
    public class EditorAsset : Asset
    {
        /// <summary>
        /// 依赖
        /// </summary>
        EditorDependency _dependency;

        /// <summary>
        /// 创建EditorAsset
        /// </summary>
        internal static EditorAsset Create(string assetPath, Type type)
        {
            if (!File.Exists(assetPath))
            {
                Debug.LogError($"资源不存在{assetPath}");
                return null;
            }

            return new EditorAsset { address = assetPath, type = type };
        }

        protected override void OnLoad()
        {
            _dependency = new EditorDependency { address = address };
            _dependency.Load();
        }

        protected override void OnLoadImmediately()
        {
            _dependency.LoadImmediately();
            OnAssetLoaded(AssetDatabase.LoadAssetAtPath(address, type));
        }

        protected override void OnUpdate()
        {
            if (Status == LoadableStatus.Loading && _dependency.IsDone)
                OnAssetLoaded(AssetDatabase.LoadAssetAtPath(address, type));
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            _dependency.Release();
            _dependency = null;

            if (!result)
                return;

            // Resources.UnloadAsset仅能释放非GameObject和Component的资源，比如Texture、Mesh等真正的资源。对于由Prefab加载出来的Object或Component，则不能通过该函数来进行释放。
            // UnloadAsset may only be used on individual assets and can not be used on GameObject's/Components or AssetBundles
            if (!EditorAssetReference.IsUsing(address) && result is not GameObject)
                Resources.UnloadAsset(result);

            result = null;
        }
    }
}
