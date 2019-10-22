﻿using System;
using System.Collections.Generic;
using ToSic.Eav.DataSources;
using ToSic.Eav.ValueProvider;
using ToSic.SexyContent.DataSources;

// ReSharper disable once CheckNamespace
namespace ToSic.SexyContent
{
    /// <remarks>
    /// Deprecation notice: this is an old interface in the "bad" SexyContent namespace.
    /// We'll probably keep it forever, but don't add any more features. Instead, put it on the new interface
    /// This will force developers to use the new interface without breaking compatibility
    /// </remarks>
    [Obsolete("Avoid this - it's in an old namespace. Use the ToSic.Sxc.Interfaces.IAppAndDataHelpers")]
    public interface IAppAndDataHelpers
    {
        App App { get; }
        ViewDataSource Data { get; }

        SxcHelper Sxc { get; }

        /// <summary>
        /// Transform a IEntity to a DynamicEntity as dynamic object
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        dynamic AsDynamic(Eav.Interfaces.IEntity entity);

        /// <summary>
        /// Makes sure a dynamicEntity could be wrapped in AsDynamic()
        /// </summary>
        /// <param name="dynamicEntity"></param>
        /// <returns></returns>
        dynamic AsDynamic(dynamic dynamicEntity);

        /// <summary>
        /// Returns the value of a KeyValuePair as DynamicEntity
        /// </summary>
        /// <param name="entityKeyValuePair"></param>
        /// <returns></returns>
        dynamic AsDynamic(KeyValuePair<int, Eav.Interfaces.IEntity> entityKeyValuePair);

        /// <summary>
        /// In case AsDynamic is used with Data["name"]
        /// </summary>
        /// <returns></returns>
        IEnumerable<dynamic> AsDynamic(IDataStream stream);

        /// <summary>
        /// Transform a DynamicEntity dynamic object back to a IEntity instance
        /// </summary>
        /// <param name="dynamicEntity"></param>
        /// <returns></returns>
        Eav.Interfaces.IEntity AsEntity(dynamic dynamicEntity);

        /// <summary>
        /// Returns a list of DynamicEntities
        /// </summary>
        /// <param name="entities">List of entities</param>
        /// <returns></returns>
        IEnumerable<dynamic> AsDynamic(IEnumerable<Eav.Interfaces.IEntity> entities);

        #region Create Data Sources
        /// <summary>
        /// Create a source with initial stream to attach...
        /// </summary>
        /// <returns></returns>
        T CreateSource<T>(IDataStream inStream);
        IDataSource CreateSource(string typeName = "", IDataSource inSource = null, IValueCollectionProvider configurationProvider = null);
        T CreateSource<T>(IDataSource inSource = null, IValueCollectionProvider configurationProvider = null);
        #endregion

    }
}