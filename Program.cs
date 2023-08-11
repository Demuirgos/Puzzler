using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;


var inPath = args[0];
var wShard = int.Parse(args[1]);
var hShard = int.Parse(args[2]);
var outPath = args[3];
var inStream = File.OpenRead(inPath);

using (Image<Rgba32>image = Image<Rgba32>.Load<Rgba32>(inStream))
{
    var pieces = PieceExt.Generate(image.Height, image.Width, hShard, wShard);
    Console.WriteLine($"Pieces: {pieces.Length}");
    for(int i = 0; i < hShard; i++)
    {
        for(int j = 0; j < wShard; j++)
        {
            var piece = PieceExt.Multiply(image, pieces[i, j]).CropPiece();
            if(!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }

            var outStream = !File.Exists($"{outPath}/piece_{i}_{j}.png") 
                ? File.OpenWrite($"{outPath}/piece_{i}_{j}.png")
                : File.Create($"{outPath}/piece_{i}_{j}.png");
            piece.SaveAsPng(outStream);
        }
    }
}