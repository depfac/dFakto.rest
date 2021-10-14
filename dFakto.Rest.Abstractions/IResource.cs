using System;
using System.Collections.Generic;
using System.Linq;

namespace dFakto.Rest.Abstractions
{
    /// <summary>
    /// Represent a Resource according to the Hypertext Application Language
    /// </summary>
    public interface IResource
    {
        /// <summary>
        /// Return Self link if exists
        /// </summary>
        Uri? Self => Links.ContainsKey(Constants.Self) ? Links[Constants.Self].FirstOrDefault()?.Href : null;

        /// <summary>
        /// Links of the Resource
        /// </summary>
        IReadOnlyDictionary<string, IReadOnlyList<Link>> Links { get; }
        
        /// <summary>
        /// Embedded Resources
        /// </summary>
        IReadOnlyDictionary<string,  IReadOnlyList<IResource>> Embedded  { get; }
        
        /// <summary>
        /// Add Link to the resource
        /// </summary>
        /// <param name="name">Link relation type</param>
        /// <param name="links">One or more Links</param>
        /// <returns>this</returns>
        IResource AddLink(string name, params Link[] links);
        
        /// <summary>
        /// Add Link to the resource
        /// </summary>
        /// <param name="name">Link relation type</param>
        /// <param name="links">One or more Links</param>
        /// <returns>this</returns>
        IResource AddLink(string name, IEnumerable<Link> links);
        
        /// <summary>
        /// Add Embedded Resource to the resource
        /// </summary>
        /// <param name="name">Link relation type</param>
        /// <param name="embedded">One or more Resources</param>
        /// <returns>this</returns>
        IResource AddEmbedded(string name,params IResource[] embedded);
        
        /// <summary>
        /// Add Embedded Resource to the resource
        /// </summary>
        /// <param name="name">Link relation type</param>
        /// <param name="embedded">One or more Resources</param>
        /// <returns>this</returns>
        IResource AddEmbedded(string name, IEnumerable<IResource> embedded);
        
        /// <summary>
        /// Add Property values to the Resources
        /// <remarks>Property Names "_links" and "_embedded" are reserved</remarks>
        /// </summary>
        /// <param name="values">The object to add to the resource</param>
        /// <param name="onlyFields">Optional Mapping applied on the object before adding Properties</param>
        /// <typeparam name="T">Must be an object Serializable as a Json Object</typeparam>
        /// <returns>this</returns>
        IResource Add<T>(T values, Func<T,object>? onlyFields = null)  where T : class;
        
        /// <summary>
        /// Retrieve the values mapped as T
        /// </summary>
        /// <typeparam name="T">The Type</typeparam>
        /// <returns></returns>
        T? As<T>();
        
        /// <summary>
        /// Retrieve the values mappas as T. Syntax mainly used using anonymous objects
        /// <example>var d = r.Bind(new {test2 = "",Test= 0});</example>
        /// </summary>
        /// <param name="type">Anonymous Object Type</param>
        /// <typeparam name="T">The Type</typeparam>
        /// <returns></returns>
        T? Bind<T>(T type);
    }
}