﻿namespace ToSic.Sxc.WebApi.Cms
{
    // #2134 - all this code was deprecated when dropping the old UI
    public partial class EntitiesController
    {
        #region Old API for V3 - Obsolete, must remove when we drop the old UI
        // #2134
        ///// <inheritdoc />
        //[HttpPost]
        //[DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        //public dynamic GetManyForEditing([FromBody] List<ItemIdentifier> items, int appId)
        //{
        //    var wrapLog = Log.Call($"get many a#{appId}, items⋮{items.Count}");

        //    // before we start, we have to convert the indexes into something more useful, because
        //    // otherwise in content-list scenarios we don't have the type
        //    var appForSecurityChecks = GetApp.LightWithoutData(new DnnTenant(PortalSettings), SystemRuntime.ZoneIdOfApp(appId), appId, Log);
        //    items = new ContentGroupList(BlockBuilder, Log).ConvertListIndexToId(items, appForSecurityChecks);

        //    // to do full security check, we'll have to see what content-type is requested
        //    var permCheck = new MultiPermissionsTypes(BlockBuilder, appId, items, Log);

        //    if (!permCheck.EnsureAll(GrantSets.WriteSomething, out var exception))
        //        throw exception;

        //    var list = new EntityApi(appId, permCheck.EnsureAny(GrantSets.ReadDraft), Log).GetEntitiesForEditing(items);

        //    // Reformat to the Entity-WithLanguage setup
        //    var listAsEwH = list.Select(p => new BundleWithHeader<EntityWithLanguages>
        //    {
        //        Header = p.Header,
        //        Entity = p.Entity != null
        //            ? EntityWithLanguages.Build(appId, p.Entity)
        //            : null
        //    }).ToList();

        //    // 2018-09-26 2dm
        //    // if we're giving items which already exist, then we must verify that edit/read is allowed.
        //    // important, this code is shared/duplicated in the UiController.Load
        //    if (list.Any(set => set.Entity != null))
        //        if (!permCheck.EnsureAll(GrantSets.ReadSomething, out exception))
        //            throw exception;

        //    wrapLog($"will return items⋮{list.Count}");
        //    return listAsEwH;
        //}


        ///// <inheritdoc />
        //[HttpPost]
        //[DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        //public Dictionary<Guid, int> SaveMany([FromUri] int appId, [FromBody] List<BundleWithHeader<EntityWithLanguages>> items, [FromUri] bool partOfPage = false)
        //{
        //    // log and do security check
        //    Log.Add($"save many started with a#{appId}, i⋮{items.Count}, partOfPage:{partOfPage}");

        //    var appReadForSecurityCheckOnly = new AppRuntime(appId, true, Log);
        //    #region check if it's an update, and do more security checks - shared with UiController.Save
        //    // basic permission checks
        //    var permCheck = new Security.Security(BlockBuilder, Log)
        //        .DoPreSaveSecurityCheck(appId, items);

        //    var foundItems = items.Where(i => i.EntityId != 0 && i.EntityGuid != Guid.Empty)
        //        .Select(i => i.EntityGuid != Guid.Empty
        //            ? appReadForSecurityCheckOnly.Entities.Get(i.EntityGuid) // prefer guid access if available
        //            : appReadForSecurityCheckOnly.Entities.Get(i.EntityId)  // otherwise id
        //        );
        //    if (foundItems.Any(i => i != null) && !permCheck.EnsureAll(GrantSets.UpdateSomething, out var exception))
        //        throw exception;
        //    #endregion

        //    return new DnnPublishing(BlockBuilder, Log)
        //        .SaveWithinDnnPagePublishingAndUpdateParent(appId, items, partOfPage,
        //            forceSaveAsDraft => SaveOldFormatKeepTillReplaced(appId, items, partOfPage, forceSaveAsDraft),
        //            permCheck);
        //}



        //private Dictionary<Guid, int> SaveOldFormatKeepTillReplaced(int appId,
        //       List<BundleWithHeader<EntityWithLanguages>> items,
        //       bool partOfPage,
        //       bool forceDraft)
        //{
        //    Log.Add($"SaveAndProcessGroups(..., appId:{appId}, items:{items?.Count}), partOfPage:{partOfPage}, forceDraft:{forceDraft}");

        //    // first, save all to do it in 1 transaction
        //    // note that it won't save the SlotIsEmpty ones, as these won't be needed
        //    var eavEntitiesController = new Eav.WebApi.EntitiesController(Log);
        //    return eavEntitiesController.SaveMany(appId, items, partOfPage, forceDraft);
        //}
        #endregion




        #region Content Types
        //// #2134
        ///// <summary>
        ///// Get a ContentType by Name
        ///// </summary>
        //[HttpGet]
        //[DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Admin)]
        //public ContentTypeInfo GetContentType(string contentType, int appId)
        //    => new Eav.WebApi.ContentTypeController().GetSingle(appId, contentType);

        #endregion

    }
}
