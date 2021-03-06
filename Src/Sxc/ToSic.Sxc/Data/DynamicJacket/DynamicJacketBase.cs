﻿using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

namespace ToSic.Sxc.Data
{
    /// <summary>
    /// Base class for DynamicJackets. You won't use this, just included in the docs. <br/>
    /// To check if something is an array or an object, use "IsArray"
    /// </summary>
    /// <typeparam name="T">The underlying type, either a JObject or a JToken</typeparam>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("just use the objects from AsDynamic, don't use this directly")]
    public abstract class DynamicJacketBase<T>: DynamicObject, IReadOnlyList<object>, IWrapper<T>
    {
        /// <summary>
        /// The underlying data, in case it's needed for various internal operations.
        /// </summary>
        public T UnwrappedContents { get; }

        /// <summary>
        /// Check if it's an array.
        /// </summary>
        /// <returns>True if an array/list, false if an object.</returns>
        public abstract bool IsList { get; }

        /// <summary>
        /// Primary constructor expecting a internal data object
        /// </summary>
        /// <param name="originalData">the original data we're wrapping</param>
        [PrivateApi]
        protected DynamicJacketBase(T originalData) => UnwrappedContents = originalData;

        /// <summary>
        /// Enable enumeration. When going through objects (properties) it will return the keys, not the values. <br/>
        /// Use the [key] accessor to get the values as <see cref="DynamicJacketList"/> or <see cref="ToSic.Sxc"/>
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator<object> GetEnumerator();


        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// If the object is just output, it should show the underlying json string
        /// </summary>
        /// <returns>the inner json string</returns>
        public override string ToString() => UnwrappedContents.ToString();

        /// <inheritdoc />
        public int Count => ((IList) UnwrappedContents).Count;

        /// <summary>
        /// Not yet implemented accessor - must be implemented by the inheriting class.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>a <see cref="System.NotImplementedException"/></returns>
        public virtual object this[int index] => throw new System.NotImplementedException();

        /// <summary>
        /// Fake property binder - just ensure that simple properties don't cause errors. <br/>
        /// Must be overriden in inheriting objects
        /// like <see cref="DynamicJacketList"/>, <see cref="DynamicJacket"/>
        /// </summary>
        /// <param name="binder">.net binder object</param>
        /// <param name="result">always null, unless overriden</param>
        /// <returns>always returns true, to avoid errors</returns>
        [PrivateApi]
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            return true;
        }
    }
}
