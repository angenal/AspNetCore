using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebInterface
{
    /// <summary>轻量级生产者消费者接口</summary>
    /// <remarks>
    /// 不支持Ack机制；也不支持消息体与消息键分离
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public interface IProducerConsumer<T>
    {
        /// <summary>元素个数</summary>
        int Count { get; }

        /// <summary>集合是否为空</summary>
        bool IsEmpty { get; }

        /// <summary>生产添加</summary>
        /// <param name="values"></param>
        /// <returns></returns>
        int Add(params T[] values);

        /// <summary>消费获取一批</summary>
        /// <param name="count"></param>
        /// <returns></returns>
        IEnumerable<T> Take(int count = 1);

        /// <summary>消费获取一个</summary>
        /// <param name="timeout">超时。默认0秒，永久等待</param>
        /// <returns></returns>
        T TakeOne(int timeout = 0);

        /// <summary>异步消费获取一个</summary>
        /// <param name="timeout">超时。默认0秒，永久等待</param>
        /// <returns></returns>
        Task<T> TakeOneAsync(int timeout = 0);

        /// <summary>确认消费</summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        int Acknowledge(params string[] keys);
    }
}
