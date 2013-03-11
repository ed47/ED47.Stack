using System.Web.Optimization;
using ED47.Stack.Sample.App_Start;

[assembly: WebActivator.PostApplicationStartMethod(typeof(BundleConfig), "Start")]
namespace ED47.Stack.Sample.App_Start
{
    public class BundleConfig
    {
        public static void Start()
        {
            BundleTable.Bundles.Add(new ScriptBundle("~/jsBundle")
                .Include(
                    "~/Scripts/jquery-*",
                    "~/Scripts/jquery.*",
                    "~/Scripts/modernizr-*"
                    //TODO: Add your general scripts here
                )
            );

            BundleTable.Bundles.Add(new StyleBundle("~/cssBundle")
                .Include(
                    "~/Content/Styles/html5reset*"
                    //TODO: Add your styles here
               )
            );
            
            /*
             * Add here your ExtJS-based per page JS bundles
             * 
            BundleTable.Bundles.Add(new ScriptBundle("~/jsXXXX")
                .IncludeDirectory("~/js/YYYY/views/XXXX", "*.js"));*/
            
#if !DEBUG
            //Forces optimization for testing purposes
            BundleTable.EnableOptimizations = true;
#endif
        }
    }
}