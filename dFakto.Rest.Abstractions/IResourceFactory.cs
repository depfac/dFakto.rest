using System;

namespace dFakto.Rest.Abstractions;

/// <summary>
/// Main interface to Create Resource and Serializer
/// </summary>
public interface IResourceFactory
{
    /// <summary>
    /// Creates a new Resource
    /// </summary>
    /// <param name="self">Self Uri</param>
    /// <returns>The IResource</returns>
    IResource Create(Uri self);
    
    /// <summary>
    /// Creates a new Resource
    /// </summary>
    /// <param name="self">Self Link</param>
    /// <returns>The IResource</returns>
    IResource Create(Link self);
        
    /// <summary>
    /// Create a new Resource Serializer
    /// </summary>
    /// <returns>The IResourceSerializer</returns>
    IResourceSerializer CreateSerializer();
}