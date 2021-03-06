﻿using ToSic.Eav.WebApi.Dto;
using ToSic.Eav.Apps;

namespace ToSic.Sxc.WebApi.Context
{
    public interface IContextBuilder
    {
        IContextBuilder InitApp(int? zoneId, IApp app);
        ContextDto Get(Ctx flags);
    }
}
