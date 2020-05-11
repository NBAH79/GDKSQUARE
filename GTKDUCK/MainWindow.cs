using System.Runtime.InteropServices;
using Gtk;


[StructLayout(LayoutKind.Sequential)]
public struct Color
{
    public byte r, g, b;
    public Color(uint _r, uint _g, uint _b) { r = (byte)_r; g = (byte)_g; b = (byte)_b; }
}

[StructLayout(LayoutKind.Explicit)]
public struct Page
{
    [FieldOffset(0)]
    public Color[] color;
    [FieldOffset(0)]
    public byte[] bytes; //конвертация памяти

    [FieldOffset(8)] //for 64bit compatibility
    public readonly int stride;
    [FieldOffset(12)]
    public readonly int length; 
    public Page(int w, int h)
    {
        bytes = null;
        length = w * h;
        color = new Color[length];
        stride = w;
        for (int n = 0; n < length; n++) color[n] = new Color(0, 0, 0);
    }

    public Color this[int x, int y] {
        get => color[y * stride + x];
        set => color[y * stride + x] = value;
    }
}
public partial class MainWindow : Gtk.Window
{
    public const int WIDTH = 600;
    public const int HEIGHT = 600;
    public bool STOP = false;

    public static Page page = new Page(WIDTH, HEIGHT);

    private Gtk.Image image;
    private Gtk.Label label = new Label("...");
    private Gtk.VBox vbox = new VBox(false, 10);
    private Gtk.ScrolledWindow scroll = new Gtk.ScrolledWindow();

    public MainWindow(string Title) : base(Gtk.WindowType.Toplevel)
    {  
        SetSizeRequest(WIDTH+50, HEIGHT + 130);
        image =new Image();
        image.SetAlignment(0f,0f);
        image.SetSizeRequest(WIDTH+20, HEIGHT+20);
        label.SetSizeRequest(WIDTH, 50);
        image.SetPadding(20, 20);
        scroll.SetSizeRequest(WIDTH+20, HEIGHT+50);
        scroll.AddWithViewport(image);
        scroll.SetPolicy(Gtk.PolicyType.Never,Gtk.PolicyType.Never);
        vbox.PackStart(label, false, true, 0);
        vbox.PackStart(scroll,false, true, 0);
        Add(vbox);
        Build2(Title,WIDTH+50,HEIGHT+20);
        KeyPressEvent += OnKeyPress;
    }

    protected virtual void Build2(string Title, int width, int height)
    {
        global::Stetic.Gui.Initialize(this);
        // Widget MainWindow
        this.Name = "MainWindow";
        this.Title = global::Mono.Unix.Catalog.GetString(Title);
        this.WindowPosition = ((global::Gtk.WindowPosition)(4));
        this.BorderWidth = ((uint)(3));
        if ((this.Child != null))
        {
            this.Child.ShowAll();
        }
        this.DefaultWidth = width;
        this.DefaultHeight = height;
        this.Show();
        this.DeleteEvent += new global::Gtk.DeleteEventHandler(this.OnDeleteEvent);
    }

    public void ShowPixels()
    {
        image.Pixbuf = new Gdk.Pixbuf(page.bytes, Gdk.Colorspace.Rgb, false, 8, WIDTH, HEIGHT, WIDTH * 3);
    }

    public void Write(string text)
    {
        label.Text = text;
    }

    [GLib.ConnectBefore] //for g_signal_connect
    protected void OnKeyPress(object sender, KeyPressEventArgs args)
    {
        STOP=true;
    }
 
    [GLib.ConnectBefore] 
    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        KeyPressEvent -= OnKeyPress;
        Application.Quit();
        a.RetVal = STOP = true;
    }
}
