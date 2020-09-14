﻿using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using DotNetNuke.Application;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using ToSic.Eav;
using ToSic.Eav.Apps;
using ToSic.Eav.Configuration;
using ToSic.Eav.Run;
using ToSic.Eav.WebApi.PublicApi;
using ToSic.Sxc.WebApi.Features;
using ToSic.Sxc.WebApi.Security;
using ToSic.Sxc.WebApi.System;

namespace ToSic.Sxc.WebApi.Cms
{
    /// <inheritdoc cref="ISystemController" />
    [SupportedModules("2sxc,2sxc-app")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public partial class SystemController : ISystemController
    {
        [HttpGet]
        public IEnumerable<Feature> Features(bool reload = false)
        {
            return Factory.Resolve<FeaturesBackend>().Init(Log).GetFeatures(reload);
            //if (reload)
            //    Eav.Configuration.Features.Reset();
            //return Eav.Configuration.Features.All;
        }

        [HttpGet]
        public IEnumerable<Feature> Features(int appId)
        {
            return Factory.Resolve<FeaturesBackend>().Init(Log).Features(GetContext(), PortalSettings.PortalId, appId);

            //// some dialogs don't have an app-id yet, because no app has been selected
            //// in this case, use the app-id of the content-app for feature-permission check
            //if (appId == Eav.Constants.AppIdEmpty)
            //{
            //    var environment = Factory.Resolve<IAppEnvironment>().Init(Log);
            //    var zoneId = environment.ZoneMapper.GetZoneId(PortalSettings.PortalId);
            //    appId = new ZoneRuntime(zoneId, Log).DefaultAppId;
            //}

            //return FeaturesHelpers.FeatureListWithPermissionCheck(new MultiPermissionsApp().Init(GetContext(), GetApp(appId), Log));
        }

        [HttpGet]
        public string ManageFeaturesUrl()
        {
            if (!UserInfo.IsSuperUser) return "error: user needs SuperUser permissions";

            return "//gettingstarted.2sxc.org/router.aspx?"
                   + $"DnnVersion={DotNetNukeContext.Current.Application.Version.ToString(4)}"
                   + $"&2SexyContentVersion={Settings.ModuleVersion}"
                   + $"&fp={HttpUtility.UrlEncode(Fingerprint.System)}"
                   + $"&DnnGuid={DotNetNuke.Entities.Host.Host.GUID}"
                   + $"&ModuleId={Request.FindModuleInfo().ModuleID}" // needed for callback later on
                   + "&destination=features";
        }

        [HttpPost]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Host)]
        public bool SaveFeatures([FromBody] FeaturesDto featuresManagementResponse)
        {
            return Factory.Resolve<FeaturesBackend>().Init(Log).SaveFeatures(featuresManagementResponse);
            //// first do a validity check 
            //if (featuresManagementResponse?.Msg?.Features == null) return false;

            //// 1. valid json? 
            //// - ensure signature is valid
            //if (!FeaturesManagementUtils.IsValidJson(featuresManagementResponse.Msg.Features)) return false;

            //// then take the newFeatures (it should be a json)
            //// and save to /desktopmodules/.data-custom/configurations/features.json
            //if (!FeaturesManagementUtils.SaveFeature(featuresManagementResponse.Msg.Features)) return false;

            //// when done, reset features
            //Eav.Configuration.Features.Reset();

            //return true;
        }

	}
}