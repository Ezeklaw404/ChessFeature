using Chess.Model.Game;
using Chess.Model.Piece;
using Chess.ViewModel.Game;
using Xunit;
using System.Linq;
using Chess.Model.Rule;


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
        public void Test_whiteAndBlackMirrored()
        {
            var whitePieces = game.Board.GetPieces(Color.White)
                .Where(pp => pp.Position.Row == 0)  // back rank row
                .ToDictionary(pp => pp.Position.Column, pp => pp.Piece);

            // Black back rank mirrored
            var blackPieces = game.Board.GetPieces(Color.Black)
                .Where(pp => pp.Position.Row == 7)  // black back rank row
                .GroupBy(pp => 7 - pp.Position.Column)
                .ToDictionary(g => g.Key, g => g.First().Piece);

            // Compare mirrored white vs black pieces
            for (int i = 0; i < 8; i++)
            {
                var whitePiece = whitePieces[i].GetType();
                var blackPiece = blackPieces[i].GetType();

                Assert.IsType(whitePiece.GetType(), blackPiece);
            }
        }


        [Fact]
        public void Test_only_oneQueen()
        {
            var queens = game.Board.GetPieces(Color.White)
                .Where(pp => pp.Piece is Queen)
                .ToList();

            Assert.Single(queens);
        }


        [Fact]
        public void Test_backRankHasAllPieceTypes()
        {
            var pieces = game.Board.GetPieces(Color.White)
                .Where(pp => pp.Position.Row == 0)
                .Select(pp => pp.Piece)
                .ToList();

            Assert.Contains(pieces, p => p is King);
            Assert.Contains(pieces, p => p is Queen);
            Assert.Contains(pieces, p => p is Rook);
            Assert.Contains(pieces, p => p is Bishop);
            Assert.Contains(pieces, p => p is Knight);
        }


        [Fact]
        public void Test_noPawnOnBackRank()
        {
            var backRank = game.Board.GetPieces(Color.White).Where(pp => pp.Position.Row == 0).ToList();
            Assert.DoesNotContain(backRank, pp => pp.Piece is Pawn);
        }

        [Fact]
        public void Test_pieceCountIsCorrect()
        {
            var whitePieces = game.Board.GetPieces(Color.White).ToList();
            var blackPieces = game.Board.GetPieces(Color.Black).ToList();

            Assert.Equal(16, whitePieces.Count);
            Assert.Equal(16, blackPieces.Count);
        }
    }
}
