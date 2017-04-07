using System;
using System.Diagnostics;
using System.Collections.Generic;
using SharpCanvas;

namespace Pishcorky{
    public class AI{
        private Game game;
        private byte width;
        private byte height;
        private Board board;
        private byte[] map;
        private byte toWin = 5;
        private Random rand;
        private int leaves = 0;
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

        public short RandomMove(){
            short[] moves = board.InterestingMoves();
            if(moves.Length == 0){
                return (short)(width/2 + width * height/2);
            }
            return moves[rand.Next(0, moves.Length)];
        }

        public short NextMove(byte depth){
            Stopwatch s = new Stopwatch();
            s.Start();
            leaves = 0;
            depth--;
            short[] moves = board.InterestingMoves();
            if(moves.Length == 0){
                return (short)(width/2 + width * height/2);
            }
            KeyValuePair<int, short> bestMove = RootAlphaBeta(int.MinValue, int.MaxValue, board, depth+1);
            Console.WriteLine("Best Move: " + bestMove.Value % width + ", " + bestMove.Value / width + " value: " + bestMove.Key);
            Console.WriteLine("Leaves: " + leaves);
            s.Stop();
            Console.WriteLine("Took: " + s.Elapsed.TotalMilliseconds + "ms");
            return bestMove.Value;
        }

        public int AlphaBeta(int alpha, int beta, Board b, int depth){
            leaves++;
            Board bCopy = b;
            if(depth == 0) return RelativeScoreBoard(bCopy);
            short[] moves = bCopy.InterestingMoves();
            for(int i = 0; i < moves.Length; i++){
                bCopy = b.QuickCopy();
                bCopy.QuietPlaceSquare((byte) (moves[i] % width), (byte) (moves[i] / width), bCopy.currentColor);
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

        public KeyValuePair<int, short> RootAlphaBeta(int alpha, int beta, Board b, int depth){
            leaves++;
            Board bCopy = b;
            short[] moves;
            if(depth < 3){
                moves = bCopy.InterestingMoves();
            } else{
                moves = RootMovesSort(bCopy);
            }
            int bestId = 0;
            for(int i = 0; i < moves.Length; i++){
                bCopy = b.QuickCopy();
                bCopy.QuietPlaceSquare((byte) (moves[i] % width), (byte) (moves[i] / width), bCopy.currentColor);
                int score = -AlphaBeta(-beta, -alpha, bCopy, depth-1);
                //Console.WriteLine(moves[i].Key + ", " + moves[i].Value + " : " + score);
                if(score > alpha){alpha = score; bestId = i;}
            }
            return new KeyValuePair<int, short>(alpha, moves[bestId]);
        }

        public short[] RootMovesSort(Board b){
            int alpha = int.MinValue;
            int beta = int.MaxValue;
            leaves++;
            Board bCopy = b;
            short[] moves = bCopy.InterestingMoves();
            List<int> movesValues = new List<int>(moves.Length);
            List<short> movesSorted = new List<short>(moves.Length);
            bCopy = b.QuickCopy();
            bCopy.QuietPlaceSquare((byte) (moves[0] % width), (byte) (moves[0] / width), bCopy.currentColor);
            int score = -AlphaBeta(-beta, -alpha, bCopy, 1);
            movesValues.Add(score);
            movesSorted.Add(moves[0]);
            for(int i = 1; i < moves.Length; i++){
                bCopy = b.QuickCopy();
                bCopy.QuietPlaceSquare((byte) (moves[i] % width), (byte) (moves[i] / width), bCopy.currentColor);
                score = -AlphaBeta(-beta, -alpha, bCopy, 1);
                for(int j = -1; j < movesValues.Count; j++){
                    if(j == movesValues.Count-1){
                        movesValues.Add(score);
                        movesSorted.Add(moves[i]);
                        break;
                    } else if(movesValues[j+1]>score){
                        movesValues.Insert(j+1, score);
                        movesSorted.Insert(j+1, moves[i]);
                        break;
                    }
                }
            }
            movesSorted.Reverse();
            return movesSorted.ToArray();
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
            List<short> squares = b.FilledSquares();
            for (int i = 0; i < squares.Count; i++){
                byte color = map[squares[i]];
                int row = InRow(squares[i], 1, 1, squares);
                score += row > 1 ? color == 1 ? (short)(row-1) : (short)(-row+1) : (short) 0;
                row = InRow(squares[i], 1, 0, squares);
                score += row > 1 ? color == 1 ? (short)(row-1) : (short)(-row+1) : (short) 0;
                row = InRow(squares[i], 1, -1, squares);
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
            List<short> squares = b.FilledSquares();
            for (int i = 0; i < squares.Count; i++){
                byte color = map[squares[i]];
                int row = InRow(squares[i], 1, 1, squares);
                score += row > 1 ? color == board.currentColor ? (short)(row-1) : (short)(-row+1) : (short) 0;
                row = InRow(squares[i], 1, 0, squares);
                score += row > 1 ? color == board.currentColor ? (short)(row-1) : (short)(-row+1) : (short) 0;
                row = InRow(squares[i], 1, -1, squares);
                score += row > 1 ? color == board.currentColor ? (short)(row-1) : (short)(-row+1) : (short) 0;
            }
            return score;
        }

        public int InRow(short pos, short rayX, short rayY, List<short> squares){
            short square = squares.Find(t => pos == t);
            if(square.Equals(null)){
                return 0;
            }
            byte color = map[square];
            short cPos = pos;
            int inRow = 1;
            int blocks = 0;
            while(cPos >= 0 && cPos < map.Length){
                int id = squares.IndexOf(cPos);
                if(id == -1){
                    break;
                } else if(map[squares[id]] != color){
                    blocks++;
                    break;
                }
                cPos += (short) (rayX + rayY*width);
            }
            cPos += (short) (-rayX - rayY*width);
            while(cPos >= 0 && cPos < map.Length){
                int id = squares.IndexOf(cPos);
                if(id == -1){
                    break;
                } else if(map[squares[id]] != color){
                    blocks++;
                    break;
                }
                inRow++;
                cPos += (short) (-rayX - rayY*width);
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