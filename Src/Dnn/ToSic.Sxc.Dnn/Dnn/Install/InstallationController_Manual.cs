﻿using System;
using System.Linq;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Controllers;
using ToSic.Eav.Run;
using ToSic.Sxc.Apps;
using ToSic.Sxc.Dnn.Run;
using Assembly = System.Reflection.Assembly;

namespace ToSic.Sxc.Dnn.Install
{
    public partial class InstallationController
    {

        public bool ResumeAbortedUpgrade()
        {
            var callLog = Log.Call<bool>();
            if (IsUpgradeRunning)
            {
                Log.Add("Upgrade is still running");
                throw new Exception("There seems to be an upgrade running - please wait. If you still see this message after 10 minutes, please restart the web application.");
            }

            _installLogger.LogStep("", "FinishAbortedUpgrade starting", false);
            _installLogger.LogStep("", "Will handle " + Settings.Installation.UpgradeVersionList.Length + " versions");
            // Run upgrade again for all versions that do not have a corresponding logfile
            foreach (var upgradeVersion in Settings.Installation.UpgradeVersionList)
            {
                var complete = IsUpgradeComplete(upgradeVersion, "- check for FinishAbortedUpgrade");
                _installLogger.LogStep("", "Status for version " + upgradeVersion + " is " + complete);
                if (!complete)
                    UpgradeModule(upgradeVersion);
            }

            _installLogger.LogStep("", "FinishAbortedUpgrade done", false);

            // Restart application
            HttpRuntime.UnloadAppDomain();
            return callLog("ok", true);
        }


        public string GetAutoInstallPackagesUiUrl(ITenant tenant, IContainer container, bool isContentApp, int appId)
        {
            var moduleInfo = (container as DnnContainer)?.UnwrappedContents;
            var portal = (tenant as DnnTenant)?.UnwrappedContents;
            if(moduleInfo == null || portal == null)
                throw new ArgumentException("missing portal/module");

            // new: check if it should allow this
            // it should only be allowed, if the current situation is either
            // Content - and no views exist (even invisible ones)
            // App - and no apps exist - this is already checked on client side, so I won't include a check here
            if (isContentApp)
                try
                {
                    // we'll usually run into errors if nothing is installed yet, so on errors, we'll continue
                    var contentViews = new CmsRuntime(appId, Log, /*Edit.Enabled,*/ false).Views.GetAll();
                    if (contentViews.Any()) return null;
                }
                catch { /* ignore */ }
            
            // ReSharper disable StringLiteralTypo
            // Add desired destination
            // Add DNN Version, 2SexyContent Version, module type, module id, Portal ID
            var gettingStartedSrc =
                "//gettingstarted.2sxc.org/router.aspx?"
                + "destination=autoconfigure" + (isContentApp ? Eav.Constants.ContentAppName.ToLower() : "app")
                + "&DnnVersion=" + Assembly.GetAssembly(typeof(Globals)).GetName().Version.ToString(4)
                + "&2SexyContentVersion=" + Settings.ModuleVersion
                + "&ModuleName=" + moduleInfo.DesktopModule.ModuleName
                + "&ModuleId=" + moduleInfo.ModuleID
                + "&PortalID=" + moduleInfo.PortalID;
            // Add VDB / Zone ID (if set)
            var zoneMapper = Eav.Factory.Resolve<IZoneMapper>().Init(Log);
            var zoneId = zoneMapper.GetZoneId(moduleInfo.PortalID);
            gettingStartedSrc += "&ZoneID=" + zoneId;
            // ReSharper restore StringLiteralTypo

            // Add DNN Guid
            var hostSettings = HostController.Instance.GetSettingsDictionary();
            gettingStartedSrc += hostSettings.ContainsKey("GUID") ? "&DnnGUID=" + hostSettings["GUID"] : "";
            // Add Portal Default Language & current language
            gettingStartedSrc += "&DefaultLanguage=" + portal.DefaultLanguage
                + "&CurrentLanguage=" + portal.CultureCode;

            // Set src to iframe
            return gettingStartedSrc;
        }
    }
}
