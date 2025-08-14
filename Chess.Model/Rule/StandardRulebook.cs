//-----------------------------------------------------------------------
// <copyright file="StandardRulebook.cs">
//     Copyright (c) Michael Szvetits. All rights reserved.
// </copyright>
// <author>Michael Szvetits</author>
//-----------------------------------------------------------------------
namespace Chess.Model.Rule
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Chess.Model.Command;
    using Chess.Model.Data;
    using Chess.Model.Game;
    using Chess.Model.Piece;
    using Chess.Model.Visitor;

    /// <summary>
    /// Represents the standard chess rulebook.
    /// </summary>
    public class StandardRulebook : IRulebook
    {
        /// <summary>
        /// Represents the check rule of a standard chess game.
        /// </summary>
        private readonly CheckRule checkRule;

        /// <summary>
        /// Represents the end rule of a standard chess game.
        /// </summary>
        private readonly EndRule endRule;

        /// <summary>
        /// Represents the movement rule of a standard chess game.
        /// </summary>
        private readonly MovementRule movementRule;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardRulebook"/> class.
        /// </summary>
        public StandardRulebook()
        {
            var threatAnalyzer = new ThreatAnalyzer();
            var castlingRule = new CastlingRule(threatAnalyzer);
            var enPassantRule = new EnPassantRule();
            var promotionRule = new PromotionRule();

            this.checkRule = new CheckRule(threatAnalyzer);
            this.movementRule = new MovementRule(castlingRule, enPassantRule, promotionRule, threatAnalyzer);
            this.endRule = new EndRule(this.checkRule, this.movementRule);
        }

        /// <summary>
        /// Creates a new chess game according to the standard rulebook.
        /// </summary>
        /// <returns>The newly created chess game.</returns>
        public ChessGame CreateGame()
        {
            IEnumerable<PlacedPiece> makeBaseLine(int row, Color color)
            {
                yield return new PlacedPiece(new Position(row, 0), new Rook(color));
                yield return new PlacedPiece(new Position(row, 1), new Knight(color));
                yield return new PlacedPiece(new Position(row, 2), new Bishop(color));
                yield return new PlacedPiece(new Position(row, 3), new Queen(color));
                yield return new PlacedPiece(new Position(row, 4), new King(color));
                yield return new PlacedPiece(new Position(row, 5), new Bishop(color));
                yield return new PlacedPiece(new Position(row, 6), new Knight(color));
                yield return new PlacedPiece(new Position(row, 7), new Rook(color));
            }

            IEnumerable<PlacedPiece> makePawns(int row, Color color) =>
                Enumerable.Range(0, 8).Select(
                    i => new PlacedPiece(new Position(row, i), new Pawn(color))
                );

            IImmutableDictionary<Position, ChessPiece> makePieces(int pawnRow, int baseRow, Color color)
            {
                var pawns = makePawns(pawnRow, color);
                var baseLine = makeBaseLine(baseRow, color);
                var pieces = baseLine.Union(pawns);
                var empty = ImmutableSortedDictionary.Create<Position, ChessPiece>(PositionComparer.DefaultComparer);
                return pieces.Aggregate(empty, (s, p) => s.Add(p.Position, p.Piece));
            }

            var whitePlayer = new Player(Color.White);
            var whitePieces = makePieces(1, 0, Color.White);
            var blackPlayer = new Player(Color.Black);
            var blackPieces = makePieces(6, 7, Color.Black);
            var board = new Board(whitePieces.AddRange(blackPieces));

            return new ChessGame(board, whitePlayer, blackPlayer);
        }

        public ChessGame Create960Game()
        {
            Random rnd = new Random();

            IEnumerable<PlacedPiece> makePawns(int row, Color color) =>
                Enumerable.Range(0, 8).Select(i => new PlacedPiece(new Position(row, i), new Pawn(color)));

            // Generate white back rank
            ChessPiece[] GenerateWhiteBackRank(Color color)
            {
                var positions = new ChessPiece[8];

                // Place bishops on opposite colors
                int darkBishop = 2 * rnd.Next(0, 4);       // dark squares: 0,2,4,6
                int lightBishop = 2 * rnd.Next(0, 4) + 1;  // light squares: 1,3,5,7
                positions[darkBishop] = new Bishop(color);
                positions[lightBishop] = new Bishop(color);

                // Remaining positions
                var remaining = Enumerable.Range(0, 8).Where(i => positions[i] == null).ToList();

                // Place queen randomly
                int queenIndex = remaining[rnd.Next(remaining.Count)];
                positions[queenIndex] = new Queen(color);
                remaining.Remove(queenIndex);

                // Place knights randomly
                int knight1Index = remaining[rnd.Next(remaining.Count)];
                positions[knight1Index] = new Knight(color);
                remaining.Remove(knight1Index);

                int knight2Index = remaining[rnd.Next(remaining.Count)];
                positions[knight2Index] = new Knight(color);
                remaining.Remove(knight2Index);

                // Two remaining spots: king between rooks
                remaining.Sort();
                positions[remaining[0]] = new Rook(color);
                positions[remaining[1]] = new King(color);
                positions[remaining[2]] = new Rook(color);

                return positions;
            }

            IEnumerable<PlacedPiece> makeBackRank(int row, ChessPiece[] backRank, Color color)
            {
                for (int i = 0; i < 8; i++)
                {
                    // Use the type of piece from backRank[i] but change color
                    var piece = ClonePieceForColor(backRank[i], color);
                    yield return new PlacedPiece(new Position(row, i), piece);
                }
            }

            // Helper to clone piece with a new color
            ChessPiece ClonePieceForColor(ChessPiece piece, Color color)
            {
                return piece switch
                {
                    King => new King(color),
                    Queen => new Queen(color),
                    Rook => new Rook(color),
                    Bishop => new Bishop(color),
                    Knight => new Knight(color),
                    Pawn => new Pawn(color),
                    _ => throw new InvalidOperationException("Unknown piece type")
                };
            }

            IImmutableDictionary<Position, ChessPiece> makePieces(int pawnRow, int baseRow, ChessPiece[] backRank, Color color)
            {
                var pawns = makePawns(pawnRow, color);
                var backPieces = makeBackRank(baseRow, backRank, color);
                var pieces = backPieces.Union(pawns);
                var empty = ImmutableSortedDictionary.Create<Position, ChessPiece>(PositionComparer.DefaultComparer);
                return pieces.Aggregate(empty, (s, p) => s.Add(p.Position, p.Piece));
            }

            // Generate white back rank
            var whiteBackRank = GenerateWhiteBackRank(Color.White);

            var whitePlayer = new Player(Color.White);
            var whitePieces = makePieces(1, 0, whiteBackRank, Color.White);

            var blackPlayer = new Player(Color.Black);
            // Mirror the white back rank for black
            var blackPieces = makePieces(6, 7, whiteBackRank, Color.Black);

            var board = new Board(whitePieces.AddRange(blackPieces));

            return new ChessGame(board, whitePlayer, blackPlayer);
        }


        /// <summary>
        /// Gets the status of a chess game, according to the standard rulebook.
        /// </summary>
        /// <param name="game">The game state to be analyzed.</param>
        /// <returns>The current status of the game.</returns>
        public Status GetStatus(ChessGame game)
        {
            return this.endRule.GetStatus(game);
        }

        /// <summary>
        /// Gets all possible updates (i.e., future game states) for a chess piece on a specified position,
        /// according to the standard rulebook.
        /// </summary>
        /// <param name="game">The current game state.</param>
        /// <param name="position">The position to be analyzed.</param>
        /// <returns>A sequence of all possible updates for a chess piece on the specified position.</returns>
        public IEnumerable<Update> GetUpdates(ChessGame game, Position position)
        {
            var piece = game.Board.GetPiece(position, game.ActivePlayer.Color);
            var updates = piece.Map(
                p =>
                {
                    var moves = this.movementRule.GetCommands(game, p);
                    var turnEnds = moves.Select(c => new SequenceCommand(c, EndTurnCommand.Instance));
                    var records = turnEnds.Select
                    (
                        c => new SequenceCommand(c, new SetLastUpdateCommand(new Update(game, c)))
                    );
                    var futures = records.Select(c => c.Execute(game).Map(g => new Update(g, c)));
                    return futures.FilterMaybes().Where
                    (
                        e => !this.checkRule.Check(e.Game, e.Game.PassivePlayer)
                    );
                }
            );

            return updates.GetOrElse(Enumerable.Empty<Update>());
        }
    }
}