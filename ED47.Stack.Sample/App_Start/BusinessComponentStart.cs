using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ED47.BusinessAccessLayer;
using ED47.Stack.Web;

[assembly: WebActivator.PreApplicationStartMethod(typeof(ED47.Stack.Sample.App_Start.BusinessComponentStart), "Start")]
[assembly: WebActivator.PostApplicationStartMethod(typeof(ED47.Stack.Sample.App_Start.BusinessComponentStart), "AfterStart")]
namespace ED47.Stack.Sample.App_Start
{
    public class BusinessComponentStart
    {
       public static void Start()
       {
           //Register the file repository to use LocalFileRepository.
           BusinessComponent.Kernel.Bind<IFileRepository>().To<LocalFileRepository>();
           //BusinessComponent.Kernel.Bind<BaseUserContext>().To<YOUCOMPONENT.UserContext>(); //Defines default context for operations that are directly handled by the BusinessAccessLayer
           
           LocalFileRepository.LocalFileRepositoryPath = ConfigurationManager.AppSettings["LocalFileRepository.LocalFileRepositoryPath"];
           //TODO: Register your business components here.
           //BusinessComponent.RegisterComponent(YOURCOMPONENT.Current);
           BusinessComponent.StartAll();

           //Defines the user context the SharedUserContext will use
           //SharedUserContext.CreateDefaultContext = () => new YOURCOMPONENT.UserContext();
           
       }

        public static void AfterStart()
        {
            //Register's templates.
            Template.RegisterDirectory(HttpContext.Current.Server.MapPath("app_data") + "\\Templates\\", "*.html");
        }
    }

    
}
