using System;

namespace WebCore
{
    /// <summary>
    /// 异步委托的调用函数BeginInvoke，可随手开启线程了。
    /// </summary>
    public class AsyncCallback
    {
        /// <summary>
        /// 开启异步方法，并且在异步结束后，触发回调方法。
        /// </summary>
        /// <param name="action"></param>
        /// <param name="callback"></param>
        /// <param name="object"></param>
        /// <returns></returns>
        public IAsyncResult BeginInvoke(Action action, Action callback = null, object @object = null)
        {
            return action.BeginInvoke(iar =>
            {
                callback?.Invoke();
            }, @object);
        }
        /// <summary>
        /// 开启异步有入参的方法，传递参数input，并且在异步结束后，触发回调方法。
        /// </summary>
        /// <typeparam name="TParam1"></typeparam>
        /// <param name="action"></param>
        /// <param name="param1"></param>
        /// <param name="callback"></param>
        /// <param name="object"></param>
        /// <returns></returns>
        public IAsyncResult BeginInvoke<TParam1>(Action<TParam1> action, TParam1 param1, Action callback = null, object @object = null)
        {
            return action.BeginInvoke(param1, iar =>
            {
                callback?.Invoke();
            }, @object);
        }
        /// <summary>
        /// 开启异步有入参的方法，传递参数input，之后返回结果output，并且在异步结束后，触发回调方法。
        /// </summary>
        /// <typeparam name="TParam1"></typeparam>
        /// <typeparam name="TParam2"></typeparam>
        /// <param name="action"></param>
        /// <param name="param1"></param>
        /// <param name="callback"></param>
        /// <param name="object"></param>
        /// <returns></returns>
        public IAsyncResult BeginInvoke<TParam1, TParam2>(Func<TParam1, TParam2> action, TParam1 param1, Action<TParam2> callback = null, object @object = null)
        {
            return action.BeginInvoke(param1, iar =>
            {
                var param2 = action.EndInvoke(iar);
                callback?.Invoke(param2);
            }, @object);
        }
    }
}
