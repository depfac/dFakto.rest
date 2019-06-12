using System;

namespace dFakto.Rest
{
    public class Link
    {
        public Link(Uri href)
        {
            Href = href;
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
        public string Title { get; set; }

        /// <summary>
        ///     Its value is boolean and SHOULD be true when the Link Object's "href" property is a URI Template.
        ///     Its value SHOULD be considered false if it is undefined or any other value than true.
        /// </summary>
        public bool? Templated { get; set; }

        /// <summary>
        ///     Its value is a string used as a hint to indicate the media type expected when dereferencing the target resource.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        ///     Its presence indicates that the link is to be deprecated (i.e. removed) at a future date.
        ///     Its value is a URL that SHOULD provide further information about the deprecation.
        ///     A client SHOULD provide some notification (for example, by logging a
        ///     warning message) whenever it traverses over a link that has this
        ///     property.  The notification SHOULD include the deprecation property's
        ///     value so that a client manitainer can easily find information about the deprecation.
        /// </summary>
        public Uri Deprecation { get; set; }

        /// <summary>
        ///     Its value MAY be used as a secondary key for selecting Link Objects which share the same relation type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Its value is a string which is a URI that hints about the profile (as defined by [I-D.wilde-profile-link]) of the
        ///     target resource.
        /// </summary>
        public Uri Profile { get; set; }

        /// <summary>
        ///     Its value is a string and is intended for indicating the language of the target resource (as defined by [RFC5988]).
        /// </summary>
        public string Hreflang { get; set; }
    }
}