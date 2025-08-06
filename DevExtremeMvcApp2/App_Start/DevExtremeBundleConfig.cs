using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Optimization;

namespace DevExtremeMvcApp2 {

    public class DevExtremeBundleConfig {

        public static void RegisterBundles(BundleCollection bundles) {

            var styleBundle = new StyleBundle("~/Content/DevExtremeBundle");
            var scriptBundle = new ScriptBundle("~/Scripts/DevExtremeBundle");

            // Adding the UTF-8 charset to display icons correctly
            styleBundle.Include("~/Content/Charset.css");

            // Uncomment to use the Gantt control
            //styleBundle.Include("~/Content/dx-gantt.css");

            // Uncomment to use the Diagram control
            //styleBundle.Include("~/Content/dx-diagram.css");

            // Predefined themes: https://js.devexpress.com/DevExtreme/Guide/Themes_and_Styles/Predefined_Themes/
            styleBundle.Include("~/Content/dx.light.css");

            // Uncomment to use the Gantt control
            //scriptBundle.Include("~/Scripts/dx-gantt.js");

            // Uncomment to use the Diagram control
            //scriptBundle.Include("~/Scripts/dx-diagram.js");

            // NOTE: jQuery may already be included in the default script bundle. Check the BundleConfig.cs file.
            //scriptBundle.Include("~/Scripts/jquery-3.6.3.js");

            // Localization: https://docs.devexpress.com/DevExtremeAspNetMvc/400706

            // Uncomment to enable client-side export
            //scriptBundle.Include("~/Scripts/jszip.js");

            scriptBundle.Include("~/Scripts/dx.all.js");

            // Uncomment to provide geo-data for the VectorMap control
            // Docs: https://js.devexpress.com/DevExtreme/Guide/Widgets/VectorMap/Providing_Data/#Data_for_Areas
            //scriptBundle.Include("~/Scripts/vectormap-data/world.js");

            scriptBundle.Include("~/Scripts/aspnet/dx.aspnet.mvc.js");
            scriptBundle.Include("~/Scripts/aspnet/dx.aspnet.data.js");

            scriptBundle.Transforms.Clear();

            bundles.Add(styleBundle);
            bundles.Add(scriptBundle);
        }
    }
}