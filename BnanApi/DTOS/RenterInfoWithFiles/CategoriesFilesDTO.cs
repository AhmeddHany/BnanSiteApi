namespace BnanApi.DTOS.RenterInfoWithFiles
{
    public class CategoriesFilesDTO
    {
        public string? Id { get; set; }
        public string? ArName { get; set; }
        public string? EnName { get; set; }
        public List<FilesDTO>? Files { get; set; }
    }
}
