﻿using ToSic.Eav.Apps;
using ToSic.Eav.Logging;
using App = ToSic.SexyContent.App;
using IApp = ToSic.Sxc.Apps.IApp;

namespace ToSic.Sxc.SxcTemp
{
    internal class GetApp
    {
        /// <summary>
        /// Special constructor to clarify it's a reduced app without data
        /// Background: data operations need to know more like showDraft etc.
        /// which often isn't needed for simpler operations
        /// </summary>
        public static IApp LightWithoutData(ITenant tenant, int appId, ILog parentLog)
            => new App(tenant, ToSic.Eav.Apps.App.AutoLookupZone, appId, null, true, parentLog);

        public static IApp LightWithoutData(ITenant tenant, int zoneId, int appId, ILog parentLog)
            => new App(tenant, zoneId, appId, null, true, parentLog);

        public static IApp LightWithoutData(ITenant tenant, int zoneId, int appId, bool allowSideEffects, ILog parentLog)
            => new App(tenant, zoneId, appId, null, allowSideEffects, parentLog);


    }
}