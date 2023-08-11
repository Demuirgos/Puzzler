using SixLabors.ImageSharp;

[Flags]
public enum PieceType {
    None = 0,
    MLeft = 1 << 0,
    PLeft = 1 << 1, 
    MRight = 1 << 2, 
    PRight = 1 << 3, 
    MTop = 1 << 4, 
    PTop = 1 << 5, 
    MBottom = 1 << 6, 
    PBottom = 1 << 7
}

[Flags]
public enum Position {
    None = 0,
    Top = 1 << 0, 
    Left = 1 << 1,
    Bottom = 1 << 2,
    Right = 1 << 3, 
    Middle = 1 << 4
}

public record struct Piece(PieceType Type, Position Position);

public static class PieceExt {

    public static Image<Rgba32> CropPiece(this Image<Rgba32> image) {
        var h = image.Height;
        var w = image.Width;
        int firstBitH = int.MaxValue, lastBitH = 0;
        int firstBitV = int.MaxValue, lastBitV = 0;
        for(int i = 0; i < h; i++) {
            for(int j = 0; j < w; j++) {
                if(image[j, i].A != 0) {
                    firstBitH = Math.Min(firstBitH, i);
                    lastBitH = Math.Max(lastBitH, i);
                    firstBitV = Math.Min(firstBitV, j);
                    lastBitV = Math.Max(lastBitV, j);
                }
            }
        }
        return image.Clone(x => x.Crop(new Rectangle(firstBitV, firstBitH, lastBitV - firstBitV, lastBitH - firstBitH)));
    }

    public static Image<Rgba32> Multiply(Image<Rgba32> lhs, Image<Rgba32> rhs) {
        var result = new Image<Rgba32>(lhs.Width, lhs.Height);
        for (int y = 0; y < lhs.Height; y++) {
            for (int x = 0; x < lhs.Width; x++) {
                var l = lhs[x, y];
                var r = rhs[x, y];
                result[x, y] = new Rgba32((byte)(l.R * r.R / 255), (byte)(l.G * r.G / 255), (byte)(l.B * r.B / 255), (byte)(l.A * r.A / 255));
            }
        }
        return result;
    }

    public static PieceType Negative(this PieceType piece, PieceType @default = PieceType.None) => piece switch {
        PieceType.PLeft => PieceType.MLeft,
        PieceType.MLeft => PieceType.PLeft,
        PieceType.PRight => PieceType.MRight,
        PieceType.MRight => PieceType.PRight,
        PieceType.PTop => PieceType.MTop,
        PieceType.MTop => PieceType.PTop,
        PieceType.PBottom => PieceType.MBottom,
        PieceType.MBottom => PieceType.PBottom,
        _ => @default
    };

    public static PieceType Opposite(this PieceType piece) => piece switch {
        PieceType.PLeft => PieceType.PRight,
        PieceType.MLeft => PieceType.MRight,
        PieceType.PRight => PieceType.PLeft,
        PieceType.MRight => PieceType.MLeft,
        PieceType.PTop => PieceType.PBottom,
        PieceType.MTop => PieceType.MBottom,
        PieceType.PBottom => PieceType.PTop,
        PieceType.MBottom => PieceType.MTop,
        _ => PieceType.None
    };

    public static PieceType TypePerSide(this Piece piece, Position side) => side switch {
        Position.Left => piece.Type.HasFlag(PieceType.PLeft) ? PieceType.PLeft : piece.Type.HasFlag(PieceType.MLeft) ? PieceType.MLeft : PieceType.None,
        Position.Right => piece.Type.HasFlag(PieceType.PRight) ? PieceType.PRight : piece.Type.HasFlag(PieceType.MRight) ? PieceType.MRight : PieceType.None,
        Position.Top => piece.Type.HasFlag(PieceType.PTop) ? PieceType.PTop : piece.Type.HasFlag(PieceType.MTop) ? PieceType.MTop : PieceType.None,
        Position.Bottom => piece.Type.HasFlag(PieceType.PBottom) ? PieceType.PBottom : piece.Type.HasFlag(PieceType.MBottom) ? PieceType.MBottom : PieceType.None,
        _ => PieceType.None
    };
    
    public static bool ValidatePiece(this Piece piece) {
        if(piece.Position.HasFlag(Position.Top) &&  (piece.Type.HasFlag(PieceType.PTop) || piece.Type.HasFlag(PieceType.MTop))) return false;
        if(piece.Position.HasFlag(Position.Bottom) &&  (piece.Type.HasFlag(PieceType.PBottom) || piece.Type.HasFlag(PieceType.MBottom))) return false;
        if(piece.Position.HasFlag(Position.Left) &&  (piece.Type.HasFlag(PieceType.PLeft) || piece.Type.HasFlag(PieceType.MLeft))) return false;
        if(piece.Position.HasFlag(Position.Right) &&  (piece.Type.HasFlag(PieceType.PRight) || piece.Type.HasFlag(PieceType.MRight))) return false;
        if((piece.Position.HasFlag(Position.Left) && piece.Position.HasFlag(Position.Top)) &&  (piece.Type.HasFlag(PieceType.PLeft) || piece.Type.HasFlag(PieceType.PTop) || piece.Type.HasFlag(PieceType.MLeft) || piece.Type.HasFlag(PieceType.MTop))) return false;
        if((piece.Position.HasFlag(Position.Left) && piece.Position.HasFlag(Position.Bottom)) &&  (piece.Type.HasFlag(PieceType.PLeft) || piece.Type.HasFlag(PieceType.PBottom) || piece.Type.HasFlag(PieceType.MLeft) || piece.Type.HasFlag(PieceType.MBottom))) return false;
        if((piece.Position.HasFlag(Position.Right) && piece.Position.HasFlag(Position.Top)) &&  (piece.Type.HasFlag(PieceType.PRight) || piece.Type.HasFlag(PieceType.PTop) || piece.Type.HasFlag(PieceType.MRight) || piece.Type.HasFlag(PieceType.MTop))) return false;
        if((piece.Position.HasFlag(Position.Right) && piece.Position.HasFlag(Position.Bottom)) &&  (piece.Type.HasFlag(PieceType.PRight) || piece.Type.HasFlag(PieceType.PBottom) || piece.Type.HasFlag(PieceType.MRight) || piece.Type.HasFlag(PieceType.MBottom))) return false;
        return true;
    }

    public static Image<Rgba32>? GenerateMask(this Piece piece, int width, int height, int verticalCount, int horizontalCount, int horizontalIndex, int verticalIndex) {
        Console.WriteLine($"{width} {height} {horizontalCount} {verticalCount} {horizontalIndex} {verticalIndex}");
        var mask = new byte[width , height];
        for(int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++) {
                mask[i, j] = 0;
            }
        }

        var pieceWidth = width / horizontalCount;
        var pieceHeight = height / verticalCount;
        var pieceX = pieceWidth * horizontalIndex;
        var pieceY = pieceHeight * verticalIndex;

        Console.WriteLine($"{pieceWidth} {pieceHeight} {pieceX} {pieceY}");
        for(int i = pieceX; i < pieceX + pieceWidth; i++) {
            for(int j = pieceY; j < pieceY + pieceHeight; j++) {
                mask[i, j] = 255;
            }
        }

        void SetCircleAt(int x, int y, int radius, byte value) {
            for(int i = x - radius; i < x + radius; i++) {
                for(int j = y - radius; j < y + radius; j++) {
                    if(i < 0 || i >= width || j < 0 || j >= height) continue;
                    if(Math.Sqrt(Math.Pow(i - x, 2) + Math.Pow(j - y, 2)) <= radius) {
                        mask[i, j] = value;
                    }
                }
            }
        }


        int radius = Math.Min(pieceWidth, pieceHeight) / 4;
        for(int pos = 1; pos < 1 << 4; pos<<=1) {
            Position position = (Position)pos;
            switch (position)
            {
                case Position.Top: {
                    PieceType type = piece.TypePerSide(Position.Top);
                    switch (type)
                    {
                        case PieceType.PTop:
                            SetCircleAt(pieceX + pieceWidth / 2, pieceY , radius, 255);
                            break;
                        case PieceType.MTop:
                            SetCircleAt(pieceX + pieceWidth / 2 , pieceY , radius, 0);
                            break;
                    }
                    break;
                }
                case Position.Bottom: {
                    PieceType type = piece.TypePerSide(Position.Bottom);
                    switch (type)
                    {
                        case PieceType.PBottom:
                            SetCircleAt(pieceX + pieceWidth / 2, pieceY + pieceHeight, radius, 255);
                            break;
                        case PieceType.MBottom:
                            SetCircleAt(pieceX + pieceWidth / 2, pieceY + pieceHeight, radius, 0);
                            break;
                    }
                    break;
                }
                case Position.Left: {
                    PieceType type = piece.TypePerSide(Position.Left);
                    switch (type)
                    {
                        case PieceType.PLeft:
                            SetCircleAt(pieceX, pieceY + pieceHeight / 2, radius, 255);
                            break;
                        case PieceType.MLeft:
                            SetCircleAt(pieceX, pieceY + pieceHeight / 2, radius, 0);
                            break;
                    }
                    break;
                }
                case Position.Right: {
                    PieceType type = piece.TypePerSide(Position.Right);
                    switch (type)
                    {
                        case PieceType.PRight:
                            SetCircleAt(pieceX + pieceWidth, pieceY + pieceHeight / 2, radius, 255);
                            break;
                        case PieceType.MRight:
                            SetCircleAt(pieceX + pieceWidth, pieceY + pieceHeight / 2, radius, 0);
                            break;
                    }
                    break;
                }
            }
        }

        // make image from mask using SixLabors.ImageSharp
        Image<Rgba32>? image = new Image<Rgba32>(width, height);
        for(int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++) {
                image[i, j] = new Rgba32(mask[i, j], mask[i, j], mask[i, j], mask[i, j]);
            }
        }
        return image;
    }


    public static Image<Rgba32>[,] Generate(int height, int width, int verticalCount, int horizontalCount) {
        Piece[,]? result = new Piece[verticalCount, horizontalCount];
        Image<Rgba32>?[,]? masks = new Image<Rgba32>[verticalCount , horizontalCount];
        var random = new Random();

        for(int i = 0; i < verticalCount; i++) {
            for(int j = 0; j < horizontalCount; j++) {
                var piece = new Piece(PieceType.None, Position.Middle);
                piece.Position = (i, j) switch {
                    (0, 0) => Position.Left | Position.Top,
                    (0, _) when j != horizontalCount-1 => Position.Top,
                    (0, _) when j == horizontalCount-1 => Position.Right | Position.Top,
                    (_, 0) when i == verticalCount-1 => Position.Left | Position.Bottom,
                    (_, 0) => Position.Left,
                    (_, _) when i == verticalCount-1 && j == horizontalCount-1 => Position.Bottom | Position.Right,
                    (_, _) when j == horizontalCount-1 => Position.Right,
                    (_, _) when i == verticalCount-1 => Position.Bottom,
                    (_, _) => Position.Middle
                };
                piece.Type = PieceType.None;
                switch (piece.Position)
                {
                    case Position.Top | Position.Left: {
                        int rightIndex = j + 1;
                        int bottomIndex = i + 1;
                        var rightSide = result[i, rightIndex].TypePerSide(Position.Left).Opposite().Negative(random.Next(0, 2) == 0 ? PieceType.PRight : PieceType.MRight);
                        var bottomSide = result[bottomIndex, j].TypePerSide(Position.Top).Opposite().Negative(random.Next(0, 2) == 0 ? PieceType.PBottom : PieceType.MBottom);
                        piece.Type = rightSide | bottomSide;
                        break;
                    }
                    case Position.Top | Position.Right : {
                        int leftIndex = j - 1;
                        int bottomIndex = i + 1;
                        var leftSide = result[i, leftIndex].TypePerSide(Position.Right).Opposite().Negative(random.Next(0, 2) == 0 ? PieceType.PLeft : PieceType.MLeft);
                        var bottomSide = result[bottomIndex, j].TypePerSide(Position.Top).Opposite().Negative(random.Next(0, 2) == 0 ? PieceType.PBottom : PieceType.MBottom);
                        piece.Type = leftSide | bottomSide;
                        break;
                    }
                    case Position.Bottom | Position.Left : {
                        int rightIndex = j + 1;
                        int topIndex = i - 1;
                        var rightSide = result[i, rightIndex].TypePerSide(Position.Left).Opposite().Negative(random.Next(0, 2) == 0 ? PieceType.PRight : PieceType.MRight);
                        var topSide = result[topIndex, j].TypePerSide(Position.Bottom).Opposite().Negative(random.Next(0, 2) == 0 ? PieceType.PTop : PieceType.MTop);
                        piece.Type = rightSide | topSide;
                        break;
                    }
                    case Position.Bottom | Position.Right : {
                        int leftIndex = j - 1;
                        int topIndex = i - 1;
                        var leftSide = result[i, leftIndex].TypePerSide(Position.Right).Opposite().Negative(random.Next(0, 2) == 0 ? PieceType.PLeft : PieceType.MLeft);
                        var topSide = result[topIndex, j].TypePerSide(Position.Bottom).Opposite().Negative(random.Next(0, 2) == 0 ? PieceType.PTop : PieceType.MTop);
                        piece.Type = leftSide | topSide;
                        break;
                    }
                    case Position.Top : {
                        int leftIndex = j - 1;
                        int rightIndex = j + 1;
                        int bottomIndex = i + 1;
                        var leftSide = result[i, leftIndex].TypePerSide(Position.Right).Opposite().Negative(random.Next(0, 2) == 0 ? PieceType.PLeft : PieceType.MLeft);
                        var rightSide = result[i, rightIndex].TypePerSide(Position.Left).Opposite().Negative(random.Next(0, 2) == 0 ? PieceType.PRight : PieceType.MRight);
                        var bottomSide = result[bottomIndex, j].TypePerSide(Position.Top).Opposite().Negative(random.Next(0, 2) == 0 ? PieceType.PBottom : PieceType.MBottom);
                        piece.Type = leftSide | rightSide | bottomSide;
                        break;
                    }
                    case Position.Bottom : {
                        int leftIndex = j - 1;
                        int rightIndex = j + 1;
                        int topIndex = i - 1;
                        var leftSide = result[i, leftIndex].TypePerSide(Position.Right).Opposite().Negative(random.Next(0, 2) == 0 ? PieceType.PLeft : PieceType.MLeft);
                        var rightSide = result[i, rightIndex].TypePerSide(Position.Left).Opposite().Negative(random.Next(0, 2) == 0 ? PieceType.PRight : PieceType.MRight);
                        var topSide = result[topIndex, j].TypePerSide(Position.Bottom).Opposite().Negative(random.Next(0, 2) == 0 ? PieceType.PTop : PieceType.MTop);
                        piece.Type = leftSide | rightSide | topSide;
                        break;
                    }
                    case Position.Left : {
                        int rightIndex = j + 1;
                        int topIndex = i - 1;
                        int bottomIndex = i + 1;
                        var rightSide = result[i, rightIndex].TypePerSide(Position.Left).Opposite().Negative(random.Next(0, 2) == 0 ? PieceType.PRight : PieceType.MRight);
                        var topSide = result[topIndex, j].TypePerSide(Position.Bottom).Opposite().Negative(random.Next(0, 2) == 0 ? PieceType.PTop : PieceType.MTop);
                        var bottomSide = result[bottomIndex, j].TypePerSide(Position.Top).Opposite().Negative(random.Next(0, 2) == 0 ? PieceType.PBottom : PieceType.MBottom);
                        piece.Type = rightSide | topSide | bottomSide;
                        break;
                    }
                    case Position.Right : {
                        int leftIndex = j - 1;
                        int topIndex = i - 1;
                        int bottomIndex = i + 1;
                        var leftSide = result[i, leftIndex].TypePerSide(Position.Right).Opposite().Negative(random.Next(0, 2) == 0 ? PieceType.PLeft : PieceType.MLeft);
                        var topSide = result[topIndex, j].TypePerSide(Position.Bottom).Opposite().Negative(random.Next(0, 2) == 0 ? PieceType.PTop : PieceType.MTop);
                        var bottomSide = result[bottomIndex, j].TypePerSide(Position.Top).Opposite().Negative(random.Next(0, 2) == 0 ? PieceType.PBottom : PieceType.MBottom);
                        piece.Type = leftSide | topSide | bottomSide;
                        break;
                    }
                    case Position.Middle : {
                        int leftIndex = j - 1;
                        int rightIndex = j + 1;
                        int topIndex = i - 1;
                        int bottomIndex = i + 1;
                        var leftSide = result[i, leftIndex].TypePerSide(Position.Right).Opposite().Negative(random.Next(0, 2) == 0 ? PieceType.PLeft : PieceType.MLeft);
                        var rightSide = result[i, rightIndex].TypePerSide(Position.Left).Opposite().Negative(random.Next(0, 2) == 0 ? PieceType.PRight : PieceType.MRight);
                        var topSide = result[topIndex, j].TypePerSide(Position.Bottom).Opposite().Negative(random.Next(0, 2) == 0 ? PieceType.PTop : PieceType.MTop);
                        var bottomSide = result[bottomIndex, j].TypePerSide(Position.Top).Opposite().Negative(random.Next(0, 2) == 0 ? PieceType.PBottom : PieceType.MBottom);
                        piece.Type = leftSide | rightSide | topSide | bottomSide;
                        break;
                    }
                }
                result[i, j] = piece;
                masks[i, j] = piece.GenerateMask(width, height, verticalCount, horizontalCount, j, i);
            }
        }

        return masks;
    }
}

