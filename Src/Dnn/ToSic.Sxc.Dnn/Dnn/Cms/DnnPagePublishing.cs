﻿using System;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Enums;
using ToSic.Eav.Apps.Environment;
using ToSic.Eav.Apps.Run;
using ToSic.Eav.Logging;
using ToSic.Sxc.Blocks;
using ToSic.Sxc.Data;
using ToSic.Sxc.DataSources;
using ToSic.Sxc.Dnn.Run;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Sxc.Dnn.Cms
{
    public partial class DnnPagePublishing : HasLog, IPagePublishing
    {
        #region DI Constructors and More
        
        public DnnPagePublishing(): base("Dnn.Publsh") { }

        public IPagePublishing Init(ILog parent)
        {
            Log.LinkTo(parent);
            return this;
        }

        #endregion

        public bool Supported => true;

        private readonly Dictionary<int, PublishingMode> _cache = new Dictionary<int, PublishingMode>();
        
        public PublishingMode Requirements(int instanceId)
        {
            var wrapLog = Log.Call<PublishingMode>($"{instanceId}");
            if (_cache.ContainsKey(instanceId)) return wrapLog("in cache", _cache[instanceId]);

            Log.Add($"Requirements(mod:{instanceId}) - checking first time (others will be cached)");
            try
            {
                var moduleInfo = ModuleController.Instance.GetModule(instanceId, Null.NullInteger, true);
                PublishingMode decision;
                var versioningEnabled =
                    TabChangeSettings.Instance.IsChangeControlEnabled(moduleInfo.PortalID, moduleInfo.TabID);
                if (!versioningEnabled)
                    decision = PublishingMode.DraftOptional;
                else if (!new PortalSettings(moduleInfo.PortalID).UserInfo.IsSuperUser)
                    decision = PublishingMode.DraftRequired;
                else
                    decision = PublishingMode.DraftRequired;

                _cache.Add(instanceId, decision);
                return wrapLog("decision: ", decision);
            }
            catch
            {
                Log.Add("Requirements had exception!");
                throw;
            }
        }
        
        public bool IsEnabled(int instanceId) => Requirements(instanceId) != PublishingMode.DraftOptional;

        public void DoInsidePublishing(IInstanceContext context, Action<VersioningActionInfo> action)
        {
            var instanceId = context.Container.Id;
            var userId = (context.User as DnnUser).UnwrappedContents.UserID;
            var enabled = IsEnabled(instanceId);
            Log.Add($"DoInsidePublishing(module:{instanceId}, user:{userId}, enabled:{enabled})");
            if (enabled)
            {
                var moduleVersionSettings = new DnnPagePublishing.ModuleVersions(instanceId, Log);
                
                // Get an new version number and submit it to DNN
                // The submission must be made every time something changes, because a "discard" could have happened
                // in the meantime.
                TabChangeTracker.Instance.TrackModuleModification(
                    moduleVersionSettings.ModuleInfo, 
                    moduleVersionSettings.IncreaseLatestVersion(), 
                    userId
                );
            }

            var versioningActionInfo = new VersioningActionInfo();
            action.Invoke(versioningActionInfo);
            Log.Add("/DoInsidePublishing");
        }



        public int GetLatestVersion(int instanceId)
        {
            var moduleVersionSettings = new DnnPagePublishing.ModuleVersions(instanceId, Log);
            var ver = moduleVersionSettings.GetLatestVersion();
            Log.Add($"GetLatestVersion(m:{instanceId}) = ver:{ver}");
            return ver;
        }

        public int GetPublishedVersion(int instanceId)
        {
            var moduleVersionSettings = new DnnPagePublishing.ModuleVersions(instanceId, Log);
            var publ = moduleVersionSettings.GetPublishedVersion();
            Log.Add($"GetPublishedVersion(m:{instanceId}) = pub:{publ}");
            return publ;
        }


        public void Publish(int instanceId, int version)
        {
            Log.Add($"Publish(m:{instanceId}, v:{version})");
            try
            {
                // publish all entites of this content block
                var dnnModule = ModuleController.Instance.GetModule(instanceId, Null.NullInteger, true);
                var container = new DnnContainer().Init(dnnModule, Log);
                // must find tenant through module, as the Portal-Settings.Current is null in search mode
                var tenant = new DnnTenant().Init(dnnModule.OwnerPortalID);
                var cb = new BlockFromModule().Init(new DnnContext(tenant, container, new DnnUser()), Log);

                Log.Add($"found dnn mod {container.Id}, tenant {tenant.Id}, cb exists: {cb.ContentGroupExists}");
                if (cb.ContentGroupExists)
                {
                    Log.Add("cb exists");
                    var appManager = new AppManager(cb, Log);

                    // Add content entities
                    IEnumerable<IEntity> list = new List<IEntity>();
                    list = TryToAddStream(list, cb.Data, Constants.DefaultStreamName);
                    list = TryToAddStream(list, cb.Data, "ListContent");
                    list = TryToAddStream(list, cb.Data, "PartOfPage");

                    // ReSharper disable PossibleMultipleEnumeration
                    // Find related presentation entities
                    var attachedPresItems = list
                        .Where(e => (e as EntityInBlock)?.Presentation != null)
                        .Select(e => ((EntityInBlock)e).Presentation);
                    Log.Add($"adding presentation item⋮{attachedPresItems.Count()}");
                    list = list.Concat(attachedPresItems);
                    // ReSharper restore PossibleMultipleEnumeration

                    var ids = list.Where(e => !e.IsPublished).Select(e => e.EntityId).ToList();

                    // publish BlockConfiguration as well - if there already is one
                    if (cb.Configuration != null)
                    {
                        Log.Add($"add group id:{cb.Configuration.Id}");
                        ids.Add(cb.Configuration.Id);
                    }

                    Log.Add(() => $"will publish id⋮{ids.Count} ids:[{ string.Join(",", ids.Select(i => i.ToString()).ToArray()) }]");

                    if (ids.Any())
                        appManager.Entities.Publish(ids.ToArray());
                    else
                        Log.Add("no ids found, won\'t publish items");
                }

                // Set published version
                new DnnPagePublishing.ModuleVersions(instanceId, Log).PublishLatestVersion();
                Log.Add("publish completed");
            }
            catch (Exception ex)
            {
                DnnLogging.LogToDnn("exception", "publishing", Log, force:true);
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                throw;
            }

        }

        private IEnumerable<IEntity> TryToAddStream(IEnumerable<IEntity> list, IBlockDataSource data, string key)
        {
            var cont = data.Out.ContainsKey(key) ? data[key]?.List?.ToList() : null;
            Log.Add($"TryToAddStream(..., ..., key:{key}), found:{cont != null} add⋮{cont?.Count ?? 0}" );
            if (cont != null) list = list.Concat(cont);
            return list;
        }

    }
}
