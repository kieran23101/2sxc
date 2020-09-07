﻿using System;

namespace ToSic.Sxc.Adam
{
    /// <summary>
    /// The ADAM Navigator creates a folder object for an entity/field combination
    /// This is the root folder where all files for this field are stored
    /// </summary>
    public class FolderOfField : Folder
    {
        protected ContainerOfField Container { get; set; }
        public FolderOfField(/*IEnvironmentFileSystem enfFileSystem, */AdamAppContext adamContext, Guid entityGuid, string fieldName) 
            : base(adamContext/*, adamContext.EnvironmentFs*//* enfFileSystem*/)
        {
            Container = new ContainerOfField(AdamContext, entityGuid, fieldName);

            if (!Exists)
                return;

            // ReSharper disable once PatternAlwaysOfType
            if (!(AdamContext.Folder(Container.Root) is Eav.Apps.Assets.Folder f))
                return;

            Path = f.Path;
            Modified = f.Modified;
            Id = f.Id;
            Created = f.Created;
            Modified = f.Modified;

            // IAdamItem interface properties
            Name = f.Name;
            Url = (f as IAsset)?.Url;
        }

        public bool Exists => AdamContext.Exists(Container.Root);

    }

}