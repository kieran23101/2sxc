﻿using ToSic.Eav.Security.Permissions;

namespace ToSic.Sxc.WebApi.Adam
{
    internal class AdamTransDelete<TFolderId, TFileId> : AdamTransactionBase<AdamTransDelete<TFolderId, TFileId>, TFolderId, TFileId>
    {
        public AdamTransDelete() : base("Adm.TrnDel") { }

        internal bool Delete(string parentSubfolder, bool isFolder, TFolderId id, TFileId fileId)
        {
            Log.Add($"delete");
            if (!State.Security.UserIsPermittedOnField(GrantSets.DeleteSomething, out var exp))
                throw exp;

            // check that if the user should only see drafts, he doesn't see items of published data
            if (!State.Security.UserIsNotRestrictedOrItemIsDraft(State.ItemGuid, out var permissionException))
                throw permissionException;

            // try to see if we can get into the subfolder - will throw error if missing
            var parent = State.ContainerContext.Folder(parentSubfolder, false); // as IFolder<TFolderId, TFileId>;

            var fs = State.AdamAppContext.AdamFs;
            if (isFolder)
            {
                var target = fs.GetFolder(id);
                VerifySecurityAndStructure(parent, target, "can't delete folder");
                fs.Delete(target);
            }
            else
            {
                var target = fs.GetFile(fileId);
                VerifySecurityAndStructure(parent, target, "can't delete file");
                fs.Delete(target);
            }

            Log.Add("delete complete");
            return true;
        }

    }
}
