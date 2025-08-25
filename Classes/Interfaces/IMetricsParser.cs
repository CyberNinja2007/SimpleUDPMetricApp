namespace Classes
{
    public interface IMetricsParser
    {
       event ErrorHandler OnError;
       
       string ParseMetric(byte[] metrics);
    }
}