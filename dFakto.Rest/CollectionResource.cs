using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.WebUtilities;

namespace dFakto.Rest
{
    public class CollectionResource : Resource
    {
        public CollectionResource(string self, 
                                  CollectionRequest request, 
                                  int total, 
                                  IEnumerable<Resource> result) : base(self)
        {
            Add(request);
            Add("total", total);
            
            Dictionary<string,string> queryParams = new Dictionary<string, string>();
            queryParams.Add("index",request.Index.ToString());
            queryParams.Add("limit",request.Limit.ToString());
            if (!string.IsNullOrWhiteSpace(request.Sort))
            {
                queryParams.Add("order", request.Order.ToString());
                queryParams.Add("sort", request.Sort);
            }

            if (request.Index > 0)
            {
                var prevIndex = request.Index - request.Limit;
                queryParams["index"] = (prevIndex < 0 ? 0 : prevIndex).ToString();
                var prevUri = QueryHelpers.AddQueryString(self.ToString(), queryParams);
                AddLink("prev",prevUri);
            }

            if (request.Index + request.Limit < total)
            {
                var nextIndex = request.Index + request.Limit;
                queryParams["index"] = (nextIndex > total ? total : nextIndex).ToString();
                var nextUri = QueryHelpers.AddQueryString(self.ToString(), queryParams);
                AddLink("next", nextUri);
            }

            int lastFragmentIndex = self.LastIndexOf('/');
            if(lastFragmentIndex < 0)
                throw new RestException("Invalid Uri specified");
            
            AddEmbedded(self.Substring(lastFragmentIndex+1), result);
        }
    }
}