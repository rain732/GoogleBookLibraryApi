namespace GoogleBookLibraryApi.Models;

public class BookModel
{
    public string Title { get; set; } = string.Empty;
    public string[] Authors { get; set; } = Array.Empty<string>();
    public string Publisher { get; set; } = string.Empty;
    public string PreviewLink { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
