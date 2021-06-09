using Castle.DynamicProxy;
using Serilog;
using System;
using System.ServiceModel;
using System.Transactions;

namespace GeneralInsurance.DataAccess.Middleware.Abstractions
{
    public class WcfMethodInterceptor<T> : IInterceptor where T : class
    {
        private readonly T _channel;

        private static ILogger Logger => Logger.ForContext<WcfMethodInterceptor<T>>();

        public WcfMethodInterceptor(T channel)
        {
            _channel = channel;
        }

        public void Intercept(IInvocation invocation)
        {
            try
            {
                Logger.Debug("Calling service method {Name} on WCF channel. ", invocation.Method.Name);
                invocation.ReturnValue = invocation.Method.Invoke(_channel, invocation.Arguments);
            }
            catch (Exception e)
            {
                // the real WCF exception is wrapped in the TargetInvocationException, so throw the inner WCF fault exception to make this interceptor completely transparent to the apps exception handling
                // logic
                Logger.Error(Exception, "Error occured calling middleware");
                throw Exception.InnerException;
            }
            finally{
                CloseChannel(invocation.Method.Name);
            }
        }

        protected internal void CloseChannel(string invocationMethodName)
        {
            if (!(_channel is ICommunicationObject communicationObject))
            {
                Logger.Warning(
                    "Cannot close WCF channel for method {invocationMethodName}() as it was null or not an ICommunicationObject.",
                    invocationMethodName);
                return;
            }

            try
            {
                Logger.Debug("Calling Close() on WCF channel for method {invocationMethodName}().",
                    invocationMethodName);
                communicationObject.Close();
                Logger.Debug("WCF channel for method {invocationMethodName}() has been closed successfully.",
                    invocationMethodName);
            }
            catch (CommunicationException ex)
            {
                Logger.Error(ex, "Communication exception occured for method ");
                communicationObject.Abort();
                throw;
            }
            catch (TimeoutException ex)
            {
                Logger.Error(ex, "Timeout occured for method {invocationMethodName}()", invocationMethodName);
                communicationObject.Abort();
            }
            catch (Exception ex)
            {
                Logger.Error(ex,
                    "Attempt to close WCF channel for method {@invocationMethodName}(), but an unexpected exception was thrown. Calling Abort() on WCF channel and throwing exception.",
                    invocationMethodName);
                communicationObject.Abort();
                throw;
            }
        }

        
    }
}