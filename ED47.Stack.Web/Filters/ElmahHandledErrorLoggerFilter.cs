using System;
using System.Web.Http.Filters;
using Elmah;

namespace ED47.Stack.Web.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ElmahHandledErrorLoggerFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnException(actionExecutedContext);

            ErrorSignal.FromCurrentContext().Raise(actionExecutedContext.Exception);
        }
    }
}
