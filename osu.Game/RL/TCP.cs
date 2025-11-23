using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using osu.Framework.Logging;
using osuTK;

namespace osu.Game.RL
{
    /// <summary>
    /// 通用TCP数据发送器，支持异步发送任意对象（自动序列化为JSON）
    /// </summary>
    public class TcpDataSender : IDisposable
    {
        // JSON序列化配置
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false, // 不格式化JSON，减小体积
            IgnoreNullValues = true, // 忽略空值
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase // 驼峰命名（可选，便于前端解析）
        };

        /// <summary>
        /// 构造TCP数据发送器
        /// </summary>
        /// <param name="serverIp">TCP服务器IP（默认本地127.0.0.1）</param>
        /// <param name="port">TCP服务器端口</param>
        public TcpDataSender(string serverIp = "127.0.0.1", int port = 12345)
        {
            _serverIp = serverIp;
            _port = port;
        }

        /// <summary>
        /// 异步发送任意对象到TCP服务器（自动序列化为JSON）
        /// </summary>
        /// <param name="data">要发送的对象（支持任意可序列化类型）</param>
        public async Task SendAsync(object data)
        {
            if (data == null)
            {
                // Logger.Log("TCP发送失败：数据为null", LoggingTarget.Network, LogLevel.Warning);
                return;
            }

            // 确保TCP连接已建立
            if (!await EnsureConnectionAsync())
                return;

            try
            {
                // 序列化对象为JSON字符串
                string json = JsonSerializer.Serialize(data, _jsonOptions);

                // 转换为UTF-8字节（添加换行符作为数据分隔符）
                byte[] sendData = Encoding.UTF8.GetBytes(json + "\n");

                // 异步发送数据
                lock (_tcpLock)
                {
                    if (_networkStream == null || !_networkStream.CanWrite)
                        throw new InvalidOperationException("网络流不可写");
                }

                await _networkStream.WriteAsync(sendData, 0, sendData.Length);
                await _networkStream.FlushAsync();

                // 调试日志（可选开启）
                // Logger.Log($"TCP发送成功：{json}", LoggingTarget.Network, LogLevel.Debug);
            }
            catch (Exception ex)
            {
                Logger.Log($"TCP发送失败：{ex.Message}", LoggingTarget.Network, LogLevel.Error);
                CloseConnection(); // 发送失败时关闭连接，下次自动重连
            }
        }

        /// <summary>
        /// 确保TCP连接已建立（未连接则自动尝试连接）
        /// </summary>
        /// <returns>连接是否成功</returns>
        private async Task<bool> EnsureConnectionAsync()
        {
            lock (_tcpLock)
            {
                // 正在连接中，直接返回false
                if (_isConnecting)
                    return false;

                // 已连接则返回true
                if (_tcpClient != null && _tcpClient.Connected)
                    return true;

                // 开始连接
                _isConnecting = true;
            }

            try
            {
                // 关闭现有无效连接
                CloseConnection();

                // 异步连接服务器（2秒超时）
                _tcpClient = new TcpClient();
                var connectTask = _tcpClient.ConnectAsync(_serverIp, _port);
                var connectSuccess = await Task.WhenAny(connectTask, Task.Delay(2000)) == connectTask;

                if (connectSuccess && _tcpClient.Connected)
                {
                    _networkStream = _tcpClient.GetStream();
                    _networkStream.ReadTimeout = 5000; // 读取超时5秒
                    // Logger.Log($"TCP连接成功：{_serverIp}:{_port}", LoggingTarget.Network, LogLevel.Info);
                    return true;
                }

                Logger.Log($"TCP连接失败：超时或无法连接到 {_serverIp}:{_port}", LoggingTarget.Network, LogLevel.Error);
                CloseConnection();
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log($"TCP连接异常：{ex.Message}", LoggingTarget.Network, LogLevel.Error);
                CloseConnection();
                return false;
            }
            finally
            {
                lock (_tcpLock)
                {
                    _isConnecting = false;
                }
            }
        }

        /// <summary>
        /// 关闭TCP连接并释放资源
        /// </summary>
        public void CloseConnection()
        {
            lock (_tcpLock)
            {
                try
                {
                    _networkStream?.Dispose();
                    _tcpClient?.Close();
                }
                catch (Exception ex)
                {
                    Logger.Log($"关闭TCP连接失败：{ex.Message}", LoggingTarget.Network, LogLevel.Error);
                }
                finally
                {
                    _networkStream = null;
                    _tcpClient = null;
                }
            }
        }

        /// <summary>
        /// 释放资源（自动关闭连接）
        /// </summary>
        public void Dispose()
        {
            CloseConnection();
            GC.SuppressFinalize(this);
        }

        // TCP配置
        private readonly string _serverIp;
        private readonly int _port;
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private bool _isConnecting;
        private readonly object _tcpLock = new object();
    }
}
public class RLData
{
    public Vector2? mouse;
    public Vector2?nextHit;
}
