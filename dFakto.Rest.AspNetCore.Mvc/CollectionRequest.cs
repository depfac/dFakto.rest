namespace dFakto.Rest.AspNetCore.Mvc
{
    public enum SortOrder
    {
        Asc,
        Desc
    }

    public class CollectionRequest
    {
        public SortOrder Order { get; set; } = SortOrder.Asc;
        public string Sort { get; set; } = string.Empty;
        public int Index { get; set; } = 0;
        public int Limit { get; set; } = 10;
    }
}