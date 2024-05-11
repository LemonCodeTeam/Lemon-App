using LemonLib;
using System;
using System.Buffers;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LemonApp.ControlItems
{
    public class AudioVisualizerRound:Control
    {
        static AudioVisualizerRound()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AudioVisualizerRound), new FrameworkPropertyMetadata(typeof(AudioVisualizerRound)));
        }
        public void Start()
        {
            if (_isPlaying||_mp == null||Settings.USettings.DynamicEffect!=1) return;
            _isPlaying = true;
            _startTime= DateTime.Now;
            _doubleArrayPool = ArrayPool<float>.Create();
            _pointArrayPool = ArrayPool<Point>.Create();

            if (_renderLoop != null) return;
            _renderCancel = new CancellationTokenSource();
            _renderLoop = RenderLoopAsync(_renderCancel.Token);
        }
        public void Stop()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            _doubleArrayPool.Return(_spectrumData);
            _renderCancel?.Cancel();
            _renderLoop = null;
            InvalidateVisual();//clear
        }
        #region Properties
        public MusicPlayer _mp { get; set; }=null;
        bool _isPlaying = false;
        DateTime _startTime;
        float[]? _spectrumData;
        CancellationTokenSource? _renderCancel;
        Task? _renderLoop;
        ArrayPool<float> _doubleArrayPool;
        ArrayPool<Point> _pointArrayPool;

        public Color BrushColor { get; set; } = Colors.AliceBlue;
        public int CircleStripCount { get; set; } = 64;
        public double CircleStripSpacing { get; set; } = 0.2;
        public double CircleStripRotationSpeed { get; set; } = 10;
        #endregion

        private async Task RenderLoopAsync(CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                    break;
                _spectrumData = _doubleArrayPool.Rent(CircleStripCount*8);
                _mp.GetFFTDataRef(ref _spectrumData);
                InvalidateVisual();

                await Task.Delay(8);
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (_mp == null||!_isPlaying) return;
            if(Visibility!=Visibility.Visible) return;
            TimeSpan dt=DateTime.Now-_startTime;
            var data = _spectrumData;
            DrawCircleStrips(drawingContext, data, dt.TotalSeconds, CircleStripCount);
        }

        private void DrawCircleStrips(DrawingContext drawingContext, float[] spectrumData, double time,int stripCount)
        {
            double xOffset = ActualWidth / 2;
            double yOffset = ActualHeight / 2;
            double radius = Math.Min(ActualWidth, ActualHeight)/2;//+ extraScale * bassScale;
            double spacing = CircleStripSpacing;
            double rotation = CircleStripRotationSpeed * time % 360;
            double scale = ActualWidth / 6 * 5;

            double rotationAngle = Math.PI / 180 * rotation;
            double blockWidth = Math.PI * 2 / stripCount;
            double stripWidth = blockWidth * (1 - spacing);
            Point[] points = _pointArrayPool.Rent(stripCount);

            for (int i = 0; i < stripCount; i++)
            {
                double x = blockWidth * i + rotationAngle;
                double y = spectrumData[i * spectrumData.Length / stripCount] * scale; 
                points[i] = new Point(x, y);
            }

            double maxHeight = points.Max(v => v.Y);
            double outerRadius = radius + maxHeight;

            double gradientStart = radius / outerRadius;

            var brush = new SolidColorBrush(BrushColor);

            PathGeometry pathGeometry = new PathGeometry();

            for (int i = 0; i < stripCount; i++)
            {
                Point p = points[i];
                double cosStart = Math.Cos(p.X);
                double sinStart = Math.Sin(p.X);
                double cosEnd = Math.Cos(p.X + stripWidth);
                double sinEnd = Math.Sin(p.X + stripWidth);

                Point
                    p0 = new Point(cosStart * radius + xOffset, sinStart * radius + yOffset),
                    p1 = new Point(cosEnd * radius + xOffset, sinEnd * radius + yOffset),
                    p2 = new Point(cosEnd * (radius + p.Y) + xOffset, sinEnd * (radius + p.Y) + yOffset),
                    p3 = new Point(cosStart * (radius + p.Y) + xOffset, sinStart * (radius + p.Y) + yOffset);

                pathGeometry.Figures.Add(
                    new PathFigure()
                    {
                        StartPoint = p0,
                        Segments =
                        {
                            new LineSegment() { Point = p1 },
                            new LineSegment() { Point = p2 },
                            new LineSegment() { Point = p3 },
                        },
                    });
            }

            _pointArrayPool.Return(points);
            drawingContext.DrawGeometry(brush, null, pathGeometry);
        }

    }
}
