namespace DocApi.DTOs
{
    public class SearchRequestDto
    {
        public required string Terme { get; set; }
        public string? OrganisationId { get; set; }
        public bool IncludeProcessus { get; set; } = true;
        public bool IncludeProcedures { get; set; } = true;
        public bool IncludeDocuments { get; set; } = true;
        public bool IncludeNonConformites { get; set; } = true;
        public bool IncludeIndicateurs { get; set; } = true;
    }

    public class SearchResultDto
    {
        public string Terme { get; set; } = "";
        public int TotalResultats { get; set; }
        public List<SearchItemDto> Processus { get; set; } = new();
        public List<SearchItemDto> Procedures { get; set; } = new();
        public List<SearchItemDto> Documents { get; set; } = new();
        public List<SearchItemDto> NonConformites { get; set; } = new();
        public List<SearchItemDto> Indicateurs { get; set; } = new();
    }

    public class SearchItemDto
    {
        public string Id { get; set; } = "";
        public string Type { get; set; } = "";
        public string Code { get; set; } = "";
        public string Titre { get; set; } = "";
        public string? Description { get; set; }
        public string? Statut { get; set; }
        public DateTime DateCreation { get; set; }
    }
}