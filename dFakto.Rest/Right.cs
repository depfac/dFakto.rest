using System;

namespace dFakto.Rest
{
    [Flags]
    public enum Right
    {
        All   = 0,
        Get    = 1,
        Put    = 2,
        Delete = 4,
        Post   = 8
    }
}