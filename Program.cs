﻿using System;
using System.Collections.Generic;
using SharpCanvas;
using OpenTK.Input;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Pishcorky{
    public class Program{
        public static void Main(string[] args){
            Game game = new Game(1000, 1000, 30, 30);
            game.canvas.NextFrame(game.Initialize);
        }
    }

    public class Game{
        public Canvas canvas;
        public double timer = 1;
        public float squareSize;
        public byte width;
        public byte height;
        public Board board;
        public AI ai;
        public int randomStartTurns = 4;
        public bool replayMode = false;
        public bool aiEnabled = true;
        public byte playerNum = 2;
        public byte playerColor = 1;
        public bool paused = false;
        public int turn = 0;
        public int replayPos = -1;
        public string replay = "14,14|13,15|15,15|13,16|16,16|17,17|13,13|12,12|12,14|12,15|11,15|14,12|13,12|11,14|14,17|11,16|10,13|12,16|14,16|10,16|9,16|14,15|13,17|16,14|12,18|11,19|11,18|15,13|17,15|13,11|12,10|15,17|14,19|13,14|14,18|14,20|13,18|18,18|15,18|x";

        public Game(int windowWidth, int windowHeight, byte width, byte height){
            this.canvas = new Canvas(windowWidth, windowHeight);
            this.squareSize = 100.0f/width;
            this.width = width;
            this.height = height;
            this.board = new Board(width, height, this);
            this.ai = new AI(board, this);
        }

        public void Initialize(){
            canvas.updateAction = Update;
            canvas.BackgroundColor = Colorb.DarkGray;
            canvas.UpdateWhenUnfocused = true;
            canvas.ObjectMode();
            canvas.mouseActions.Add(new MouseAction(MouseButton.Left, MouseClick));
            canvas.keyboardInterruptAction = KeyboardInterrupt;
            GL.LineWidth(3);
            DrawBoard();
            if(!replayMode) replay = "";
        }

        public void DrawBoard(){
            for (int i = 0; i < width; i++){
                for (int j = 0; j < height; j++){
                    Square s = new Square(i*squareSize, j*squareSize, i*squareSize+squareSize, j*squareSize+squareSize);
                    s.setFillMode(PolygonMode.Line);
                    s.color = new Colorb(111, 144, 90);
                    canvas.Draw(s);
                }
            }
        }

        public void DrawSquare(byte x, byte y, byte color){
            if(color == 1){
                Group g = new Group(0, 0);
                Square s = new Square(squareSize/10, 7*squareSize/16, squareSize-squareSize/10, 9*squareSize/16);
                s.color = Colorb.Red;
                g.Add(s);
                s = new Square(7*squareSize/16, squareSize/10, 9*squareSize/16, squareSize-squareSize/10);
                s.color = Colorb.Red;
                g.Add(s);
                g.MoveTo(x*squareSize, y*squareSize);
                canvas.Draw(g);
            } else if(color == 2){
                Circle c = new Circle(x*squareSize + squareSize/2, y*squareSize + squareSize/2, 3*squareSize/10);
                c.color = Colorb.Blue;
                canvas.Draw(c);
            } else if(color == 255){
                CircleUnfilled c = new CircleUnfilled(x*squareSize + squareSize/2, y*squareSize + squareSize/2, squareSize/2);
                c.color = Colorb.Black;
                canvas.Draw(c);
            }
        }

        public void KeyboardInterrupt(){
            if(canvas.LastPressedChar == 'r'){
                if(paused){
                    Console.WriteLine("Restarting.");
                    Restart();
                    replay = "";
                }
            } else if(canvas.LastPressedChar == ' '){
                if(replayMode){
                    Console.WriteLine((replayPos+2) + ", " + replay.Length);
                    if(replayPos+2>=replay.Length){
                        Console.WriteLine("End of replay.");
                        return;
                    }
                    int nextPos = replay.IndexOf('|', replayPos+1);
                    string next = replay.Substring(replayPos+1, nextPos - replayPos - 1);
                    replayPos = nextPos;
                    string x = next.Split(new string[]{","}, StringSplitOptions.None)[0];
                    string y = next.Split(new string[]{","}, StringSplitOptions.None)[1];
                    board.PlaceSquare(byte.Parse(x), byte.Parse(y), board.currentColor);
                    //Console.WriteLine(x + ", " + y + " : " + nextPos);
                } else{
                    paused = !paused;
                }
            } else if(canvas.LastPressedChar == 'm'){
                replayMode = !replayMode;
            } else if(canvas.LastPressedChar == 'b'){
                if(replayMode){
                    //TODO: Implement backwards movement in replay mode.
                    /*int endPos = replayPos;
                    Restart();
                    while(replayPos != endPos){
                        int nextPos = replay.IndexOf('|', replayPos+1);
                        string next = replay.Substring(replayPos+1, nextPos - replayPos - 1);
                        replayPos = nextPos;
                        string x = next.Split(new string[]{","}, StringSplitOptions.None)[0];
                        string y = next.Split(new string[]{","}, StringSplitOptions.None)[1];
                        board.PlaceSquare(byte.Parse(x), byte.Parse(y), board.currentColor);
                    }*/
                }
            }
        }

        public void Restart(){
            canvas.ClearObjects();
            board = new Board(width, height, this);
            ai = new AI(board, this);
            paused = false;
            turn = 0;
            replayPos = 0;
            DrawBoard();
        }

        public void Update(){
            double delta = canvas.DeltaTime;
            timer -= delta;
            if(timer<0 && !paused && !replayMode){
                if(playerColor != board.currentColor && aiEnabled){
                    short move = 0;
                    if(turn > randomStartTurns-1){
                        if(board.currentColor == 1) move = ai.NextMove(2);
                        if(board.currentColor == 2) move = ai.NextMove(4);
                    } else{
                        move = ai.RandomMove();
                    }
                    board.PlaceSquare((byte) (move % width), (byte) (move / width), board.currentColor);
                }
                timer = 0.5f;
            }
        }

        public void MouseClick(){
            Vector2i mousePos = canvas.MousePosition();
            Vector2d pos = canvas.ScreenToWorldPosition(mousePos);
            byte x = (byte) Math.Floor(pos.X/squareSize);
            byte y = (byte) Math.Floor(pos.Y/squareSize);
            if(x >= width || y >= height){
                return;
            }
            Console.WriteLine(x + ", " + y + " : " + board.GetSquare(x, y));
            if(paused || replayMode || playerColor != board.currentColor && aiEnabled){
                return;
            }
            board.PlaceSquare(x, y, board.currentColor);
            Console.WriteLine("Score: " + ai.ScoreBoard(board));
        }
    }
}