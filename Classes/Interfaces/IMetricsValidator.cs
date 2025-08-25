namespace Classes
{
    public interface IMetricsValidator
    {
        event ErrorHandler OnError;
        
        void ValidateMetric(string metric, out string metricName, out double? metricValue);
    }
}