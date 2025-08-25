using System;
using System.Text;

namespace Classes
{
    public class MetricsParser  : IMetricsParser
    {
        /// <summary>
        ///     Создаёт новый экземпляр парсера метрик.
        /// </summary>
        /// <param name="errorHandler">Коллбэк для обработки ошибок.</param>
        public MetricsParser(ErrorHandler errorHandler)
        {
            OnError += errorHandler;
        }

        /// <summary>
        ///     Событие, вызываемое при ошибке валидации метрики.
        /// </summary>
        public event ErrorHandler OnError;

        /// <summary>
        ///     Парсит метрику, представленную в формате массива байт
        /// </summary>
        /// <param name="metrics">Метрика</param>
        /// <returns>Распаршенную метрику</returns>
        public string ParseMetric(byte[] metrics)
        {
            try
            {
                var potentialMetric = Encoding.UTF8.GetString(metrics);
                return potentialMetric;
            }
            catch (Exception)
            {
                InvokeParsingError();
                return null;
            }
        }

        private void InvokeParsingError()
        {
            OnError?.Invoke(Errors.ParseError);
        }
    }
}