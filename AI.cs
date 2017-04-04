using System;
using System.Collections.Generic;
using SharpCanvas;

namespace Pishcorky{
    public class AI{
        private Game game;
        private byte width;
        private byte height;
        private Board board;
        private byte[,] map;
        private byte toWin = 5;
        private Random rand;
        public short score = 0;
        public byte colorWon = 0;

        public AI(Board board, Game game){
            this.game = game;
            this.width = game.width;
            this.height = game.height;
            this.board = board;
            this.toWin = board.toWin;
            this.map = board.GetMap();
            this.rand = new Random();
        }

        public Byte2 RandomMove(){
            Byte2[] moves = board.InterestingMoves();
            if(moves.Length == 0){
                return new Byte2((byte) (width/2), (byte) (height/2));
            }
            return moves[rand.Next(0, moves.Length)];
        }

        public Byte2 GreedyNextMove(){
            Byte2[] moves = board.InterestingMoves();
            if(moves.Length == 0){
                return new Byte2((byte) (width/2), (byte) (height/2));
            }
            Board bCopy = board.QuickCopy();
            int bestMoveId = 0;
            bCopy.QuietPlaceSquare(moves[0].x, moves[0].y, bCopy.currentColor);
            int bestMoveValue = ScoreBoard(bCopy);
            for(int i = 1; i < moves.Length; i++){
                bCopy = board.QuickCopy();
                bCopy.QuietPlaceSquare(moves[i].x, moves[i].y, bCopy.currentColor);
                int val = ScoreBoard(bCopy);
                Console.WriteLine(moves[i].x + ", " + moves[i].y + " : " + val);
                if(board.currentColor == 1 && val > bestMoveValue){
                    bestMoveId = i;
                    bestMoveValue = val;
                } else if(board.currentColor == 2 && val < bestMoveValue){
                    bestMoveId = i;
                    bestMoveValue = val;
                }
            }
            Console.WriteLine("Best Move: " + moves[bestMoveId].x + ", " + moves[bestMoveId].y + " value: " + bestMoveValue);
            return moves[bestMoveId];
        }

        public Byte2 NextMove(byte depth){
            depth--;
            Byte2[] moves = board.InterestingMoves();
            if(moves.Length == 0){
                return new Byte2((byte) (width/2), (byte) (height/2));
            }
            Board bCopy = board.QuickCopy();
            int bestMoveId = 0;
            bCopy.QuietPlaceSquare(moves[0].x, moves[0].y, bCopy.currentColor);
            int bestMoveValue = AlphaBeta(int.MinValue, int.MaxValue, bCopy, depth);
            for(int i = 1; i < moves.Length; i++){
                bCopy = board.QuickCopy();
                bCopy.QuietPlaceSquare(moves[i].x, moves[i].y, bCopy.currentColor);
                int val = AlphaBeta(int.MinValue, int.MaxValue, bCopy, depth);
                Console.WriteLine(moves[i].x + ", " + moves[i].y + " : " + val);
                if(val < bestMoveValue){
                    bestMoveId = i;
                    bestMoveValue = val;
                }
            }
            Console.WriteLine("Best Move: " + moves[bestMoveId].x + ", " + moves[bestMoveId].y + " value: " + bestMoveValue);
            return moves[bestMoveId];
        }

        public int AlphaBeta(int alpha, int beta, Board b, int depth){
            Board bCopy = b;
            if(depth == 0) return RelativeScoreBoard(bCopy);
            Byte2[] moves = bCopy.InterestingMoves();
            for(int i = 0; i < moves.Length; i++){
                bCopy = b.QuickCopy();
                bCopy.QuietPlaceSquare(moves[i].x, moves[i].y, bCopy.currentColor);
                if(bCopy.colorWon == board.currentColor){
                    return 10000+depth;
                } else if(bCopy.colorWon != board.currentColor && b.colorWon != 0){
                    return -10000-depth;
                }
                int score = -AlphaBeta(-beta, -alpha, bCopy, depth-1);
                //Console.WriteLine("m: " + moves[i].x + ", " + moves[i].y + " : " + score + " alp: " + alpha + " bet: " + beta);
                if(beta != -2147483648 && score >= beta) return beta;
                if(score > alpha) alpha = score;
            }
            return alpha;
        }

        public int NegaMax(Board b, int depth){
            Board bCopy = b;
            if(depth == 0) return RelativeScoreBoard(bCopy);
            int max = int.MinValue;
            int score = int.MinValue;
            Byte2[] moves = bCopy.InterestingMoves();
            for(int i = 0; i < moves.Length; i++){
                bCopy = b.QuickCopy();
                bCopy.QuietPlaceSquare(moves[i].x, moves[i].y, bCopy.currentColor);
                if(bCopy.colorWon == board.currentColor){
                    return 10000;
                } else if(bCopy.colorWon != board.currentColor && b.colorWon != 0){
                    return -10000;
                }
                score = -NegaMax(bCopy, depth-1);
                //Console.WriteLine("m: " + moves[i].x + ", " + moves[i].y + " : " + score);
                if(score > max) max = score;
            }
            return max;
        }

        public short ScoreBoard(Board b){
            score = 0;
            map = b.GetMap();
            if(b.colorWon == 1){
                score = 10000;
                return score;
            } else if(b.colorWon == 2){
                score = -10000;
                return score;
            }
            List<Byte2> squares = b.FilledSquares();
            for (int i = 0; i < squares.Count; i++){
                byte color = map[squares[i].x, squares[i].y];
                byte xPos = squares[i].x;
                byte yPos = squares[i].y;
                int row = InRow(xPos, yPos, 1, 1, squares);
                score += row > 1 ? color == 1 ? (short)(row-1) : (short)(-row+1) : (short) 0;
                row = InRow(xPos, yPos, 1, 0, squares);
                score += row > 1 ? color == 1 ? (short)(row-1) : (short)(-row+1) : (short) 0;
                row = InRow(xPos, yPos, 1, -1, squares);
                score += row > 1 ? color == 1 ? (short)(row-1) : (short)(-row+1) : (short) 0;
            }
            return score;
        }

        public short RelativeScoreBoard(Board b){
            score = 0;
            map = b.GetMap();
            if(b.colorWon == board.currentColor){
                score = 10000;
                return score;
            } else if(b.colorWon != board.currentColor && b.colorWon != 0){
                score = -10000;
                return score;
            }
            List<Byte2> squares = b.FilledSquares();
            for (int i = 0; i < squares.Count; i++){
                byte color = map[squares[i].x, squares[i].y];
                byte xPos = squares[i].x;
                byte yPos = squares[i].y;
                int row = InRow(xPos, yPos, 1, 1, squares);
                score += row > 1 ? color == board.currentColor ? (short)(row-1) : (short)(-row+1) : (short) 0;
                row = InRow(xPos, yPos, 1, 0, squares);
                score += row > 1 ? color == board.currentColor ? (short)(row-1) : (short)(-row+1) : (short) 0;
                row = InRow(xPos, yPos, 1, -1, squares);
                score += row > 1 ? color == board.currentColor ? (short)(row-1) : (short)(-row+1) : (short) 0;
            }
            return score;
        }

        public int InRow(byte x, byte y, int rayX, int rayY, List<Byte2> squares){
            Byte2 square = squares.Find(t => t.x == x && t.y == y);
            if(square.Equals(null)){
                return 0;
            }
            byte color = map[square.x, square.y];
            byte xPos = square.x;
            byte yPos = square.y;
            int inRow = 1;
            int blocks = 0;
            while(xPos+1 < width && yPos+1 < height && xPos-1 >= 0 && yPos-1 >= 0 && xPos != 255 && yPos != 255){
                int id = squares.IndexOf(new Byte2((byte) (xPos + rayX), (byte) (yPos + rayY)));
                if(id == -1){
                    break;
                } else if(map[squares[id].x, squares[id].y] != color){
                    blocks++;
                    break;
                }
                xPos = (byte) (xPos + rayX);
                yPos = (byte) (yPos + rayY);
            }
            xPos = (byte) (xPos - rayX);
            yPos = (byte) (yPos - rayY);
            while(xPos+1 < width && yPos+1 < height && xPos-1 >= 0 && yPos-1 >= 0 && xPos != 255 && yPos != 255){
                int id = squares.IndexOf(new Byte2(xPos, yPos));
                if(id == -1){
                    break;
                } else if(map[squares[id].x, squares[id].y] != color){
                    blocks++;
                    break;
                }
                inRow++;
                xPos = (byte) (xPos - rayX);
                yPos = (byte) (yPos - rayY);
            }
            if(blocks == 2 || inRow == 1 || inRow == 0){
                return 0;
            } else if(inRow == toWin-1 && blocks == 1){
                return toWin;
            } else if(inRow == toWin-1){
                return toWin*2;
            } else if(inRow == toWin-2 && blocks == 0){
                return toWin+1;
            } else if(blocks == 1){
                return (inRow+1)/2;
            }
            return inRow;
        }
    }
}