namespace Mercato.Application.Common.Models.Pagination;

public class ProductQueryParameters
{
    private const int MaxPageSize = 50;
    private int _pageNumber = 1;
    private int _pageSize = 10;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (value < 1)
            {
                _pageSize = 10;
                return;
            }

            _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }

    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public string? SortBy { get; set; } = "id";
    public bool Descending { get; set; } = false;
}