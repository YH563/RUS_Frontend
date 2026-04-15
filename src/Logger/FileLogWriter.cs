using Godot;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SRC.Logger
{
    /// <summary>
    /// 负责将日志信息异步写入文件
    /// </summary>
    public class FileLogWritter : IDisposable
    {
        private readonly string _filePath;  // 日志文件路径
        private readonly BlockingCollection<string> _messageQueue = new BlockingCollection<string>();  // 消息队列
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();  // 用于取消后台任务
        private readonly Task _writerTask;  // 写入任务
        private bool _disposed = false;

        public FileLogWritter(string filePath)
        {
            _filePath = filePath;
            // 确保目录存在
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            // 启动后台写入任务
            _writerTask = Task.Run(WriteLoop);
        }

        /// <summary>
        /// 循环写入日志信息
        /// </summary>
        private void WriteLoop()
        {
            using (var writer = new StreamWriter(_filePath, append: true, encoding: Encoding.UTF8))
            {
                foreach (var message in _messageQueue.GetConsumingEnumerable(_cts.Token))
                {
                    try
                    {
                        writer.WriteLine(message);
                        writer.Flush(); // 确保立即写入，防止程序崩溃时丢失
                    }
                    catch (Exception ex)
                    {
                        GD.PrintErr($"异步日志写入失败: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 添加一条日志信息
        /// </summary>
        /// <param name="message">日志信息</param>
        public void WriteLine(string message)
        {
            if (_disposed) return;
            _messageQueue.Add(message);
        }

        /// <summary>
        /// 关闭日志写入
        /// </summary>
        public void Close()
        {
            if (_disposed) return;
            _disposed = true;
            _messageQueue.CompleteAdding(); // 停止接受新消息
            try
            {
                _writerTask.Wait(5000); // 最多等待5秒
            }
            catch (AggregateException) { /* 忽略超时 */ }
            _cts.Cancel();
        }

        public void Dispose()
        {
            Close();
        }
    }
}