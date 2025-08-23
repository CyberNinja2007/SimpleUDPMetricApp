using System;
using System.Text;

namespace Classes
{
    public class MetricsValidator
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
        /// Создаёт новый экземпляр валидатора метрик.
        /// </summary>
        /// <param name="errorHandler">Коллбэк для обработки ошибок.</param>
        public MetricsValidator(ErrorHandler errorHandler)
        {
            OnError += errorHandler;
        }
        
        /// <summary>
        /// Выполняет проверку строки метрики в формате "name:value".
        /// </summary>
        /// <param name="metric">Строка метрики.</param>
        /// <param name="metricName">Имя метрики, если успешно прошло проверку.</param>
        /// <param name="metricValue">Значение метрики, если успешно прошло проверку.</param>
        public void ValidateMetric(string metric, out string metricName, out double? metricValue)
        {
            string[] validatedMetrics = metric.Split(':');

            metricName = null;
            metricValue = null;
            
            if (validatedMetrics.Length != 2 || NullChecker.IsNull(validatedMetrics))
            {
                InvokeValidationError(metric);
                return;
            }
            
            string validatedMetricName = ValidateMetricName(validatedMetrics[0]);
            if (NullChecker.IsNull(validatedMetricName))
            {
                InvokeValidationError(metric);
                return;
            }

            double? validatedMetricValue = ValidateMetricValue(validatedMetrics[1]);
            if (NullChecker.IsNull(validatedMetricValue))
            {
                InvokeValidationError(metric);
                return;
            }
            
            metricValue = validatedMetricValue;
            metricName = validatedMetricName;
        }

        private string ValidateMetricName(string metricName)
        {
            if (String.IsNullOrEmpty(metricName))
            {
                return null;
            }
            
            return metricName;
        }

        private double? ValidateMetricValue(string metricValue)
        {
            if (String.IsNullOrEmpty(metricValue))
            {
                return null;
            }

            try
            {
                return Double.Parse(metricValue);
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        private void InvokeValidationError(string metric)
        {
            StringBuilder sb = new StringBuilder(Errors.ValidationError);
            sb.Append(" ");
            sb.Append(metric);
            OnError?.Invoke(sb.ToString());
            sb.Clear();
        }
    }
}