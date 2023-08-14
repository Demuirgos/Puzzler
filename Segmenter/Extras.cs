using System.Text;
using SixLabors.ImageSharp;
using Microsoft.JSInterop;
using System.Threading.Tasks;

public class BrowserDimension
{
    public int Width { get; set; }
    public int Height { get; set; }
}
public static class BoardExt {
    public static async Task<BrowserDimension> GetDimensions(IJSRuntime js)
    {
        return await js.InvokeAsync<BrowserDimension>("getDimensions");
    }
    public static string GetBoardId(int i, int j) => $"Board{i}{j}";
    public static string GetHexString(Image<Rgba32> img) {
        // encode image to png hex string
        Stream stream = new MemoryStream();
        img.SaveAsPng(stream);
        stream.Position = 0;
        byte[] bytes = new byte[stream.Length];
        stream.Read(bytes, 0, (int)stream.Length);
        // get png file prefix and hex string
        StringBuilder sb = new StringBuilder();
        sb.Append("data:image/png;base64,");
        sb.Append(Convert.ToBase64String(bytes));
        return sb.ToString();
    }
    
}