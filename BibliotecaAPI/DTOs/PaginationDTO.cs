namespace BibliotecaAPI.DTOs
{
    /*Esto quiere decir que es inmutable*/
    public record PaginationDTO(int Pagina = 1, int RecordPorPagina = 10)
    {
        private const int MaxRecordForPage = 50;

        public int Pagina { get; init; } = Math.Max(1, Pagina);
        public int RecordPorPagina { get; init; } = Math.Clamp(RecordPorPagina, 1, MaxRecordForPage);

    }
}
