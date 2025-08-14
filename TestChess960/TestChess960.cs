using Chess.Model.Game;
using Chess.Model.Piece;
using Chess.ViewModel.Game;
using Xunit;
using System.Linq;


namespace Chess.TestChess960
{

    public class TestChess960
    {
        private ChessGame game;

        public TestChess960()
        {
            // Create a Chess960 game before each test
            game = new StandardRulebook().Create960Game();
        }

        [Fact]
        public void Test_bishopOnReversedColors()
        {
            var bishops = game.Board.GetPieces(Color.White)
                .Where(pp => pp.Piece is Bishop)
                .ToList();

            // There should be exactly 2 bishops
            Assert.Equal(2, bishops.Count);

            // One bishop must be on a light square (odd column), one on dark (even column)
            Assert.Contains(bishops, b => b.Position.Column % 2 == 0);
            Assert.Contains(bishops, b => b.Position.Column % 2 == 1);
        }

        [Fact]
        public void Test_kingBetweenRooks()
        {
            var pieces = game.Board.GetPieces(Color.White)
                .OrderBy(pp => pp.Position.Column)
                .ToList();

            int kingCol = pieces.First(p => p.Piece is King).Position.Column;
            var rookCols = pieces.Where(p => p.Piece is Rook).Select(p => p.Position.Column).OrderBy(c => c).ToList();

            // King must be between the two rooks
            Assert.True(kingCol > rookCols.First() && kingCol < rookCols.Last());
        }

        [Fact]
        public void Test_whiteAndRooksMirrored()
        {
            // White back rank
            var whitePieces = game.Board.GetPieces(Color.White)
                .ToDictionary(pp => pp.Position.Column, pp => pp.Piece);

            // Black back rank (mirrored horizontally)
            var blackPieces = game.Board.GetPieces(Color.Black)
                .ToDictionary(pp => 7 - pp.Position.Column, pp => pp.Piece);

            // Compare mirrored white vs black pieces
            foreach (var col in whitePieces.Keys)
            {
                var whitePiece = whitePieces[col];
                var blackPiece = blackPieces[col];
                if (whitePiece is King || whitePiece is Queen || whitePiece is Rook || whitePiece is Bishop || whitePiece is Knight)
                {
                    Assert.IsType(whitePiece.GetType(), blackPiece);
                }
            }
        }
    }
}
    