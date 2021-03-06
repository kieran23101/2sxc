﻿using System;
using ToSic.Eav;
using ToSic.Eav.Data;
using ToSic.Sxc.Run;

namespace ToSic.Sxc.Blocks.Edit
{
    internal class BlockEditorForModule: BlockEditorBase
    {
        protected override void SavePreviewTemplateId(Guid templateGuid)
            => Factory.Resolve<IEnvironmentConnector>().Init(Log)
                .SetPreview(Block.Context.Container.Id, templateGuid);

        internal override void SetAppId(int? appId)
            => Factory.Resolve<IEnvironmentConnector>().Init(Log)
                .SetAppId(Block.Context.Container, appId, Log);

        internal override void EnsureLinkToContentGroup(Guid cgGuid)
            => Factory.Resolve<IEnvironmentConnector>().Init(Log)
                .SetContentGroup(Block.Context.Container.Id, true, cgGuid);

        internal override void UpdateTitle(IEntity titleItem)
        {
            Log.Add("update title");
            Factory.Resolve<IEnvironmentConnector>().Init(Log).UpdateTitle(Block, titleItem);
        }
    }
}