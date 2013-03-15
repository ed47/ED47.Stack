using System.Configuration;
using System.Web;
using ED47.BusinessAccessLayer;
using ED47.Communicator.Admin;
using ED47.Communicator.Web.App_Start;
using ED47.Stack.Web.Template;
using UserContext = ED47.Communicator.Shared.UserContext;

[assembly: WebActivator.PreApplicationStartMethod(typeof(BusinessComponentStart), "Start")]
[assembly: WebActivator.PostApplicationStartMethod(typeof(BusinessComponentStart), "AfterStart")]
namespace ED47.Communicator.Web.App_Start
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
           UserContext.CreateDefaultContext = () => new Communicator.Admin.UserContext();
       }

        public static void AfterStart()
        {
            //Register's templates.
            Template.RegisterDirectory(HttpContext.Current.Server.MapPath("app_data") + "\\Templates\\", "*.html");
            Template.RegisterDirectory(HttpContext.Current.Server.MapPath("app_data") + "\\Templates\\", "*.chtml");
        }
    }
}
