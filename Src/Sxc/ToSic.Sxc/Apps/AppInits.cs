﻿using ToSic.Eav.Apps;
using ToSic.Eav.Logging;
using ToSic.Eav.LookUp;
using ToSic.Sxc.Blocks;
using ToSic.Sxc.LookUp;

namespace ToSic.Sxc.Apps
{
    public static class AppInits
    {
        public static IApp Init(this App app, IAppIdentity appIdentity, ILog log, bool showDrafts = false)
        {
            var buildConfig = ConfigurationProvider.Build(showDrafts, false, new LookUpEngine(log));
            return app.Init(appIdentity, buildConfig, false, log);
        }

        public static IApp Init(this App app, int appId, ILog log, IBlock optionalBlock = null, bool showDrafts = false)
        {
            var appIdentity = new AppIdentity(SystemRuntime.ZoneIdOfApp(appId), appId);
            if (optionalBlock == null) return app.Init(appIdentity, log, showDrafts);
            var buildConfig = ConfigurationProvider.Build(optionalBlock, true);
            return app.Init(appIdentity, buildConfig, false, log);
        }
    }
}
