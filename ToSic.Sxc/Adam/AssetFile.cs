﻿using ToSic.Eav.Apps.Assets;
using ToSic.SexyContent;

namespace ToSic.Sxc.Adam
{
    public class AssetFile : File, IAsset
    {
        private AdamAppContext AppContext { get; }

        public AssetFile(AdamAppContext appContext)
        {
            AppContext = appContext;
        }

        /// <inheritdoc />
        public DynamicEntity Metadata => Adam.Metadata.GetFirstOrFake(AppContext, Id, false);

        /// <inheritdoc />
        public bool HasMetadata => Adam.Metadata.GetFirstMetadata(AppContext.App, Id, false) != null;

        /// <inheritdoc />
        public  string Url => AppContext.Tenant.ContentPath + Folder + FileName;

         /// <inheritdoc />
       public string Type => Classification.TypeName(Extension);

         /// <inheritdoc />
       public string Name { get; internal set; }
    }
}