﻿using DotNetNuke.Entities.Portals;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Environment;
using ToSic.Eav.Logging;
using ToSic.SexyContent.Interfaces;
using DynamicCodeHelper = ToSic.Sxc.Dnn.DynamicCodeHelper;
using IApp = ToSic.Eav.Apps.IApp;
using IEntity = ToSic.Eav.Data.IEntity;
using PermissionCheckBase = ToSic.Eav.Security.PermissionCheckBase;

namespace ToSic.SexyContent.Environment.Dnn7
{
    public class DnnEnvironmentFactory : IEnvironmentFactory, IWebFactoryTemp
    {
        public PermissionCheckBase ItemPermissions(IInAppAndZone appIdentity, IEntity targetItem, ILog parentLog, IContainer module = null) 
            => new DnnPermissionCheck(parentLog, targetItem: targetItem, instance: module, portal: PortalSettings.Current, appIdentity: appIdentity);

        public PermissionCheckBase TypePermissions(IInAppAndZone appIdentity, IContentType targetType, IEntity targetItem, ILog parentLog, IContainer module = null) 
            => new DnnPermissionCheck(parentLog, targetType, targetItem, module, portal: PortalSettings.Current, appIdentity: appIdentity);

        public PermissionCheckBase InstancePermissions(ILog parentLog, IContainer module, IApp app)
            => new DnnPermissionCheck(parentLog, portal: PortalSettings.Current, instance: module, app: app);

        public IPagePublishing PagePublisher(ILog parentLog) => new PagePublishing(parentLog);

        public IAppEnvironment Environment(ILog parentLog) => new DnnEnvironment(parentLog);




        public Sxc.Web.DynamicCodeHelper AppAndDataHelpers(Sxc.Blocks.ICmsBlock cms) => new DynamicCodeHelper(cms);
    }
}