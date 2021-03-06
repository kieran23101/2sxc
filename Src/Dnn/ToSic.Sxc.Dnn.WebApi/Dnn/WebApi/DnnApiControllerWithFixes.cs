﻿using System;
using System.Web;
using System.Web.Http.Controllers;
using DotNetNuke.Web.Api;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.WebApi.Helpers;
using ToSic.Sxc.Dnn.WebApi.Logging;
using ToSic.Sxc.WebApi;

namespace ToSic.Sxc.Dnn.WebApi
{
    [DnnLogWebApi, JsonResponse]
    public abstract class DnnApiControllerWithFixes: DnnApiController, IHasLog
    {
        protected DnnApiControllerWithFixes() 
	    {
            // ensure that the sql connection string is correct
            // this is technically only necessary, when dnn just restarted and didn't already set this
            Settings.EnsureSystemIsInitialized();

	        // ensure that the call to this webservice doesn't reset the language in the cookie
	        // this is a dnn-bug
	        RemoveLanguageChangingCookie();

            // ReSharper disable once VirtualMemberCallInConstructor
            Log = new Log(HistoryLogName, null, $"Path: {HttpContext.Current?.Request?.Url?.AbsoluteUri}");
            TimerWrapLog = Log.Call(message: "timer", useTimer: true);
	        
            // ReSharper disable VirtualMemberCallInConstructor
	        if (HistoryLogGroup != null)
	            History.Add(HistoryLogGroup, Log);
            // ReSharper restore VirtualMemberCallInConstructor

        }

        // ReSharper disable once InconsistentNaming
        private readonly Action<string> TimerWrapLog;

        protected override void Initialize(HttpControllerContext controllerContext)
	    {
            // Add the logger to the request, in case it's needed in error-reporting
	        controllerContext.Request.Properties.Add(Constants.EavLogKey, Log);
	        base.Initialize(controllerContext);
        }

        protected override void Dispose(bool disposing)
        {
            TimerWrapLog(null);
            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public ILog Log { get; }

        /// <summary>
        /// The group name for log entries in insights.
        /// Helps group various calls by use case. 
        /// </summary>
        protected virtual string HistoryLogGroup { get; } = "web-api";

        /// <summary>
        /// The name of the logger in insights.
        /// The inheriting class should provide the real name to be used.
        /// </summary>
        protected abstract string HistoryLogName { get; }

        #region Helpers

        internal static void RemoveLanguageChangingCookie()
        {
            // this is to fix a dnn-bug, which adds a cookie to set the language
            // but always defaults to the main language, which is simply wrong. 
            var cookies = global::System.Web.HttpContext.Current?.Response.Cookies;
            cookies?.Remove("language"); // try to remove, otherwise no exception will be thrown
        }

        #endregion

        #region Extend Time so Web Server doesn't time out

        protected void PreventServerTimeout300() => HttpContext.Current.Server.ScriptTimeout = 300;

        #endregion

    }
}