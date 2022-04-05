using Elemental.Components;

namespace Datahub.Core.Components.Pageable;

public class PagingInfo : IPageableComponent
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public AePagination Paginator { get; set; }
}