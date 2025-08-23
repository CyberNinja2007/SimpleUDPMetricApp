using System;
using System.Text;

namespace Classes
{
    public class MetricsParser
    {
        /// <summary>
        /// Делегат для обработки ошибок валидации метрик.
        /// </summary>
        /// <param name="error">Сообщение об ошибке.</param>
        public delegate void ErrorHandler(string error);

        /// <summary>
        /// Событие, вызываемое при ошибке валидации метрики.
        /// </summary>
        public event ErrorHandler OnError;
        
        /// <summary>
        /// Создаёт новый экземпляр парсера метрик.
        /// </summary>
        /// <param name="errorHandler">Коллбэк для обработки ошибок.</param>
        public MetricsParser(ErrorHandler errorHandler)
        {
            OnError += errorHandler;
        }

        /// <summary>
        /// Парсит метрику, представленную в формате массива байт
        /// </summary>
        /// <param name="metrics">Метрика</param>
        /// <returns>Распаршенную метрику</returns>
        public string ParseMetric(byte[] metrics)
        {
            try
            {
                string potentialMetric = Encoding.UTF8.GetString(metrics);
                return potentialMetric;
            }
            catch (Exception)
            {
                InvokeParsingError();
                return null;
            }
        }
        
        private void InvokeParsingError() => OnError?.Invoke(Errors.ParseError);
    }

}