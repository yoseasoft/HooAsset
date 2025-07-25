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
using System.Linq;
using System.Collections.Generic;

namespace HooAsset
{
    /// <summary>
    /// Asset对象管理
    /// </summary>
    public static class AssetHandler
    {
        /// <summary>
        /// 缓存队列
        /// </summary>
        public static readonly Dictionary<string, Asset> Cache = new();

        /// <summary>
        /// 获取创建资源类方法, 可进行自定义覆盖, 默认为DefaultCreateAssetFunc
        /// </summary>
        public static Func<string, Type, Asset> CreateAssetFunc { get; set; } = DefaultCreateAssetFunc;

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="address">资源地址</param>
        /// <param name="type">资源类型</param>
        internal static Asset Load(string address, Type type)
        {
            Asset asset = LoadAsync(address, type);
            asset?.LoadImmediately();
            return asset;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="address">资源地址</param>
        /// <param name="type">资源类型</param>
        /// <param name="completed">完成回调</param>
        internal static Asset LoadAsync(string address, Type type, Action<Asset> completed = null)
        {
            string path = AssetPath.GetActualPath(address);
            if (!ManifestHandler.IsAssetContains(path))
            {
                Logger.Error($"资源加载失败, 因所有资源清单中都没有此资源的记录:{path}");
                return null;
            }

            if (!Cache.TryGetValue(path, out var asset))
            {
                asset = CreateAssetFunc(path, type);
                Cache.Add(path, asset);
            }

            if (completed != null)
                asset.completed += completed;

            asset.Load();
            return asset;
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        internal static void RemoveCache(string path)
        {
            Cache.Remove(path);
        }

        /// <summary>
        /// 清除缓存并全部卸载
        /// </summary>
        internal static void ClearCache()
        {
            Asset[] assets = Cache.Values.ToArray();
            foreach (Asset asset in assets)
            {
                asset.FullyRelease();
                asset.Unload();
            }

            Cache.Clear();
        }

        /// <summary>
        /// 创建ab包资源对象
        /// </summary>
        static BundledAsset DefaultCreateAssetFunc(string assetPath, Type type)
        {
            return new BundledAsset { address = assetPath, type = type };
        }
    }
}
