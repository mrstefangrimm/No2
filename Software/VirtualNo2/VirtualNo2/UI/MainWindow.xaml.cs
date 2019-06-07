/* MainWindow.xaml.cs - Virtual No2 (C) motion phantom application.
 * Copyright (C) 2019 by Stefan Grimm
 *
 * This is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This software is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.

 * You should have received a copy of the GNU Lesser General Public License
 * along with the VirtualNo2 software.  If not, see
 * <http://www.gnu.org/licenses/>.
 */

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace VirtualNo2.UI {

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {

    private DispatcherTimer _timer;
    private PointCollection _points;

    public MainWindow() {
      InitializeComponent();

      _points = new PointCollection(100);
      for (int n = 0; n < 100; n++) {
        _points.Add(new Point());
      }
      GatingPlatformHistory.Points = _points;

      EventHandler timerDelegate =
        new EventHandler(delegate (object sender, EventArgs e) {
          for (int n = 1; n < 100; n++) {
            _points[n - 1] = new Point(_points[n - 1].X, _points[n].Y);
          }
          _points[99] = new Point(_points[99].X, Pointer.Y1);
        });
      _timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
      _timer.Interval = TimeSpan.FromMilliseconds(200);
      _timer.Tick += timerDelegate;
      _timer.Start();
    } 

    protected override void OnClosing(CancelEventArgs e) {
      base.OnClosing(e);
      if (_timer != null) {
        _timer.Stop();
        _timer = null;
      }
    }

    private void _OnSizeChanged(object sender, SizeChangedEventArgs e) {
      var m = Pointer.RenderTransform.Value;
      m.OffsetY = e.NewSize.Height / 2.0;
      Pointer.RenderTransform = new MatrixTransform(m);
      GatingPlatformHistory.RenderTransform = new MatrixTransform(m);

      double xStep = e.NewSize.Width / 100.0;
      for (int n = 0; n < 100; n++) {
        _points[n] = new Point(n * xStep, _points[n].Y);
      }
    }

    //private void _OnGatingPlatform(object sender, RoutedEventArgs e) {
    //  var axis = new Vector3D(0, 1, 0);
    //  int angle = 10;
    //  var matrix = gatingPlatform.Transform.Value;
    //  matrix.RotateAt(new Quaternion(axis, angle), new Point3D(60, 140, 194));
    //  gatingPlatform.Transform = new MatrixTransform3D(matrix);
    //}

  }
}
