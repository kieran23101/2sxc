﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Environment;
using ToSic.Eav.Apps.Run;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Sxc.Apps;
using ToSic.Sxc.Apps.Blocks;
using ToSic.Sxc.Blocks;
using static System.StringComparison;
using IApp = ToSic.Sxc.Apps.IApp;

namespace ToSic.Sxc.WebApi.ItemLists
{
    public class ListsBackendBase: HasLog
    {
        private readonly IPagePublishing _publishing;

        #region Constructor / di

        public ListsBackendBase(IPagePublishing publishing) : base("Bck.Lists")
        {
            _publishing = publishing.Init(Log);
        }

        public ListsBackendBase Init(IApp app, ILog parentLog)
        {
            Log.LinkTo(parentLog);
            _app = app;
            return this;
        }

        private IApp _app;

        #endregion



        // TODO: shouldn't be part of ContentGroupController any more, as it's generic now
        public void Replace(IInstanceContext context, Guid guid, string part, int index, int entityId, bool add = false)
        {
            var wrapLog = Log.Call($"target:{guid}, part:{part}, index:{index}, id:{entityId}");
            var versioning = Factory.Resolve<IPagePublishing>().Init(Log);

            void InternalSave(VersioningActionInfo args)
            {
                var cms = new CmsManager(_app, Log);
                var entity = cms.AppState.List.One(guid);
                if (entity == null) throw new Exception($"Can't find item '{guid}'");

                // correct casing of content / listcontent for now - TODO should already happen in JS-Call
                if (entity.Type.Name == BlocksRuntime.BlockTypeName)
                {
                    if (string.Equals(part, ViewParts.Content, OrdinalIgnoreCase)) part = ViewParts.Content;
                    if (string.Equals(part, ViewParts.ListContent, OrdinalIgnoreCase)) part = ViewParts.ListContent;
                }

                if (add)
                    cms.Entities.FieldListAdd(entity, new[] { part }, index, new int?[] { entityId }, cms.EnablePublishing);
                else
                    cms.Entities.FieldListReplaceIfModified(entity, new[] { part }, index, new int?[] { entityId },
                        cms.EnablePublishing);
            }

            // use dnn versioning - this is always part of page
            //var block = GetBlock();
            versioning.DoInsidePublishing(context, InternalSave);
            wrapLog(null);
        }

        // TODO: WIP changing this from ContentGroup editing to any list editing


        public ReplacementListDto GetReplacementOptions(Guid guid, string part, int index)
        {
            var wrapLog = Log.Call<ReplacementListDto>($"target:{guid}, part:{part}, index:{index}");
            part = part.ToLower();

            var itemList = FindContentGroupAndTypeName(guid, part, out var attributeSetName)
                           ?? FindItemAndFieldTypeName(guid, part, out attributeSetName);

            // if no type was defined in this set, then return an empty list as there is nothing to choose from
            if (string.IsNullOrEmpty(attributeSetName))
                return null;

            var appState = State.Get(_app);
            var ct = appState.GetContentType(attributeSetName);

            var dataSource = _app.Data[ct.Name];
            var results = dataSource.List.ToDictionary(p => p.EntityId,
                p => p.GetBestTitle() ?? "");

            // if list is empty or shorter than index (would happen in an add-to-end-request) return null
            var selectedId = itemList.Count > index
                ? itemList[index]?.EntityId
                : null;

            var result = new ReplacementListDto {SelectedId = selectedId, Items = results, ContentTypeName = ct.StaticName};
            return wrapLog(null, result);
        }



        public List<EntityInListDto> ItemList(Guid guid, string part)
        {
            Log.Add($"item list for:{guid}");
            var cg = _app.Data.List.One(guid);
            var itemList = cg.Children(part);

            var list = itemList.Select((c, index) => new EntityInListDto
            {
                Index = index,
                Id = c?.EntityId ?? 0,
                Guid = c?.EntityGuid ?? Guid.Empty,
                Title = c?.GetBestTitle() ?? "",
            }).ToList();

            return list;
        }


        // TODO: part should be handed in with all the relevant names! atm it's "content" in the content-block scenario
        public bool Reorder(IInstanceContext context, Guid guid, List<EntityInListDto> list,  string part = null)
        {
            Log.Add($"list for:{guid}, items:{list?.Count}");
            if (list == null) throw new ArgumentNullException(nameof(list));

            _publishing.DoInsidePublishing(context, args =>
            {
                var cms = new CmsManager(_app, Log);
                var entity = cms.Read.AppState.List.One(guid);

                var sequence = list.Select(i => i.Index).ToArray();
                var fields = part == ViewParts.ContentLower ? ViewParts.ContentPair : new[] {part};
                cms.Entities.FieldListReorder(entity, fields, sequence, cms.EnablePublishing);
            });

            return true;
        }


        private List<IEntity> FindItemAndFieldTypeName(Guid guid, string part, out string attributeSetName)
        {
            var parent = _app.Data.List.One(guid);
            if (parent == null) throw new Exception($"No item found for {guid}");
            if (!parent.Attributes.ContainsKey(part)) throw new Exception($"Could not find field {part} in item {guid}");
            var itemList = parent.Children(part);

            // find attribute-type-name
            var attribute = parent.Type[part];
            if (attribute == null) throw new Exception($"Attribute definition for '{part}' not found on the item {guid}");
            attributeSetName = attribute.EntityFieldItemTypePrimary();
            return itemList;
        }

        private List<IEntity> FindContentGroupAndTypeName(Guid guid, string part, out string attributeSetName)
        {
            var wrapLog = Log.Call<List<IEntity>>($"{guid}, {part}");
            var contentGroup = GetContentGroup(guid);
            attributeSetName = null;
            var partIsContent = string.Equals(part, ViewParts.ContentLower, OrdinalIgnoreCase);
            // try to get the entityId. Sometimes it will try to get #0 which doesn't exist yet, that's why it has these checks
            var itemList = partIsContent ? contentGroup.Content : contentGroup.Header;

            if (itemList == null) return wrapLog(null, null);

            // not sure what this check is for, just leaving it in for now (2015-09-19 2dm)
            if (contentGroup.View == null)
            {
                Log.Add("Something found, but doesn't seem to be a content-group. Cancel.");
                return wrapLog(null, null);
            }

            attributeSetName = partIsContent ? contentGroup.View.ContentType : contentGroup.View.HeaderType;
            return wrapLog(null, itemList);
        }


        public EntityInListDto HeaderItem(Guid guid)
        {
            Log.Add($"header for:{guid}");
            var cg = GetContentGroup(guid);

            // new in v11 - this call might be run on a non-content-block, in which case we return null
            if (cg.Entity.Type.Name != BlocksRuntime.BlockTypeName) return null;

            var header = cg.Header.FirstOrDefault();

            return new EntityInListDto
            {
                Index = 0,
                Id = header?.EntityId ?? 0,
                Guid = header?.EntityGuid ?? Guid.Empty,
                Title = header?.GetBestTitle() ?? "",
                Type = header?.Type.StaticName ?? cg.View.HeaderType
            };
        }



        protected BlockConfiguration GetContentGroup(Guid contentGroupGuid)
        {
            Log.Add($"get group:{contentGroupGuid}");
            var cms = new CmsRuntime(_app, Log, true, false);
            var contentGroup = cms.Blocks.GetBlockConfig(contentGroupGuid);

            if (contentGroup == null)
                throw new Exception("BlockConfiguration with Guid " + contentGroupGuid + " does not exist.");
            return contentGroup;
        }

    }
}
