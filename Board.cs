using System;
using System.Collections.Generic;
using SharpCanvas;

namespace Pishcorky{
    public class Board{
        private Game game;
        private byte width;
        private byte height;
        private byte[,] map;
        private byte toWin = 5;
        public byte colorWon = 0;

        public Board(byte width, byte height, Game game){
            this.width = (byte) width;
            this.height = (byte) height;
            this.map = new byte[width, height];
            this.game = game;
        }

        public Board(Board b){
            this.width = b.width;
            this.height = b.height;
            this.map = (byte[,]) b.map.Clone();
        }

        public byte[,] GetMap(){
            return map;
        }

        public Board QuickCopy(){
            return new Board(this);
        }

        public byte GetSquare(byte x, byte y){
            return map[x, y];
        }

        public bool PlaceSquare(byte x, byte y, byte color){
            if(map[x, y] != 0){
                return false;
            }
            map[x, y] = color;
            game.DrawSquare(x, y, color);
            EndTurn(x, y);
            if(game != null && WinCheck()){
                game.paused = true;
            }
            return true;
        }

        public void EndTurn(byte x, byte y){
            byte c = map[x, y];
            byte xPos;
            byte yPos = y;
            byte inRow = 0;
            for (xPos = x; xPos < width && yPos < height; xPos++){
                if(map[xPos+1, yPos+1] != c){
                    break;
                }
                yPos++;
            }
            for (; xPos >= 0 && yPos >= 0; xPos--){
                if(map[xPos, yPos] == c){
                    inRow++;
                    if(inRow == toWin){
                        colorWon = c;
                        return;
                    }
                } else{
                    break;
                }
                yPos--;
            }
            yPos = y;
            inRow = 0;
            for (xPos = x; yPos < height && xPos >= 0; xPos--){
                if(map[xPos-1, yPos+1] != c){
                    break;
                }
                yPos++;
            }
            for (; xPos < width && yPos >= 0; xPos++){
                if(map[xPos, yPos] == c){
                    inRow++;
                    if(inRow == toWin){
                        colorWon = c;
                        return;
                    }
                } else{
                    break;
                }
                yPos--;
            }
            yPos = y;
            inRow = 0;
            for (xPos = x; xPos < width; xPos++){
                if(map[xPos+1, yPos] != c){
                    break;
                }
            }
            for (; xPos >= 0; xPos--){
                if(map[xPos, yPos] == c){
                    inRow++;
                    if(inRow == toWin){
                        colorWon = c;
                        return;
                    }
                } else{
                    break;
                }
            }
            xPos = x;
            inRow = 0;
            for (yPos = y; yPos < width; yPos++){
                if(map[xPos, yPos+1] != c){
                    break;
                }
            }
            for (; yPos >= 0; yPos--){
                if(map[xPos, yPos] == c){
                    inRow++;
                    if(inRow == toWin){
                        colorWon = c;
                        return;
                    }
                } else{
                    break;
                }
            }
        }

        public bool WinCheck(){
            if(colorWon == 0){
                return false;
            } else if(colorWon == 1){
                Console.WriteLine("Red crosses win!");
                return true;
            } else if(colorWon == 2){
                Console.WriteLine("Blue circles win!");
                return true;
            } else{
                Console.WriteLine("Color " + colorWon + " wins!");
                return true;
            }
        }

        public bool NeighborsFilled(byte x, byte y){
            if(map[x, y] != 0) return false;
            if(x+1 < width && y+1 < height && map[x+1, y+1] != 0) return true;
            if(x+1 < width && y-1 >= 0 && map[x+1, y-1] != 0) return true;
            if(x-1 >= 0 && y-1 >= 0 && map[x-1, y-1] != 0) return true;
            if(x-1 >= 0 && y+1 < height && map[x-1, y+1] != 0) return true;
            if(x+1 < width && map[x+1, y] != 0) return true;
            if(x-1 >= 0 && map[x-1, y] != 0) return true;
            if(y+1 < height && map[x, y+1] != 0) return true;
            if(y-1 >= 0 && map[x, y-1] != 0) return true;
            return false;
        }

        public Byte2[] InterestingMoves(){
            List<Byte2> moves = new List<Byte2>();
            for (byte x = 0; x < width; x++){
                for (byte y = 0; y < height; y++){
                    if(NeighborsFilled(x, y)){
                        moves.Add(new Byte2(x, y));
                    }
                }
            }
            return moves.ToArray();
        }
    }

    public struct Byte2{
        public byte x;
        public byte y;
        public Byte2(byte x, byte y){
            this.x = x;
            this.y = y;
        }
    }
}