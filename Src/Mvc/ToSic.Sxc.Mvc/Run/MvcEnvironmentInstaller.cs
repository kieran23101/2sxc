﻿using ToSic.Eav.Logging;
using ToSic.Eav.Run;
using ToSic.Sxc.Run;

namespace ToSic.Sxc.Mvc.Run
{
    public class MvcEnvironmentInstaller: HasLog, IEnvironmentInstaller
    {
        public string UpgradeMessages()
        {
            // for now, always assume installation worked
            return null;
        }

        private bool IsUpgradeRunning => false;

        public bool ResumeAbortedUpgrade()
        {
            // don't do anything for now
            // throw new NotImplementedException();
            return true;
        }

        public string GetAutoInstallPackagesUiUrl(ITenant tenant, IContainer container, bool forContentApp, int appId)
        {
            return "mvc not implemented #todo #mvc";
        }

        public MvcEnvironmentInstaller(string logName, ILog parentLog = null, string initialMessage = null, string cPath = null, string cName = null, int cLine = 0) : base(logName, parentLog, initialMessage, cPath, cName, cLine)
        {
        }

        public IEnvironmentInstaller Init(ILog parent)
        {
            Log.LinkTo(parent);
            return this;
        }
    }
}
