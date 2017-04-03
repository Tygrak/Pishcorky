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
        public byte colorWon = 0;

        public AI(Board board, Game game){
            this.game = game;
            this.width = game.width;
            this.height = game.height;
            this.board = board;
            this.map = board.GetMap();
            this.rand = new Random();
        }

        public Byte2 RandomMove(){
            Byte2[] moves = board.InterestingMoves();
            if(moves.Length == 0){
                return new Byte2(20, 20);
            }
            return moves[rand.Next(0, moves.Length)];
        }
    }
}