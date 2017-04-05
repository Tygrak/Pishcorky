using System;
using System.Collections.Generic;
using SharpCanvas;

namespace Pishcorky{
    public class Board{
        private Game game;
        private byte width;
        private byte height;
        private byte[] map;
        public byte currentColor = 1;
        public byte toWin = 5;
        public byte colorWon = 0;

        public Board(byte width, byte height, Game game){
            this.width = (byte) width;
            this.height = (byte) height;
            this.map = new byte[width * height];
            this.game = game;
        }

        public Board(Board b){
            this.width = b.width;
            this.height = b.height;
            this.currentColor = b.currentColor;
            this.map = (byte[]) b.map.Clone();
        }

        public byte[] GetMap(){
            return map;
        }

        public Board QuickCopy(){
            return new Board(this);
        }

        public byte GetSquare(byte x, byte y){
            return map[x + width * y];
        }

        public bool PlaceSquare(byte x, byte y, byte color){
            if(map[x + width * y] != 0){
                return false;
            }
            map[x + width * y] = color;
            game.DrawSquare(x, y, color);
            EndTurn(x, y);
            currentColor++;
            if(currentColor>2){
                currentColor = 1;
            }
            if(game != null){
                game.turn++;
                if(!game.replayMode){
                    if(game.replayPos > 0) game.replay = game.replay.Substring(0, game.replayPos);
                    game.replay += x + "," + y + "|";
                    game.replayPos = game.replay.Length;
                }
                if(WinCheck()){
                    Console.WriteLine("Games string: " + game.replay);
                    game.DrawSquare(x, y, 255);
                    game.paused = true;
                }
            }
            return true;
        }

        public bool QuietPlaceSquare(byte x, byte y, byte color){
            if(map[x + width * y] != 0){
                return false;
            }
            map[x + width * y] = color;
            EndTurn(x, y);
            currentColor++;
            if(currentColor>2){
                currentColor = 1;
            }
            return true;
        }

        public void EndTurn(byte x, byte y){
            byte c = map[x + width * y];
            byte xPos;
            byte yPos = y;
            byte inRow = 0;
            for (xPos = x; xPos+1 < width && yPos+1 < height; xPos++){
                if(map[xPos+1 + width * (yPos+1)] != c){
                    break;
                }
                yPos++;
            }
            for (; xPos >= 0 && yPos >= 0 && xPos != 255 && yPos != 255; xPos--){
                if(map[xPos + width * yPos] == c){
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
            for (xPos = x; yPos+1 < height && xPos-1 >= 0 && xPos != 255; xPos--){
                if(map[xPos-1 + width * (yPos+1)] != c){
                    break;
                }
                yPos++;
            }
            for (; xPos < width && yPos >= 0 && yPos != 255; xPos++){
                if(map[xPos + width * yPos] == c){
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
            for (xPos = x; xPos+1 < width; xPos++){
                if(map[xPos+1 + width * yPos] != c){
                    break;
                }
            }
            for (; xPos >= 0 && xPos != 255; xPos--){
                if(map[xPos + width * yPos] == c){
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
            for (yPos = y; yPos+1 < width; yPos++){
                if(map[xPos + width * (yPos+1)] != c){
                    break;
                }
            }
            for (; yPos >= 0 && yPos != 255; yPos--){
                if(map[xPos + width * yPos] == c){
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
                if(!game.replayMode) game.replay += "x";
                Console.WriteLine("Red crosses win!");
                return true;
            } else if(colorWon == 2){
                if(!game.replayMode) game.replay += "o";
                Console.WriteLine("Blue circles win!");
                return true;
            } else{
                Console.WriteLine("Color " + colorWon + " wins!");
                return true;
            }
        }

        public bool NeighborsFilled(byte x, byte y){
            if(map[x + width * y] != 0) return false;
            if(x+1 < width && y+1 < height && map[x+1 + width * (y+1)] != 0) return true;
            if(x+1 < width && y-1 >= 0 && map[x+1 + width * (y-1)] != 0) return true;
            if(x-1 >= 0 && y-1 >= 0 && map[x-1 + width * (y-1)] != 0) return true;
            if(x-1 >= 0 && y+1 < height && map[x-1 + width * (y+1)] != 0) return true;
            if(x+1 < width && map[x+1 + width * y] != 0) return true;
            if(x-1 >= 0 && map[x-1 + width * y] != 0) return true;
            if(y+1 < height && map[x + width * (y+1)] != 0) return true;
            if(y-1 >= 0 && map[x + width * (y-1)] != 0) return true;
            return false;
        }

        public List<KeyValuePair<byte, byte>> FilledSquares(){
            List<KeyValuePair<byte, byte>> squares = new List<KeyValuePair<byte, byte>>();
            for (byte x = 0; x < width; x++){
                for (byte y = 0; y < height; y++){
                    if(map[x + width * y] != 0){
                        squares.Add(new KeyValuePair<byte, byte>(x, y));
                    }
                }
            }
            return squares;
        }

        public KeyValuePair<byte, byte>[] InterestingMoves(){
            List<KeyValuePair<byte, byte>> moves = new List<KeyValuePair<byte, byte>>();
            for (byte x = 0; x < width; x++){
                for (byte y = 0; y < height; y++){
                    if(NeighborsFilled(x, y)){
                        moves.Add(new KeyValuePair<byte, byte>(x, y));
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