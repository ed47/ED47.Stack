using System;
using System.Web.Http.Filters;
using Elmah;

namespace ED47.Stack.Web.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    [Obsolete("No longuer required thanks to Elmah.Mvc")]
    public sealed class ElmahHandledErrorLoggerFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnException(actionExecutedContext);

            ErrorSignal.FromCurrentContext().Raise(actionExecutedContext.Exception);
        }
    }
}
