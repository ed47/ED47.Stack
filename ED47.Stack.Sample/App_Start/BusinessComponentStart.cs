using System.Configuration;
using System.Web;
using ED47.BusinessAccessLayer;
using ED47.Stack.Sample.Domain;
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
           
           LocalFileRepository.LocalFileRepositoryPath = ConfigurationManager.AppSettings["LocalFileRepository.LocalFileRepositoryPath"];
           //TODO: Register your business components here.
           BusinessComponent.RegisterComponent(SampleDomainBusinessComponent.Current);
           BusinessComponent.StartAll();

           //Defines the user context the shared UserContext will use
           UserContext.CreateDefaultContext = () => new Sample.Domain.UserContext();
       }

        public static void AfterStart()
        {
            //Register's templates.
            Template.RegisterDirectory(HttpContext.Current.Server.MapPath("app_data") + "\\Templates\\", "*.html");
            Template.RegisterDirectory(HttpContext.Current.Server.MapPath("app_data") + "\\Templates\\", "*.chtml");
        }
    }
}
