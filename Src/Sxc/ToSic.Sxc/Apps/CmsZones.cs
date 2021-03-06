﻿using ToSic.Eav.Apps;
using ToSic.Eav.Logging;

namespace ToSic.Sxc.Apps
{
    public class CmsZones: ZoneRuntime
    {
        public CmsZones(int zoneId, ILog parentLog) : base(zoneId, parentLog) { }

        public AppsRuntime AppsRt => _apps ?? (_apps = new AppsRuntime(this, Log));
        private AppsRuntime _apps;

        public AppsManager AppsMan => _appsMan ?? (_appsMan = new AppsManager(this, Log));
        private AppsManager _appsMan;
    }
}
