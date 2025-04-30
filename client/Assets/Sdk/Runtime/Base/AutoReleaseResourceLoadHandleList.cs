using System;
using System.Collections.Generic;

namespace Sdk.Runtime.Base
{
    /// <summary>
    /// 自动释放给定的资源列表
    /// </summary>
    /// <typeparam name="TElement">资源类型</typeparam>
    public class AutoReleaseResourceLoadHandleList<TElement> : IDisposable
    {
        private readonly List<AsyncResourceLoadHandle<TElement>> _list;

        public AutoReleaseResourceLoadHandleList(List<AsyncResourceLoadHandle<TElement>> list)
        {
            _list = list;
        }

        public void Dispose()
        {
            if (_list == null)
                return;
            foreach (var item in _list)
                item.Release();
        }
    }
}