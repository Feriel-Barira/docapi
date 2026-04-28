using DocApi.DTOs;

namespace DocApi.Services.Interfaces
{
    public interface ISearchService
    {
        Task<SearchResultDto> SearchAsync(SearchRequestDto request);
    }
}
