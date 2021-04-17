using In_Extremis.Editor.Gauss;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace In_Extremis.Editor {

   class Ring {
      public Point[] Points { get; set; }
      public Rect[] Rects { get; set; }
      public int Id { get; set; }
      public Brush Brush { get; set; }
      public int Mass { get; set; }
      public int Pressure { get; set; }
      public double RadiusDelta { get; set; }
      public double Radius { get => (double)Mass / (2 * Math.PI); }
   }

   public class PressureCanvas : Canvas {

      #region Dependency Properties used by Animations.
      public GradientBrush Gradient {
         get { return (GradientBrush)GetValue(GradientProperty); }
         set { SetValue(GradientProperty, value); }
      }
      public double Radius {
         get { return (double)GetValue(RadiusProperty); }
         set { SetValue(RadiusProperty, value); }
      }
      public double RadiusSquared {
         get { return Radius * Radius; }
      }
      public int Time {
         get { return (int)GetValue(TimeProperty); }
         set { SetValue(TimeProperty, value); }
      }
      public static readonly DependencyProperty GradientProperty;
      public static readonly DependencyProperty RadiusProperty;
      public static readonly DependencyProperty TimeProperty;
      static PressureCanvas() {
         var gradientMetadata = new FrameworkPropertyMetadata(OnRadiusChanged);
         var radiusMetadata = new FrameworkPropertyMetadata(OnRadiusChanged);
         var timeMetadata = new FrameworkPropertyMetadata(OnRadiusChanged);

         GradientProperty = DependencyProperty.Register("Gradient", typeof(GradientBrush), typeof(PressureCanvas), gradientMetadata);
         RadiusProperty = DependencyProperty.Register("Radius", typeof(double), typeof(PressureCanvas), radiusMetadata);
         TimeProperty = DependencyProperty.Register("Time", typeof(int), typeof(PressureCanvas), timeMetadata);
      }
      public PressureCanvas()
          : base() {
         Loaded += PressureCanvas_Loaded;
      }
      private static void OnRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
         ((PressureCanvas)d).Draw();
      }
      #endregion

      #region Visual details
      private const double scale = 10;
      private const double half_scale = scale / 2;
      private SolidColorBrush background;
      private Pen pen;

      public ObservableCollection<PrimeFactors> Factors { get; private set; }
      private Dictionary<int, Point[]> Lattice;
      private Dictionary<int, Ring> Rings;
      private const double point_radius = 1;
      private const double half_point_radius = point_radius / 2;
      private bool loaded = false;
      private void PressureCanvas_Loaded(object sender, RoutedEventArgs e) {
         Factors = new ObservableCollection<PrimeFactors>(Gaussian.Factors.Select(x => x.Value));
         Lattice = Gaussian.Lattice(scale);
         var pressureCanvas = sender as PressureCanvas;
         if(pressureCanvas != null)
            Rings = CreateRings(pressureCanvas);
         visual_registry = new Dictionary<int, Visual>(Lattice.Count);
         background = Brushes.Transparent;
         pen = new Pen(Brushes.Transparent, 1);

         loaded = true;
         Draw();
      }

      Dictionary<int, Ring> CreateRings(PressureCanvas pressureCanvas) {
         var points = new List<Point>();
         var size = (int)Math.Sqrt(Lattice.Count) + 1;
         var result = new Dictionary<int, Ring>(size);
         var i = 0;
         var ring_index = 0;
         while (i < Lattice.Count) {
            if (Math.Sqrt(i) <= ring_index) {
               points.AddRange(Lattice[i]);
               i++;
            }
            else {
               var ring = new Ring {
                  Id = ring_index,
                  Points = points.ToArray(),
                  Mass = points.Count,
                  Rects = points.Select(x =>
                     new Rect(
                        new Point(x.X - half_scale, x.Y + half_scale),
                        new Point(x.X + half_scale, x.Y - half_scale))).ToArray(),
               };
               ring.RadiusDelta = ring_index - ring.Radius;
               result[ring_index] = ring;
               points.Clear();
               ring_index++;
            }
         }
         int total = 0;
         for (i = result.Count - 1; i >= 0; i--) {
            total += result[i].Mass;
            result[i].Pressure = total;
         }
         var max_pressure = result[0].Pressure;
         for (i = 0; i < result.Count; i++) {
            var scaled = (double)result[i].Pressure / max_pressure;
            result[i].Brush = new SolidColorBrush(pressureCanvas.Gradient.GradientStops.GetRelativeColor(scaled, 1.0f));
         }
         return result;
      }

      void StartRadiusAnimation() {
         DoubleAnimation radiusAnimation = new DoubleAnimation();
         radiusAnimation.From = 0;
         radiusAnimation.To = Radius;
         radiusAnimation.AutoReverse = true;
         radiusAnimation.RepeatBehavior = RepeatBehavior.Forever;
         radiusAnimation.Duration = TimeSpan.FromMinutes(.125);
         this.BeginAnimation(PressureCanvas.RadiusProperty, radiusAnimation);
         loaded = true;
      }

      public void Draw() {
         if (loaded) {
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen()) {
               foreach (var ring in Rings.Values) {
                  for (int i = 0; i < ring.Rects.Length; i++) {
                     dc.DrawRectangle(ring.Brush, pen, ring.Rects[i]);
                  }
                  Console.WriteLine($"{ring.Id}\t{ring.RadiusDelta}\t{(ring.RadiusDelta * ring.RadiusDelta )/ (ring.Mass * ring.Mass)}");
               }
            }
            AddVisual(visual);
         }
      }
      #endregion

      #region Visual data controls
      private List<Visual> visuals = new List<Visual>();
      private Dictionary<int, Visual> visual_registry = new Dictionary<int, Visual>();
      protected override int VisualChildrenCount {
         get { return visuals.Count; }
      }
      protected override Visual GetVisualChild(int index) {
         return visuals[index];
      }

      public void RegisterAndAddVisual(Visual visual, int index) {
         visual_registry[index] = visual;
         AddVisual(visual);
      }
      public void AddVisual(Visual visual) {
         visuals.Add(visual);

         base.AddVisualChild(visual);
         base.AddLogicalChild(visual);
      }
      public void ClearVisual() {
         while (this.visuals.Count > 0) {
            this.DeleteVisual(this.visuals.First());
         }
      }
      public void DeleteVisual(Visual visual) {
         visuals.Remove(visual);

         base.RemoveVisualChild(visual);
         base.RemoveLogicalChild(visual);
      }
      #endregion

   }

   #region Gradiant
   internal static class GradientStopCollectionExtensions {
      public static Color GetRelativeColor(this GradientStopCollection gsc, double offset, float alpha) {
         var point = gsc.SingleOrDefault(x => x.Offset == offset);
         if (point != null)
            return point.Color;

         GradientStop before = gsc.Where(x => x.Offset == gsc.Min(m => m.Offset)).First();
         GradientStop after = gsc.Where(x => x.Offset == gsc.Max(m => m.Offset)).First();

         foreach (var gs in gsc) {
            if (gs.Offset < offset && gs.Offset > before.Offset)
               before = gs;
            if (gs.Offset > offset && gs.Offset < after.Offset)
               after = gs;
         }

         var color = new Color();
         color.ScR = (float)((offset - before.Offset) * (after.Color.ScR - before.Color.ScR) / (after.Offset - before.Offset) + before.Color.ScR);
         color.ScG = (float)((offset - before.Offset) * (after.Color.ScG - before.Color.ScG) / (after.Offset - before.Offset) + before.Color.ScG);
         color.ScB = (float)((offset - before.Offset) * (after.Color.ScB - before.Color.ScB) / (after.Offset - before.Offset) + before.Color.ScB);

         if (alpha >= 0 && alpha <= 1)
            color.ScA = alpha;
         else
            color.ScA = (float)((offset - before.Offset) * (after.Color.ScA - before.Color.ScA) / (after.Offset - before.Offset) + before.Color.ScA);

         return color;
      }
   }
   #endregion
}
