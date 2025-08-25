using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Classes
{
    public class MetricsUdpServer : IMetricsServer
    {
        /// <summary>
        ///     Максимальное кол-во буффера приема
        /// </summary>
        private const int BufferSize = 256;

        /// <summary>
        ///     Словарь актуальных метрик имя - значение.
        /// </summary>
        private readonly Dictionary<string, double> _metrics;

        private readonly IMetricsParser _metricsParser;

        private readonly IMetricsValidator _metricsValidator;

        private readonly Thread _reader;
        private readonly int _readerTimeout = 5 * 1000;
        private readonly Thread _writer;
        private IPEndPoint _endPoint;

        private bool _running;
        private ReaderWriterLockSlim _rwLock;
        private UdpClient _udpClient;

        /// <summary>
        ///     Создаёт новый экземпляр UDP сервера.
        /// </summary>
        /// <param name="port">Порт, на котором слушает сервер.</param>
        /// <param name="metricsParser">Парсер метрик.</param>
        /// <param name="metricsValidator">Валидатор метрик.</param>
        /// <param name="onError">Коллбэк для обработки ошибок.</param>
        /// <param name="updatedMetricsHandler">Коллбэк при обновлении метрик.</param>
        /// <param name="endPoint">Необязательный конечный адрес для приёма данных.</param>
        public MetricsUdpServer(
            int port,
            IMetricsParser metricsParser,
            IMetricsValidator metricsValidator,
            ErrorHandler onError,
            UpdatedMetricsHandler updatedMetricsHandler,
            IPEndPoint endPoint = null)
        {
            _udpClient = new UdpClient(port);
            _reader = new Thread(Write);
            _writer = new Thread(UpdateMetrics);
            _metrics = new Dictionary<string, double>();
            _rwLock = new ReaderWriterLockSlim();
            _endPoint = endPoint;
            OnError += onError;
            OnUpdate += updatedMetricsHandler;
            _metricsValidator = metricsValidator; 
            _metricsParser = metricsParser; 
        }

        /// <summary>
        ///     Событие, возникающее при ошибке.
        /// </summary>
        public event ErrorHandler OnError;

        /// <summary>
        ///     Событие, возникающее при обновлении метрик.
        /// </summary>
        public event UpdatedMetricsHandler OnUpdate;

        /// <summary>
        ///     Запускает сервер для прослушивания UDP сокета и сохранения метрик.
        ///     Для остановки используйте метод <see cref="Stop" />.
        /// </summary>
        public void Start()
        {
            _running = true;
            _writer.Start();
            _reader.Start();
        }

        /// <summary>
        ///     Останавливает сервер и очищает ресурсы.
        /// </summary>
        public void Stop()
        {
            _running = false;
            _udpClient.Close();
            _writer.Join();
            _reader.Join();
            _metrics.Clear();
            _rwLock.Dispose();
            _udpClient = null;
            _rwLock = null;
            _endPoint = null;
            OnUpdate = null;
            OnError = null;
        }

        /// <summary>
        ///     С переодичностью <see cref="_readerTimeout" /> для передачи актуальных метрик.
        /// </summary>
        private void UpdateMetrics()
        {
            try
            {
                // Пока сервер запущен, безопасно читает метрики, делает их копию и передаёт её в делегат обновления 
                while (_running)
                {
                    _rwLock.EnterReadLock();

                    try
                    {
                        var updatedMetrics = new Dictionary<string, double>(_metrics);
                        OnUpdate?.Invoke(updatedMetrics);
                    }
                    finally
                    {
                        _rwLock.ExitReadLock();
                    }

                    Thread.Sleep(_readerTimeout);
                }
            }
            catch (Exception e)
            {
                InvokeOtherError(e.Message);
            }
        }

        /// <summary>
        ///     Поток: принимает UDP-пакеты и обновляет словарь метрик.
        /// </summary>
        private void Write()
        {
            // Пока сервер запущен, принимает пакеты, проверяет их корректность, парсит и безопасно сохраняет, если это метрика
            while (_running)
                try
                {
                    var receiveBytes = _udpClient.Receive(ref _endPoint);

                    if (receiveBytes.Length == 0 || receiveBytes.Length > BufferSize) continue;

                    var parsedMetrics = _metricsParser.ParseMetric(receiveBytes);
                    if (string.IsNullOrEmpty(parsedMetrics)) continue;

                    _metricsValidator.ValidateMetric(parsedMetrics, out var validatedMetricName,
                        out var validatedMetricValue);

                    if (string.IsNullOrEmpty(validatedMetricName) || validatedMetricValue == null) continue;

                    _rwLock.EnterWriteLock();
                    try
                    {
                        _metrics[validatedMetricName] = validatedMetricValue.Value;
                    }
                    finally
                    {
                        _rwLock.ExitWriteLock();
                    }
                }
                catch (Exception e)
                {
                    InvokeOtherError(e.Message);
                }
        }

        private void InvokeOtherError(string message)
        {
            var sb = new StringBuilder(Errors.OtherError);
            sb.Append(' ');
            sb.Append(message);
            OnError?.Invoke(sb.ToString());
            sb.Clear();
        }
    }
}