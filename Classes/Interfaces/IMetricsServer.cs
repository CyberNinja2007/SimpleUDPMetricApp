using System;
using System.Collections.Generic;

namespace Classes
{
    
    /// <summary>
    ///     Делегат для обработки ошибок.
    /// </summary>
    public delegate void ErrorHandler(string error);

    /// <summary>
    ///     Делегат для обновления метрик.
    /// </summary>
    public delegate void UpdatedMetricsHandler(Dictionary<string, double> metrics);
    
    public interface IMetricsServer
    {
        event ErrorHandler OnError;
        event UpdatedMetricsHandler OnUpdate;

        void Start();
        void Stop();
    }
}