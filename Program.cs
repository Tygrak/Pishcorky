using System;
using System.Collections.Generic;
using SharpCanvas;
using OpenTK.Input;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Pishcorky{
    public class Program{
        public static void Main(string[] args){
            Game game = new Game(1000, 1000, 50, 50);
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
        public bool aiEnabled = true;
        public byte currentColor = 1;
        public byte playerNum = 2;
        public byte playerColor = 0;
        public bool paused = false;

        public Game(int windowWidth, int windowHeight, byte width, byte height){
            this.canvas = new Canvas(windowWidth, windowHeight);
            this.squareSize = 100/width;
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
            DrawBoard();
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
            }
        }

        public void KeyboardInterrupt(){
            if(canvas.LastPressedChar == 'r'){
                if(paused){
                    Console.WriteLine("Restarting.");
                    canvas.ClearObjects();
                    board = new Board(width, height, this);
                    ai = new AI(board, this);
                    paused = false;
                    DrawBoard();
                }
            }
        }

        public void Update(){
            double delta = canvas.DeltaTime;
            timer -= delta;
            if(timer<0 && !paused){
                timer = 0.25f;
                if(playerColor != currentColor && aiEnabled){
                    Byte2 move = ai.RandomMove();
                    if(board.PlaceSquare(move.x, move.y, currentColor)){
                        currentColor++;
                        if(currentColor>playerNum){
                            currentColor = 1;
                        }
                    }
                }
            }
        }

        public void MouseClick(){
            Vector2i mousePos = canvas.MousePosition();
            Vector2d pos = canvas.ScreenToWorldPosition(mousePos);
            byte x = (byte) Math.Floor(pos.X/squareSize);
            byte y = (byte) Math.Floor(pos.Y/squareSize);
            Console.WriteLine(x + ", " + y + " : " + board.GetSquare(x, y));
            if(paused || playerColor != currentColor && aiEnabled || x<0 || x>=width || y<0 || y>=height){
                return;
            }
            if(board.PlaceSquare(x, y, currentColor)){
                currentColor++;
                if(currentColor>playerNum){
                    currentColor = 1;
                }
            }
        }
    }
}