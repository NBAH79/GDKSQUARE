using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Gtk;

namespace GTKDUCK
{
    class MainClass
    {
        const int MAXTHREADS = 4; //bug: position++ result is int -> out of range
        const int padding = 210;

        public enum direction
        {
            none, up, up_right, right, down_right, down, down_left, left, up_left
        }

        public class Pseudorandom
        {
            private byte[] random = new byte[ushort.MaxValue+ MAXTHREADS]; //economy and thread-safe
            /*volatile*/ private ushort position = 0; //volatile doesn't work
            public Pseudorandom()
            {
                Random r = new Random();
                for (int n = 0; n < ushort.MaxValue+ MAXTHREADS; n++) random[n] = (byte)r.Next();
            }
            public byte next { 
                get => random[(ushort)position++];
            }

        }
        public class Field
        {
            /*volatile*/ public byte[] item; //slows down 
            public readonly int width, height;
            private Pseudorandom r = new Pseudorandom();
            public readonly int ITEMS;
            private readonly int s2,s3,s4,s5; //store the parameters
            public Field(int w, int h,int p1, int p2,int p3,int p4,int p5)
            {
                ITEMS=p1;
                s2 = p2;
                s3 = p3;
                s4 = p4;
                s5 = p5;
                int len = w * h;
                item = new byte[len];
                width = w;
                height = h;
                for (int n = 0; n < len; n++) item[n] = 0;
            }

            public byte this[int x, int y] {
                get => item[y * width + x];
                set => item[y * width + x] = value;
            }

            public void SetDefault()
            {
                Random r = new Random(); //int
                for (int n = 0; n < ITEMS; n++) CreateNew();
            }

            public void CreateNew()
            {
                Random r = new Random();
                int x, y;
                do
                {
                    x = r.Next(padding, width - padding);
                    y = r.Next(padding, height - padding);
                } while (this[x, y] != 0);
                this[x, y] = 1;
            }

            public void Flatten()
            {
                for (int n = 0; n < item.Length; n++) item[n]&=1; 
                /*{
                    if (item[n] == 3) item[n] = 1;
                    if (item[n] == 2) item[n] = 0;
                };*/
            }

            public int Blocked()
            {
                int width2 = width - padding;
                int height2 = height - padding;

                int ret = 0;
                for (int x = padding; x < width2; x++)
                    for (int y = padding; y < height2; y++)
                        if (Blocked(x, y) == 9) ret++;
                return ret;
            }

            public int Blocked(int x, int y)
            {
                int count = 0;
                for (int n = -1; n < 2; n++)
                    for (int m = -1; m < 2; m++)
                        if (this[x + n, y + m] == 1) count++;
                return count;
            }

            public void Calculate(int x, int y)//, int cx, int cy)
            {
                if (this[x, y] != 1) return;
                
                direction dir = direction.none;
                int d = r.next;

                switch (d & 7)//mask 0x111
                {
                    case 0: if (this[x, y + 1] == 0) dir = direction.up; break;
                    case 1: if (this[x + 1, y + 1] == 0) dir = direction.up_right; break;
                    case 2: if (this[x + 1, y] == 0) dir = direction.right; break;
                    case 3: if (this[x + 1, y - 1] == 0) dir = direction.down_right; break;
                    case 4: if (this[x, y - 1] == 0) dir = direction.down; break;
                    case 5: if (this[x - 1, y - 1] == 0) dir = direction.down_left; break;
                    case 6: if (this[x - 1, y] == 0) dir = direction.left; break;
                    case 7: if (this[x - 1, y + 1] == 0) dir = direction.up_left; break;
                }

                if (dir == direction.none) return;

                if (d > 127)
                    switch (dir)
                    {
                        case direction.up: if (this[x, y - 1] == 1) return; break;
                        case direction.up_right: if (this[x - 1, y - 1] == 1) return; break;
                        case direction.right: if (this[x - 1, y] == 1) return; break;
                        case direction.down_right: if (this[x - 1, y + 1] == 1) return; break;
                        case direction.down: if (this[x, y + 1] == 1) return; break;
                        case direction.down_left: if (this[x + 1, y + 1] == 1) return; break;
                        case direction.left: if (this[x + 1, y] == 1) return; break;
                        case direction.up_left: if (this[x + 1, y - 1] == 1) return; break;
                    }
                if (d > 63)
                    switch (dir)
                    {
                        //case direction.up: if (this[x, y - s2] == 1) return; break;
                        case direction.up_right: if (this[x - s2, y - s2] == 1) return; break;
                        //case direction.right: if (this[x - s2, y] == 1) return; break;
                        case direction.down_right: if (this[x - s2, y + s2] == 1) return; break;
                        //case direction.down: if (this[x, y + s2] == 1) return; break;
                        case direction.down_left: if (this[x + s2, y + s2] == 1) return; break;
                        //case direction.left: if (this[x + s2, y] == 1) return; break;
                        case direction.up_left: if (this[x + s2, y - s2] == 1) return; break;
                    }
                if (d > 31)
                    switch (dir)
                    {
                        //case direction.up: if (this[x, y - s3] == 1) return; break;
                        case direction.up_right: if (this[x - s3, y - s3] == 1) return; break;
                        //case direction.right: if (this[x - s3, y] == 1) return; break;
                        case direction.down_right: if (this[x - s3, y + s3] == 1) return; break;
                        // case direction.down: if (this[x, y + s3] == 1) return; break;
                        case direction.down_left: if (this[x + s3, y + s3] == 1) return; break;
                        //case direction.left: if (this[x + s3, y] == 1) return; break;
                        case direction.up_left: if (this[x + s3, y - s3] == 1) return; break;
                    }
                if (d > 15)
                    switch (dir)
                    {
                        //case direction.up: if (this[x, y - s4] == 1) return; break;
                        case direction.up_right: if (this[x - s4, y - s4] == 1) return; break;
                        //case direction.right: if (this[x - s4, y] == 1) return; break;
                        case direction.down_right: if (this[x - s4, y + s4] == 1) return; break;
                        //case direction.down: if (this[x, y + s4] == 1) return; break;
                        case direction.down_left: if (this[x + s4, y + s4] == 1) return; break;
                        //case direction.left: if (this[x + s4, y] == 1) return; break;
                        case direction.up_left: if (this[x + s4, y - s4] == 1) return; break;
                    }
                if (d > 7)
                    switch (dir)
                    {
                        case direction.up: if (this[x, y - s5] == 1) return; break;
                        case direction.up_right: if (this[x - s5, y - s5] == 1) return; break;
                        case direction.right: if (this[x - s5, y] == 1) return; break;
                        case direction.down_right: if (this[x - s5, y + s5] == 1) return; break;
                        case direction.down: if (this[x, y + s5] == 1) return; break;
                        case direction.down_left: if (this[x + s5, y + s5] == 1) return; break;
                        case direction.left: if (this[x + s5, y] == 1) return; break;
                        case direction.up_left: if (this[x + s5, y - s5] == 1) return; break;
                    }

                switch (dir)
                {
                    case direction.up: if (y + 1 < height - padding) { this[x, y + 1] = 3; this[x, y] = 2; } break;
                    case direction.up_right: if (x + 1 < width - padding && y + 1 < height - padding) { this[x + 1, y + 1] = 3; this[x, y] = 2; } break;
                    case direction.right: if (x + 1 < width - padding) { this[x + 1, y] = 3; this[x, y] = 2; } break;
                    case direction.down_right: if (x + 1 < width - padding && y - 1 > padding) { this[x + 1, y - 1] = 3; this[x, y] = 2; } break;
                    case direction.down: if (y - 1 > padding) { this[x, y - 1] = 3; this[x, y] = 2; } break;
                    case direction.down_left: if (x - 1 > padding && y - 1 > padding) { this[x - 1, y - 1] = 3; this[x, y] = 2; } break;
                    case direction.left: if (x - 1 > padding) { this[x - 1, y] = 3; this[x, y] = 2; } break;
                    case direction.up_left: if (x - 1 > padding && y + 1 < height - padding) { this[x - 1, y + 1] = 3; this[x, y] = 2; } break;
                }

            }
        }

        public static void UpdateField()
        {
            int width2= field.width - padding + 1;
            int height2 = field.height - padding + 1;

            var t1 = Task.Factory.StartNew(() => {
                for (int x = padding; x < width2; x += 2)
                    for (int y = padding; y < height2; y += 2)
                        field.Calculate(x, y);
            });

            var t2 = Task.Factory.StartNew(() => {
                for (int x = width2;x >padding; x -= 2)
                    for (int y = height2; y>padding; y -= 2)
                        field.Calculate(x, y);
            });
            var t3 = Task.Factory.StartNew(() => {
                for (int x = width2;x>padding; x -= 2)
                    for (int y = padding; y < height2; y += 2)
                        field.Calculate(x, y);
            });
            var t4 = Task.Factory.StartNew(() => {
                for (int x = padding; x < width2; x += 2)
                    for (int y = height2; y>padding; y -= 2)
                        field.Calculate(x, y);
            });
            t1.Wait();
            t2.Wait();
            t3.Wait();
            t4.Wait();

            field.Flatten();
        }

        public static void Update(MainWindow win)
        {
            Color C0 = new Color(0, 0, 0);
            Color C1 = new Color(255, 255, 0);

            for (int x = 0; x < MainWindow.WIDTH; x++)
                for (int y = 0; y < MainWindow.HEIGHT; y++)
                {
                    if (field[x + padding, y + padding] == 1) MainWindow.page[x, y] = C1;
                    else MainWindow.page[x, y] = C0;
                }

            win.ShowPixels();

        }

        public static Field field;
        
        public static void Main(string[] args)
        {
            int items=50000;
            int p1=4;
            int p2=16;
            int p3=64;
            int p4=200;

            if (args.Length > 0) items=Convert.ToInt32(args[0]);
            if (items<100 && items>200000) items=100000;
            if (args.Length > 1) p1 = Convert.ToInt32(args[1]);
            if (p1<2 && p1>200) p1=2;
            if (args.Length > 2) p2 = Convert.ToInt32(args[2]);
            if (p2 < 2 && p2 > 200) p2 = 8;
            if (args.Length > 3) p3 = Convert.ToInt32(args[3]);
            if (p3 < 2 && p3 > 200) p3 = 133;
            if (args.Length > 4) p4 = Convert.ToInt32(args[4]);
            if (p4 < 2 && p4 > 200) p4 = 200;

            string title = "Command line arguments: "+items+"," + p1 + "," + p2 + "," + p3 + "," + p4;

            field = new Field(MainWindow.WIDTH + padding * 2, MainWindow.HEIGHT + padding * 2,items,p1,p2,p3,p4);
            field.SetDefault();

            Application.Init();
            MainWindow win = new MainWindow(title);
            
            win.ShowAll();
            Idle(ref win);
        }


        private static void Idle(ref MainWindow win)
        {
            ulong total = 0;
            int iterations = 0;
            Stopwatch time = new Stopwatch();
            time.Start();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            do
            {
                UpdateField();

                if (sw.ElapsedMilliseconds > 33)
                {
                    win.Write("Frame time " + sw.ElapsedMilliseconds + "ms. Blocked " + ((100.0f/ field.ITEMS)*field.Blocked()).ToString("F1") + "%. Iterations per frame " + iterations + "(" + total + "). Total time "+time.Elapsed.TotalSeconds.ToString("F0")+"sec" );
                    sw.Restart();
                    Update(win);
                    Application.RunIteration();
                    total += (ulong)iterations;
                    iterations = 0;
                }
                else iterations++;
            } while (!win.STOP);
            sw.Stop();
            time.Stop();

        }
    }
}
