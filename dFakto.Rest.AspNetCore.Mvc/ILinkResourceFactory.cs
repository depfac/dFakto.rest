using System;

namespace dFakto.Rest.AspNetCore.Mvc;

public interface ILinkResourceFactory
{
    Uri GetUriByName(string name, object parameters = null);
}