namespace Core.DTOs;

public class PaginationDTO<T>
{
    public List<T> Data { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalElements { get; set; }
    public int TotalPages { get; set; }
}
