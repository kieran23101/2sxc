﻿using ToSic.Eav.Logging;
using ToSic.SexyContent.Razor.Helpers;
using ToSic.Sxc.Blocks;

namespace ToSic.Sxc.Dnn
{
    public class DynamicCode : Sxc.Web.DynamicCode, IDynamicCode
    {
        public ICmsBlock CmsBlock;

        public DynamicCode(ICmsBlock cmsBlock, ILog parentLog = null): base(cmsBlock, new Tenant(null), parentLog)
        {
            CmsBlock = cmsBlock;
            // Init things than require module-info or similar, but not 2sxc
            var instance = cmsBlock?.Container;
            Dnn = new DnnHelper(instance);
            Link = new LinkHelper(Dnn);
        }

        #region IHasDnnContext

        /// <summary>
        /// Dnn context with module, page, portal etc.
        /// </summary>
        public IDnnContext Dnn { get; }

        #endregion


    }
}