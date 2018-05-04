﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using ToSic.Eav.Apps;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Security.Permissions;
using ToSic.SexyContent.Environment.Dnn7;
using ToSic.SexyContent.Razor.Helpers;
using ToSic.SexyContent.WebApi.Errors;
using ToSic.SexyContent.WebApi.Permissions;
using ToSic.Sxc.Adam;
using SysConf = ToSic.Eav.Configuration;
using Feats = ToSic.Eav.Configuration.Features;

namespace ToSic.SexyContent.WebApi.Adam
{
    internal class AdamSecureState: AppAndPermissions
    {
        public string Field;
        public bool UserIsRestricted;
        public Guid Guid;
        internal ContainerBase ContainerContext;
        internal AdamAppContext AdamAppContext;
        internal IAttributeDefinition Attribute;

        public readonly Guid[] FeaturesForRestrictedUsers =
            {
                SysConf.FeatureIds.PublicUpload,
                SysConf.FeatureIds.PublicForms
            };

        /// <summary>
        /// Initializes the object and performs all the initial security checks
        /// </summary>
        public AdamSecureState(SxcInstance sxcInstance, int appId, string contentType, string field, Guid guid, bool usePortalRoot, Log log)
            : base(sxcInstance, appId, log)
        {
            Field = field;
            Guid = guid;
            EnsureOrThrow(GrantSets.WriteSomething, contentType);
            UserIsRestricted = !SecurityChecks.ThrowIfUserMayNotWriteEverywhere(usePortalRoot, Permissions);


                if (UserIsRestricted && !Feats.Enabled(FeaturesForRestrictedUsers))
                    throw Http.PermissionDenied($"low-permission users may not access this - {Feats.MsgMissingSome(FeaturesForRestrictedUsers)}");

            PrepCore(App, guid, field, usePortalRoot);

            if (string.IsNullOrEmpty(contentType) || string.IsNullOrEmpty(field)) return;

            Attribute = Definition(appId, contentType, field);
            ThrowIfWrongFieldType();
        }

        private void PrepCore(App app, Guid entityGuid, string fieldName, bool usePortalRoot)
        {
            var dnn = new DnnHelper(SxcInstance?.EnvInstance);
            var tenant = new DnnTenant(dnn.Portal);
            AdamAppContext = new AdamAppContext(tenant, app, SxcInstance);
            ContainerContext = usePortalRoot
                ? new ContainerOfTenant(AdamAppContext) as ContainerBase
                : new ContainerOfField(AdamAppContext, entityGuid, fieldName);
        }

        private IAttributeDefinition Definition(int appId, string contentType, string fieldName)
        {
            // try to find attribute definition - for later extra security checks
            var appRead = new AppRuntime(appId, Log);
            var type = appRead.ContentTypes.Get(contentType);
            return type[fieldName];
        }

        public void ThrowIfWrongFieldType()
        {
            var fieldDef = Attribute;

            // check if this field exists and is actually a file-field or a string (wysiwyg) field
            if (fieldDef == null || !(fieldDef.Type != Eav.Constants.DataTypeHyperlink || fieldDef.Type != Eav.Constants.DataTypeString))
                throw Http.BadRequest("Requested field '" + Field + "' type doesn't allow upload");
            Log.Add($"field type:{fieldDef.Type}");
        }

        [AssertionMethod]
        public void ThrowIfRestrictedUserIsntPermittedOnField(List<Grants> requiredPermissions)
        {
            // check field permissions, but only for non-publish-data
            if (UserIsRestricted && !FieldPermissionOk(requiredPermissions))
                throw Http.PermissionDenied("this field is not configured to allow uploads by the current user");
        }


        /// <summary>
        /// This will check if the field-definition grants additional rights
        /// Should only be called if the user doesn't have full edit-rights
        /// </summary>
        public bool FieldPermissionOk(List<Grants> requiredGrant)
        {
            var fieldPermissions = new DnnPermissionCheck(Log,
                instance: SxcInstance.EnvInstance,
                permissions1: Attribute.Permissions);

            return fieldPermissions.UserMay(requiredGrant);
        }

        public void ThrowIfRestrictedUserIsOutsidePermittedFolders(string path)
        {
            if (UserIsRestricted)
                SecurityChecks.ThrowIfDestNotInItem(Guid, Field, path);
        }

        public void ThrowIfBadExtension(string fileName)
        {
            if (!SecurityChecks.IsAllowedDnnExtension(fileName))
                throw Http.NotAllowedFileType(fileName, "Not in whitelisted CMS file types.");

            if (SecurityChecks.IsKnownRiskyExtension(fileName))
                throw Http.NotAllowedFileType(fileName, "This is a known risky file type.");

        }

        internal bool CustomFileFilterOk(string additionalFilter, string fileName)
        {
            var extension = Path.GetExtension(fileName)?.TrimStart('.') ?? "";
            var hasNonAzChars = new Regex("[^a-z]", RegexOptions.IgnoreCase);

            Log.Add($"found additional file filter: {additionalFilter}");
            var filters = additionalFilter.Split(',').Select(f => f.Trim()).ToList();
            Log.Add($"found {filters.Count} filters in {additionalFilter}, will test on {fileName} with ext {extension}");

            foreach (var f in filters)
            {
                // just a-z characters
                if (!hasNonAzChars.IsMatch(f))
                    if (extension == f)
                    {
                        Log.Add($"filter {f} matched filename {fileName}");
                        return true;
                    }
                    else continue;

                // could be regex or simple *.ext
                if (f.StartsWith("*."))
                    if (string.Equals(extension, f.Substring(2), StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Add($"filter {f} matched filename {fileName}");
                        return true;
                    }
                    else continue;

                // it's a regex
                try
                {
                    if (new Regex(f, RegexOptions.IgnoreCase).IsMatch(fileName))
                    {
                        Log.Add($"filter {f} matched filename {fileName}");
                        return true;
                    }
                }
                catch
                {
                    Log.Add($"filter {f} was detected as reg-ex but threw error");
                }

            }

            return false;
        }
    }
}