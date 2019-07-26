﻿using DashboardDemo.Web.Bundles;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.Widgets;

namespace DashboardDemo.Web.Pages.Components.LicenseStatisticWidget
{
    [Widget(
        StyleTypes = new[] { typeof(ChartjsStyleContributor) },
        ScriptTypes = new[] { typeof(LicenseStatisticWidgetScriptContributor) }
        )]
    public class LicenseStatisticWidgetViewComponent : AbpViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
