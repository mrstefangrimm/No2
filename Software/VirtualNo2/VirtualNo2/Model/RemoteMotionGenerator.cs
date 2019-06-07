/* RemoteMotionGenerator.cs - Virtual No2 (C) motion phantom application.
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
using System.Threading;
using VirtualNo2.Model.Ev;
using VirtualNo2.Model.Ev.PreSet;
using VirtualNo2.Model.Ev.UI;

namespace VirtualNo2.Model {

  public delegate void PreSetMotion(EvCylinderPositions pos);

  public class RemoteMotionGenerator : IDisposable {

    private const int PRESETTIMERINCR = 40;

    private Timer _timer;
    private int _preSetTimer;
    private int _currentPreset;

    public RemoteMotionGenerator(PreSetMotion handler) {
      TimerCallback timerDelegate =
      new TimerCallback(delegate (object state) {
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
        switch (_currentPreset) {
          default: StopPreset(); break;

          case 1:_prog1(handler); break;
          case 2:_prog2(handler); break;
          case 3:_prog3(handler); break;
          case 4:_prog4(handler); break;
          case 5:_prog5(handler); break;
          case 6:_prog6(handler); break;
          case 7:_prog7(handler); break;
          case 8:_prog8(handler); break;
        }
        _timer.Change(PRESETTIMERINCR, PRESETTIMERINCR);
      });

      _timer = new Timer(timerDelegate);
    }

    public void Dispose() {
      if (_timer != null) {
        _timer.Dispose();
        _timer = null;
      }
    }

    public void PresetMotionClick(EvPresetMotionClick ev) {
      StopPreset();
      _currentPreset = ev.Num;
      _preSetTimer = 0;
      if (_timer != null) {
        _timer.Change(0, PRESETTIMERINCR);
      }
    }

    public void ManualMotionClick(EvManualMotionClick ev) {
      StopPreset();
    }

    public void Shutdown(EvShutdown ev) {
      StopPreset();
    }

    private void StopPreset() {
      if (_timer != null) {
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
      }
    }

    private void _prog1(PreSetMotion handler) {
      //  Position 1
      const ushort STEPSZ = 2;
      EvCylinderPositions pos = new EvCylinderPositions();
      pos.Positions = new EvCylinderPosition[3];
      pos.Positions[0] = new EvCylinderPosition() { Cy = Cylinder.Left, Lng = 128, Rtn = 127, StepSize = STEPSZ };
      pos.Positions[1] = new EvCylinderPosition() { Cy = Cylinder.Right, Lng = 128, Rtn = 127, StepSize = STEPSZ };
      pos.Positions[2] = new EvCylinderPosition() { Cy = Cylinder.Platform, Lng = 128, Rtn = 127, StepSize = STEPSZ };
      handler(pos);
    }

    private void _prog2(PreSetMotion handler) {
      // Position 2
      const ushort STEPSZ = 2;
      EvCylinderPositions pos = new EvCylinderPositions();
      pos.Positions = new EvCylinderPosition[3];
      pos.Positions[0] = new EvCylinderPosition() { Cy = Cylinder.Left, Lng = 0, Rtn = 255, StepSize = STEPSZ };
      pos.Positions[1] = new EvCylinderPosition() { Cy = Cylinder.Right, Lng = 40, Rtn = 79, StepSize = STEPSZ };
      pos.Positions[2] = new EvCylinderPosition() { Cy = Cylinder.Platform, Lng = 0, Rtn = 135, StepSize = STEPSZ };
      handler(pos);
    }

    private void _prog3(PreSetMotion handler) {
      // Position 1 <-> 2

      if (_preSetTimer == 0) {
        const ushort STEPSZ = 2;
        EvCylinderPositions pos = new EvCylinderPositions();
        pos.Positions = new EvCylinderPosition[3];
        pos.Positions[0] = new EvCylinderPosition() { Cy = Cylinder.Left, Lng = 64, Rtn = 191, StepSize = STEPSZ };
        pos.Positions[1] = new EvCylinderPosition() { Cy = Cylinder.Right, Lng = 84, Rtn = 103, StepSize = STEPSZ };
        pos.Positions[2] = new EvCylinderPosition() { Cy = Cylinder.Platform, Lng = 127, Rtn = 127, StepSize = STEPSZ };
        handler(pos);
      }
      else if (_preSetTimer >= 3000) {
        const ushort STEPSZ = 8;
        double targetLlng = 64 - 64 * Math.Sin((_preSetTimer - 3000) / 3000.0 * Math.PI);
        double targetRlng = 84 - 44 * Math.Sin((_preSetTimer - 3000) / 3000.0 * Math.PI);
        double targetLrtn = 191 + 64 * Math.Sin((_preSetTimer - 3000) / 3000.0 * Math.PI);
        double targetRrtn = 103 - 24 * Math.Sin((_preSetTimer - 3000) / 3000.0 * Math.PI);

        EvCylinderPositions pos = new EvCylinderPositions();
        pos.Positions = new EvCylinderPosition[2];
        pos.Positions[0] = new EvCylinderPosition() { Cy = Cylinder.Left, Lng = (ushort)targetLlng, Rtn = (ushort)targetLrtn, StepSize = STEPSZ };
        pos.Positions[1] = new EvCylinderPosition() { Cy = Cylinder.Right, Lng = (ushort)targetRlng, Rtn = (ushort)targetRrtn, StepSize = STEPSZ };
        handler(pos);
      }

      if (_preSetTimer == 8960) {
        _preSetTimer = 3000;
      }
      else {
        _preSetTimer += PRESETTIMERINCR;
      }
    }

    private void _prog4(PreSetMotion handler) {
      // Free-breath Gating   

      if (_preSetTimer == 0) {
        const ushort STEPSZ = 2;
        EvCylinderPositions pos = new EvCylinderPositions();
        pos.Positions = new EvCylinderPosition[3];
        pos.Positions[0] = new EvCylinderPosition() { Cy = Cylinder.Left, Lng = 127, Rtn = 127, StepSize = STEPSZ };
        pos.Positions[1] = new EvCylinderPosition() { Cy = Cylinder.Right, Lng = 127, Rtn = 127, StepSize = STEPSZ };
        pos.Positions[2] = new EvCylinderPosition() { Cy = Cylinder.Platform, Lng = 127, Rtn = 127, StepSize = STEPSZ };
        handler(pos);
      }
      else if (_preSetTimer >= 3000) {
        const ushort STEPSZ = 8;
        double target = 127 + 80 * Math.Sin((_preSetTimer - 3000) / 2500.0 * Math.PI);

        EvCylinderPositions pos = new EvCylinderPositions();
        pos.Positions = new EvCylinderPosition[3];
        pos.Positions[0] = new EvCylinderPosition() { Cy = Cylinder.Left, Lng = (ushort)target, Rtn = 127, StepSize = STEPSZ };
        pos.Positions[1] = new EvCylinderPosition() { Cy = Cylinder.Right, Lng = (ushort)target, Rtn = 127, StepSize = STEPSZ };
        pos.Positions[2] = new EvCylinderPosition() { Cy = Cylinder.Platform, Lng = (ushort)target, Rtn = 127, StepSize = STEPSZ };
        handler(pos);
      }

      if (_preSetTimer == 7960) {
        _preSetTimer = 3000;
      }
      else {
        _preSetTimer += PRESETTIMERINCR;
      }
    }

    private void _prog5(PreSetMotion handler) {
      // Breath-hold Gating    
    
      if (_preSetTimer == 0) {
        const ushort STEPSZ = 2;
        EvCylinderPositions pos = new EvCylinderPositions();
        pos.Positions = new EvCylinderPosition[3];
        pos.Positions[0] = new EvCylinderPosition() { Cy = Cylinder.Left, Lng = 60, Rtn = 127, StepSize = STEPSZ };
        pos.Positions[1] = new EvCylinderPosition() { Cy = Cylinder.Right, Lng = 60, Rtn = 127, StepSize = STEPSZ };
        pos.Positions[2] = new EvCylinderPosition() { Cy = Cylinder.Platform, Lng = 60, Rtn = 127, StepSize = STEPSZ };
        handler(pos);
      }
      else if (_preSetTimer >= 3000 && _preSetTimer < 28000) {
        const ushort STEPSZ = 8;
        double target = 60 + 50 * Math.Sin((_preSetTimer - 3000) / 2500.0 * Math.PI);

        EvCylinderPositions pos = new EvCylinderPositions();
        pos.Positions = new EvCylinderPosition[3];
        pos.Positions[0] = new EvCylinderPosition() { Cy = Cylinder.Left, Lng = (ushort)target, Rtn = 127, StepSize = STEPSZ };
        pos.Positions[1] = new EvCylinderPosition() { Cy = Cylinder.Right, Lng = (ushort)target, Rtn = 127, StepSize = STEPSZ };
        pos.Positions[2] = new EvCylinderPosition() { Cy = Cylinder.Platform, Lng = (ushort)target, Rtn = 127, StepSize = STEPSZ };
        handler(pos);
      }     
      else if (_preSetTimer > 28000 && _preSetTimer < 38000) {
        const ushort STEPSZ = 4;
        double target = 200 + 50 * Math.Cos((_preSetTimer - 28000) / 40000.0 * Math.PI);

        EvCylinderPositions pos = new EvCylinderPositions();
        pos.Positions = new EvCylinderPosition[3];
        pos.Positions[0] = new EvCylinderPosition() { Cy = Cylinder.Left, Lng = (ushort)target, Rtn = 127, StepSize = STEPSZ };
        pos.Positions[1] = new EvCylinderPosition() { Cy = Cylinder.Right, Lng = (ushort)target, Rtn = 127, StepSize = STEPSZ };
        pos.Positions[2] = new EvCylinderPosition() { Cy = Cylinder.Platform, Lng = (ushort)target, Rtn = 127, StepSize = STEPSZ };
        handler(pos);
      }
      else if (_preSetTimer == 38000) {
        const ushort STEPSZ = 4;

        EvCylinderPositions pos = new EvCylinderPositions();
        pos.Positions = new EvCylinderPosition[3];
        pos.Positions[0] = new EvCylinderPosition() { Cy = Cylinder.Left, Lng = 10, Rtn = 127, StepSize = STEPSZ };
        pos.Positions[1] = new EvCylinderPosition() { Cy = Cylinder.Right, Lng = 10, Rtn = 127, StepSize = STEPSZ };
        pos.Positions[2] = new EvCylinderPosition() { Cy = Cylinder.Platform, Lng = 10, Rtn = 127, StepSize = STEPSZ };
        handler(pos);
      }

      if (_preSetTimer == 40000) {
        _preSetTimer = 6720;
      }
      else {
        _preSetTimer += PRESETTIMERINCR;
      }
    }

    private void _prog6(PreSetMotion handler) {
      // Free-breath Gating, Position 1 <-> 2

      if (_preSetTimer == 0) {
        const ushort STEPSZ = 2;
        EvCylinderPositions pos = new EvCylinderPositions();
        pos.Positions = new EvCylinderPosition[3];
        pos.Positions[0] = new EvCylinderPosition() { Cy = Cylinder.Left, Lng = 64, Rtn = 191, StepSize = STEPSZ };
        pos.Positions[1] = new EvCylinderPosition() { Cy = Cylinder.Right, Lng = 84, Rtn = 103, StepSize = STEPSZ };
        pos.Positions[2] = new EvCylinderPosition() { Cy = Cylinder.Platform, Lng = 64, Rtn = 131, StepSize = STEPSZ };
        handler(pos);
      }
      else if (_preSetTimer >= 3000) {
        const ushort STEPSZ = 8;
        double targetLGlng = 64 - 64 * Math.Sin((_preSetTimer - 3000) / 3000.0 * Math.PI);
        double targetRlng = 84 - 44 * Math.Sin((_preSetTimer - 3000) / 3000.0 * Math.PI);
        double targetLrtn = 191 + 64 * Math.Sin((_preSetTimer - 3000) / 3000.0 * Math.PI);
        double targetRrtn = 103 - 24 * Math.Sin((_preSetTimer - 3000) / 3000.0 * Math.PI);
        double targetGrtn = 131 + 4 * Math.Sin((_preSetTimer - 3000) / 3000.0 * Math.PI);

        EvCylinderPositions pos = new EvCylinderPositions();
        pos.Positions = new EvCylinderPosition[3];
        pos.Positions[0] = new EvCylinderPosition() { Cy = Cylinder.Left, Lng = (ushort)targetLGlng, Rtn = (ushort)targetLrtn, StepSize = STEPSZ };
        pos.Positions[1] = new EvCylinderPosition() { Cy = Cylinder.Right, Lng = (ushort)targetRlng, Rtn = (ushort)targetRrtn, StepSize = STEPSZ };
        pos.Positions[2] = new EvCylinderPosition() { Cy = Cylinder.Platform, Lng = (ushort)targetLGlng, Rtn = (ushort)targetGrtn, StepSize = STEPSZ };
        handler(pos);
      }
            
      if (_preSetTimer == 8960) {
        _preSetTimer = 3000;
      }
      else {
        _preSetTimer += PRESETTIMERINCR;
      }
    }

    private void _prog7(PreSetMotion handler) {
      // Free-breath Gating loosing signal

      if (_preSetTimer == 0) {
        const ushort STEPSZ = 2;
        EvCylinderPositions pos = new EvCylinderPositions();
        pos.Positions = new EvCylinderPosition[3];
        pos.Positions[0] = new EvCylinderPosition() { Cy = Cylinder.Left, Lng = 127, Rtn = 127, StepSize = STEPSZ };
        pos.Positions[1] = new EvCylinderPosition() { Cy = Cylinder.Right, Lng = 127, Rtn = 127, StepSize = STEPSZ };
        pos.Positions[2] = new EvCylinderPosition() { Cy = Cylinder.Platform, Lng = 127, Rtn = 127, StepSize = STEPSZ };
        handler(pos);
      }
      else if (_preSetTimer >= 3000) {
        const ushort STEPSZ = 8;
        double target = 127 + 80 * Math.Sin((_preSetTimer - 3000) / 2500.0 * Math.PI);

        ushort rtnGP = 127;
        if (_preSetTimer >= 25000 && _preSetTimer < 35000) { rtnGP = 255; }

        EvCylinderPositions pos = new EvCylinderPositions();
        pos.Positions = new EvCylinderPosition[3];
        pos.Positions[0] = new EvCylinderPosition() { Cy = Cylinder.Left, Lng = (ushort)target, Rtn = 127, StepSize = STEPSZ };
        pos.Positions[1] = new EvCylinderPosition() { Cy = Cylinder.Right, Lng = (ushort)target, Rtn = 127, StepSize = STEPSZ };
        pos.Positions[2] = new EvCylinderPosition() { Cy = Cylinder.Platform, Lng = (ushort)target, Rtn = rtnGP, StepSize = STEPSZ };
        handler(pos);
      }
      
      if (_preSetTimer == 37960) {
        _preSetTimer = 3000;
      }
      else {
        _preSetTimer += PRESETTIMERINCR;
      }
    }

    private void _prog8(PreSetMotion handler) {
      // Free-breath Gating base line shift
      if (_preSetTimer == 0) {
        const ushort STEPSZ = 2;
        EvCylinderPositions pos = new EvCylinderPositions();
        pos.Positions = new EvCylinderPosition[3];
        pos.Positions[0] = new EvCylinderPosition() { Cy = Cylinder.Left, Lng = 130, Rtn = 127, StepSize = STEPSZ };
        pos.Positions[1] = new EvCylinderPosition() { Cy = Cylinder.Right, Lng = 130, Rtn = 127, StepSize = STEPSZ };
        pos.Positions[2] = new EvCylinderPosition() { Cy = Cylinder.Platform, Lng = 130, Rtn = 127, StepSize = STEPSZ };
        handler(pos);
      }
      else if (_preSetTimer >= 3000) {
        const ushort STEPSZ = 8;
        double baseline = 130 + 30 * Math.Sin((_preSetTimer - 3000) / 30000.0 * Math.PI);
        double target = baseline + 50 * Math.Sin((_preSetTimer - 3000) / 3000.0 * Math.PI);

        EvCylinderPositions pos = new EvCylinderPositions();
        pos.Positions = new EvCylinderPosition[3];
        pos.Positions[0] = new EvCylinderPosition() { Cy = Cylinder.Left, Lng = (ushort)target, Rtn = 127, StepSize = STEPSZ };
        pos.Positions[1] = new EvCylinderPosition() { Cy = Cylinder.Right, Lng = (ushort)target, Rtn = 127, StepSize = STEPSZ };
        pos.Positions[2] = new EvCylinderPosition() { Cy = Cylinder.Platform, Lng = (ushort)target, Rtn = 127, StepSize = STEPSZ };
        handler(pos);
      }          

      if (_preSetTimer == 62960) {
        _preSetTimer = 3000;
      }
      else {
        _preSetTimer += PRESETTIMERINCR;
      }
    }
   
  }
}
