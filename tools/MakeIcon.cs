using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

// Standalone .NET Framework tool (compiled directly with Roslyn, framework default refs):
// paints a 256x256 purple/gold tower icon without any external assets.
internal static class MakeIcon
{
    private static int Main(string[] args)
    {
        try
        {
            string pluginDir = args[0];
            string distIcon = args[1];
            Directory.CreateDirectory(pluginDir);
            Directory.CreateDirectory(Path.GetDirectoryName(distIcon));

            using (Bitmap bmp = new Bitmap(256, 256))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                Draw(g);
                bmp.Save(distIcon, System.Drawing.Imaging.ImageFormat.Png);
            }
            return 0;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return 1;
        }
    }

    private static readonly Color DeepBg = Color.FromArgb(18, 10, 30);
    private static readonly Color BodyDark = Color.FromArgb(52, 28, 78);
    private static readonly Color Body = Color.FromArgb(96, 56, 140);
    private static readonly Color BodyLight = Color.FromArgb(154, 108, 210);
    private static readonly Color Glow = Color.FromArgb(154, 92, 255);
    private static readonly Color Gold = Color.FromArgb(227, 179, 76);
    private static readonly Color GoldBright = Color.FromArgb(255, 224, 150);

    private static void Draw(Graphics g)
    {
        using (LinearGradientBrush bg = new LinearGradientBrush(
            new Rectangle(0, 0, 256, 256), DeepBg, Color.FromArgb(38, 20, 60), 45f))
        {
            g.FillRectangle(bg, 0, 0, 256, 256);
        }

        // soft purple halo
        using (GraphicsPath halo = new GraphicsPath())
        {
            halo.AddEllipse(38, 30, 180, 190);
            using (PathGradientBrush pb = new PathGradientBrush(halo))
            {
                pb.CenterColor = Color.FromArgb(90, Glow);
                pb.SurroundColors = new[] { Color.FromArgb(0, Glow) };
                g.FillPath(pb, halo);
            }
        }

        // base platform
        Point[] basePts =
        {
            new Point(52, 226), new Point(204, 226), new Point(182, 196), new Point(74, 196)
        };
        using (LinearGradientBrush b = new LinearGradientBrush(new Rectangle(52, 196, 152, 30), BodyLight, BodyDark, 90f))
            g.FillPolygon(b, basePts);
        g.DrawLine(new Pen(Gold, 3f), 74, 196, 182, 196);

        // body: tapered tower silhouette (PLS-like)
        Point[] bodyPts =
        {
            new Point(86, 196), new Point(170, 196),
            new Point(156, 64), new Point(100, 64)
        };
        using (LinearGradientBrush b = new LinearGradientBrush(new Rectangle(86, 60, 90, 140), BodyLight, BodyDark, 0f))
            g.FillPolygon(b, bodyPts);
        using (Pen edge = new Pen(Color.FromArgb(120, GoldBright), 2f))
            g.DrawPolygon(edge, bodyPts);

        // front facet
        Point[] facet =
        {
            new Point(110, 196), new Point(146, 196),
            new Point(140, 72), new Point(116, 72)
        };
        using (SolidBrush b = new SolidBrush(Color.FromArgb(70, 20, 8, 40)))
            g.FillPolygon(b, facet);

        // gold lamp bands
        using (SolidBrush gold = new SolidBrush(Gold))
        {
            g.FillRectangle(gold, 96, 150, 64, 6);
            g.FillRectangle(gold, 102, 104, 52, 5);
        }

        // lamps
        using (SolidBrush lamp = new SolidBrush(GoldBright))
        using (SolidBrush lampDim = new SolidBrush(Color.FromArgb(180, Gold)))
        {
            g.FillEllipse(lamp, 90, 128, 10, 10);
            g.FillEllipse(lamp, 156, 128, 10, 10);
            g.FillEllipse(lampDim, 122, 84, 12, 12);
        }

        // roof + spire
        Point[] roof =
        {
            new Point(96, 64), new Point(160, 64), new Point(128, 28)
        };
        using (LinearGradientBrush b = new LinearGradientBrush(new Rectangle(96, 28, 64, 36), BodyLight, BodyDark, 90f))
            g.FillPolygon(b, roof);
        using (Pen edge = new Pen(Gold, 2.5f))
            g.DrawLines(edge, new[] { new Point(96, 64), new Point(128, 28), new Point(160, 64) });
        using (Pen antenna = new Pen(GoldBright, 2f))
            g.DrawLine(antenna, 128, 28, 128, 8);
        using (SolidBrush tip = new SolidBrush(GoldBright))
            g.FillEllipse(tip, 123, 2, 10, 10);

        // purple glow core
        using (GraphicsPath core = new GraphicsPath())
        {
            core.AddEllipse(112, 160, 32, 32);
            using (PathGradientBrush pb = new PathGradientBrush(core))
            {
                pb.CenterColor = Color.FromArgb(220, Glow);
                pb.SurroundColors = new[] { Color.FromArgb(40, Glow) };
                g.FillPath(pb, core);
            }
        }
        using (Pen ring = new Pen(Color.FromArgb(200, Glow), 2f))
            g.DrawEllipse(ring, 110, 158, 36, 36);
    }
}
