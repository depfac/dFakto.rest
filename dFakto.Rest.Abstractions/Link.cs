using System;
using System.Collections.Generic;
using System.Net.Http;

namespace dFakto.Rest.Abstractions
{
    /// <summary>
    /// A Link Object represents a hyperlink from the containing resource to a URI.
    /// </summary>
    public class Link
    {
        public Link(string href) : this(new Uri(href))
        {
            
        }
        
        /// <summary>
        /// Create a new Link
        /// </summary>
        /// <param name="href">
        ///     Its value is either a URI [RFC3986] or a URI Template [RFC6570].
        ///     If the value is a URI Template then the Link Object SHOULD have a "templated" attribute whose value is true.
        /// </param>
        /// <param name="title">
        ///     Its value is a string and is intended for labelling the link with a human-readable identifier (as defined by
        ///     [RFC5988]).
        /// </param>
        /// <param name="type">
        ///     Its value is a string used as a hint to indicate the media type expected when dereferencing the target resource.
        /// </param>
        /// <param name="name">
        ///      Its value MAY be used as a secondary key for selecting Link Objects which share the same relation type.
        /// </param>
        /// <param name="templated">
        ///     Its value is boolean and SHOULD be true when the Link Object's "href" property is a URI Template.
        ///     Its value SHOULD be considered false if it is undefined or any other value than true.
        /// </param>
        /// <param name="deprecation">
        ///     Its presence indicates that the link is to be deprecated (i.e. removed) at a future date.
        ///     Its value is a URL that SHOULD provide further information about the deprecation.
        ///     A client SHOULD provide some notification (for example, by logging a
        ///     warning message) whenever it traverses over a link that has this
        ///     property.  The notification SHOULD include the deprecation property's
        ///     value so that a client maintainer can easily find information about the deprecation.
        /// </param>
        /// <param name="hrefLang">
        ///      Its value is a string and is intended for indicating the language of the target resource (as defined by [RFC5988]).
        /// </param>
        /// <param name="profile">
        ///     Its value is a string which is a URI that hints about the profile (as defined by [I-D.wilde-profile-link]) of the
        ///     target resource.
        /// </param>
        /// <param name="methods">
        ///     List available HTTP method for this Link.
        ///     <remarks>This is an extension to the standard to avoid an additional OPTIONS request to retrieve the Accept HTTP Header</remarks>
        /// </param>
        public Link(
            Uri href,
            string? title = null,
            string? type = null,
            string? name = null,
            bool templated = false,
            string? deprecation = null,
            string? hrefLang = null,
            string? profile = null,
            params HttpMethod[] methods)
        {
            Href = href;
            Title = title;
            Templated = templated;
            Deprecation = deprecation;
            Name = name;
            Type = type;
            Hreflang = hrefLang;
            Profile = profile;
            Methods = methods;
        }

        /// <summary>
        ///     Its value is either a URI [RFC3986] or a URI Template [RFC6570].
        ///     If the value is a URI Template then the Link Object SHOULD have a "templated" attribute whose value is true.
        /// </summary>
        public Uri Href { get; set; }

        /// <summary>
        ///     Its value is a string and is intended for labelling the link with a human-readable identifier (as defined by
        ///     [RFC5988]).
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        ///     Its value is boolean and SHOULD be true when the Link Object's "href" property is a URI Template.
        ///     Its value SHOULD be considered false if it is undefined or any other value than true.
        /// </summary>
        public bool? Templated { get; set; }

        /// <summary>
        ///     Its value is a string used as a hint to indicate the media type expected when dereferencing the target resource.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        ///     Its presence indicates that the link is to be deprecated (i.e. removed) at a future date.
        ///     Its value is a URL that SHOULD provide further information about the deprecation.
        ///     A client SHOULD provide some notification (for example, by logging a
        ///     warning message) whenever it traverses over a link that has this
        ///     property.  The notification SHOULD include the deprecation property's
        ///     value so that a client maintainer can easily find information about the deprecation.
        /// </summary>
        public string? Deprecation { get; set; }

        /// <summary>
        ///     Its value MAY be used as a secondary key for selecting Link Objects which share the same relation type.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        ///     Its value is a string which is a URI that hints about the profile (as defined by [I-D.wilde-profile-link]) of the
        ///     target resource.
        /// </summary>
        public string? Profile { get; set; }

        /// <summary>
        ///     Its value is a string and is intended for indicating the language of the target resource (as defined by [RFC5988]).
        /// </summary>
        public string? Hreflang { get; set; }
        
        /// <summary>
        /// List available HTTP method for this Link.
        /// <remarks>This is an extension to the standard to avoid an additional OPTIONS request to retrieve the Accept HTTP Header</remarks>
        /// </summary>
        public HttpMethod[] Methods { get; set; }
        
    }
}