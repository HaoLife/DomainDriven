using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;

namespace Rainbow.DomainDriven.ConsoleApp.Interceptors
{
    public class LoggerInterceptor : IInterceptor
    {
        private readonly ILogger _logger;
        public LoggerInterceptor(ILoggerFactory loggerFactory)
        {
            this._logger = loggerFactory.CreateLogger<LoggerInterceptor>();
        }

        public void Intercept(IInvocation invocation)
        {
            this._logger.LogDebug("handle log Interceptor ");
            invocation.Proceed();

        }
    }
}