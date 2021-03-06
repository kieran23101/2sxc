﻿using System;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Run;
using ToSic.Eav.Logging;
using ToSic.Eav.Security.Permissions;
using ToSic.Eav.WebApi.Errors;
using ToSic.Eav.WebApi.Security;
using ToSic.Sxc.Apps;
using ToSic.Sxc.Blocks;
using ToSic.Sxc.Blocks.Edit;

namespace ToSic.Sxc.WebApi.InPage
{
    internal class AppViewPickerBackend: HasLog
    {
        private IInstanceContext _context;
        private IBlock _block;
        private CmsRuntime _cmsRuntime;

        public AppViewPickerBackend() : base("Bck.ViwApp")
        {
        }

        public AppViewPickerBackend Init(IInstanceContext context, IBlock block, ILog parentLog)
        {
            Log.LinkTo(parentLog);
            _context = context;
            _block = block;
            _cmsRuntime = _block.App == null ? null : new CmsRuntime(_block.App, Log, true, false);
            return this;
        }


        public void SetAppId(int? appId)
        {
            BlockEditorBase.GetEditor(_block).SetAppId(appId);
        }


        public Guid? SaveTemplateId(int templateId, bool forceCreateContentGroup)
        {
            var permCheck = new MultiPermissionsApp().Init(_context, _block.App, Log);
            if (!permCheck.EnsureAll(GrantSets.WriteSomething, out var error))
                throw HttpException.PermissionDenied(error);

            return BlockEditorBase.GetEditor(_block).SaveTemplateId(templateId, forceCreateContentGroup);
        }

        public bool Publish(int id)
        {
            Log.Add($"try to publish id #{id}");
            if (!new MultiPermissionsApp().Init(_context, _block.App, Log).EnsureAll(GrantSets.WritePublished, out var error))
                throw HttpException.PermissionDenied(error);
            new AppManager(_block.App, Log).Entities.Publish(id);
            return true;
        }


        public string Render(int templateId, string lang)
        {
            var callLog = Log.Call<string>($"{nameof(templateId)}:{templateId}, {nameof(lang)}:{lang}");
            SetThreadCulture(lang);

            // if a preview templateId was specified, swap to that
            if (templateId > 0)
            {
                var template = _cmsRuntime.Views.Get(templateId);
                _block.View = template;
            }

            var rendered = _block.BlockBuilder.Render();
            return callLog("ok", rendered);
        }

        /// <summary>
        /// Try setting thread language to enable 2sxc to render the template in this language
        /// </summary>
        /// <param name="lang"></param>
        private static void SetThreadCulture(string lang)
        {
            if (!string.IsNullOrEmpty(lang))
                try
                {
                    System.Threading.Thread.CurrentThread.CurrentCulture =
                        System.Globalization.CultureInfo.GetCultureInfo(lang);
                }
                // Fallback / ignore if the language specified has not been found
                catch (System.Globalization.CultureNotFoundException)
                {
                    /* ignore */
                }
        }
    }
}
