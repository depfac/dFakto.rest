using System;

namespace dFakto.Rest.AspNetCore.Mvc;

public interface IResourceUriFactory
{
    Uri GetUriByName(string name, object? parameters = null);
}