﻿using System.Threading;
using DotNetNuke.Entities.Portals;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.LookUp;

namespace ToSic.Sxc.Dnn.LookUp
{
    /// <summary>
    /// Retrieves the current engine for a specific module. <br/>
    /// Internally it asks DNN for the current Property-Access objects and prepares them for use in EAV.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public class GetDnnEngine : IGetEngine
    {
        /// <inheritdoc />
        public ILookUpEngine GetEngine(int instanceId, ILog parentLog)
        {
            var portalSettings = PortalSettings.Current;
            return portalSettings == null 
                ? new LookUpEngine(parentLog) 
                : GenerateDnnBasedLookupEngine(portalSettings, instanceId, parentLog);
        }

        [PrivateApi]
        public static LookUpEngine GenerateDnnBasedLookupEngine(PortalSettings portalSettings, int instanceId, ILog parentLog)
        {
            var providers = new LookUpEngine(parentLog);
            var dnnUsr = portalSettings.UserInfo;
            var dnnCult = Thread.CurrentThread.CurrentCulture;
            var dnn = new DnnTokenReplace(instanceId, portalSettings, dnnUsr);
            var stdSources = dnn.PropertySources;
            foreach (var propertyAccess in stdSources)
                providers.Add(new LookUpInDnnPropertyAccess(propertyAccess.Key, propertyAccess.Value, dnnUsr, dnnCult));
            return providers;
        }
    }
}
