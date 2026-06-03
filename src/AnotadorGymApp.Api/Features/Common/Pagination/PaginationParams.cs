namespace AnotadorGymAppApi.Features.Common.Pagination
{
    public class PaginationParams
    {
        private const int MaxPageSize = 50;
        public int Page { get; set; } = 1;
        public string Nombre { get; set; } = string.Empty;

        private int pageSize = 10;
        public int PageSize
        {
            get => pageSize;
            set => pageSize = value > MaxPageSize ? MaxPageSize : value;
        }       
    }
}
