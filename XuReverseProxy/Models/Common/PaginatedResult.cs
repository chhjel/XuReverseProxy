using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Models.Common;

[GenerateFrontendModel]
public class PaginatedResult<TData>
{
    public int TotalItemCount { get; set; }
    public List<TData> PageItems { get; set; } = new();
}
