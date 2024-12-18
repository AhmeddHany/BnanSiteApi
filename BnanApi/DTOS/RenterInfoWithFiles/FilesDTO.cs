namespace BnanApi.DTOS.RenterInfoWithFiles
{
    public class FilesDTO
    {
        public string? FileId { get; set; }
        public string? ArLessorName { get; set; }
        public string? EnLessorName { get; set; }
        public string? ArBranchName { get; set; }
        public string? EnBranchName { get; set; }
        public string? ArReferenceType { get; set; }
        public string? EnReferenceType { get; set; }
        public DateTime? Date { get; set; }
        public string? ArPdfPath { get; set; }
        public string? EnPdfPath { get; set; }
        public string? TGAPdfPath { get; set; }

    }
}
