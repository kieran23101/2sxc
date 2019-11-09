﻿using System;
using System.Collections.Generic;
using ToSic.Eav;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Caches;

namespace ToSic.Sxc.Blocks
{
    internal class BlockConfigForEntity : BlockConfigBase
    {
        internal BlockConfigForEntity(ICmsBlock cms): base(cms) { }

        #region methods which the entity-implementation must customize 

        protected override void SavePreviewTemplateId(Guid templateGuid)
            => Update(new Dictionary<string, object>
            {
                {BlockFromEntity.CbPropertyTemplate, templateGuid.ToString()}
            });


        internal override void SetAppId(int? appId)
        {
            // 2rm 2016-04-05 added resolver for app guid here (discuss w/ 2dm, I'm not sure why the id was saved before)
            var appName = "";
            if (appId.HasValue)
            {
                var zoneAppId = ((BaseCache)DataSource.GetCache(0, 0)).GetZoneAppId(null, appId);
                appName = ((BaseCache)DataSource.GetCache(0, 0)).ZoneApps[zoneAppId.Item1].Apps[appId.Value];
            }
            UpdateValue(BlockFromEntity.CbPropertyApp, appName);
        }

        internal override void EnsureLinkToContentGroup(Guid cgGuid)
            => UpdateValue(BlockFromEntity.CbPropertyContentGroup, cgGuid.ToString()); // must pre-convert to string, as it's not a reference to an entity in the same app


        internal override void UpdateTitle(IEntity titleItem)
        {
            if (titleItem?.GetBestTitle() == null) return;
            UpdateValue(BlockFromEntity.CbPropertyTitle, titleItem.GetBestTitle());
        }

        #endregion

        #region private helpers

        private void UpdateValue(string key, object value) 
            => Update(new Dictionary<string, object> { { key, value } });

        private void Update(Dictionary<string, object> newValues)
        {
            var app = ((BlockBase)CmsContext.Block).Parent.App;
            new AppManager(app.ZoneId, app.AppId, Log).Entities.UpdateParts(Math.Abs(CmsContext.Block.ContentBlockId), newValues);
        }

        #endregion

    }

}