/* EventMediator.cs - Virtual No2 (C) motion phantom application.
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

using MessagingLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using VirtualNo2.MessagingUtil;
using VirtualNo2.Model.Ev;
using VirtualNo2.Model.Ev.MotionSystem;
using VirtualNo2.Model.Ev.UI;
using ZeroMQ;

namespace VirtualNo2.Model {

  public class EventMediator : IDisposable {

    private Task _taskIncoming;
    private ZSocket _mqNotifyViewModel;
    private Dictionary<Type, Action<object>> _eventHandlers = new Dictionary<Type, Action<object>>();

    private MotionSystem _motionSystem;
    private RemoteMotionGenerator _preSetMotionGenerator;
    private bool _isMotionSystemConnected;

    public EventMediator() {
      _motionSystem = new MotionSystem(OnMotionSystemTextMessage);
      _preSetMotionGenerator = new RemoteMotionGenerator(OnEvCylinderPositionsPreSet);
      RegisterCommands();
    }

    public void Dispose() {
      try {
        _taskIncoming.Wait();
      }
      catch (Exception e) {
        if (e.InnerException == null || (e.InnerException != null && !(e.InnerException is OperationCanceledException))) {
          Debug.Fail(e.Message);
        }
      }
      finally {
        _mqNotifyViewModel.Dispose();
        _taskIncoming.Dispose();
      }
    }

    public Task StartBindIncomingAsync(CancellationToken cancellationToken, ZContext ctx, string commandQueueName, AutoResetEvent startupWaitHandle) {
      _taskIncoming = Task.Run(() => {
        Thread.CurrentThread.Name = "EventMediator/Incoming";
        using (ZSocket mqIncoming = new ZSocket(ctx, ZSocketType.PULL)) {
          // Also notify sockets have to be created on the thread they are used
          _mqNotifyViewModel = new ZSocket(ctx, ZSocketType.PAIR);
          mqIncoming.Bind(commandQueueName);
          startupWaitHandle.Set();
          while (true) {
            cancellationToken.ThrowIfCancellationRequested();
            try {
              using (ZFrame mqmsg = mqIncoming.ReceiveFrame()) {
                IMessage msg = WireMessage.Deserialize(mqmsg.ReadString());
                _eventHandlers[msg.GetType()].Invoke(msg);
              }
            }
            catch (ZException ze) {
              if (ze.Error == ZError.ETERM) {
                // ZError.ETERM is thrown if the context is closed
                break;
              }
              if (ze.Error != ZError.EAGAIN) {
                // ZError.EAGAIN is thrown if the receive timeout exceeded (mqIncoming.SetOption(ZSocketOption.RCVTIMEO, 200);)
                throw;
              }
            }
          }
        }
      });
      return _taskIncoming;
    }
        
    public void ConnectViewModel(ZContext ctx, string notifyQueueName) {
      _mqNotifyViewModel.Connect(notifyQueueName);
    }

    private static EvMotionSystemPositions CylinderToNo2MotionSystem(Ev.UI.EvCylinderMotion ev) {
      EvMotionSystemPositions cmd = new EvMotionSystemPositions();
      cmd.Positions = new MotionSystemPosition[2];
      switch (ev.Cy) {
      default:
        break;
      case Cylinder.Left:
        cmd.Positions[0] = new MotionSystemPosition() { Channel = (byte)ServoNumber.LLNG, Value = ev.Lng, StepSize = 5 };
        cmd.Positions[1] = new MotionSystemPosition() { Channel = (byte)ServoNumber.LRTN, Value = ev.Rtn, StepSize = 5 };
        break;
      case Cylinder.Right:
        cmd.Positions[0] = new MotionSystemPosition() { Channel = (byte)ServoNumber.RLNG, Value = ev.Lng, StepSize = 5 };
        cmd.Positions[1] = new MotionSystemPosition() { Channel = (byte)ServoNumber.RRTN, Value = ev.Rtn, StepSize = 5 };
        break;
      case Cylinder.Platform:
        cmd.Positions[0] = new MotionSystemPosition() { Channel = (byte)ServoNumber.GLNG, Value = ev.Lng, StepSize = 5 };
        cmd.Positions[1] = new MotionSystemPosition() { Channel = (byte)ServoNumber.GRTN, Value = ev.Rtn, StepSize = 5 };
        break;
      }
      return cmd;
    }
    
    private static EvMotionSystemPositions CylinderToNo2MotionSystem(Ev.PreSet.EvCylinderPositions ev) {
      EvMotionSystemPositions cmd = new EvMotionSystemPositions();
      cmd.Positions = new MotionSystemPosition[ev.Positions.Length * 2];
      for (int n = 0, m = 0; n < ev.Positions.Length; n++, m += 2) {
        switch (ev.Positions[n].Cy) {
        default:
          break;
        case Cylinder.Left:
          cmd.Positions[m] = new MotionSystemPosition() { Channel = (byte)ServoNumber.LLNG, Value = ev.Positions[n].Lng, StepSize = ev.Positions[n].StepSize };
          cmd.Positions[m + 1] = new MotionSystemPosition() { Channel = (byte)ServoNumber.LRTN, Value = ev.Positions[n].Rtn, StepSize = ev.Positions[n].StepSize };
          break;
        case Cylinder.Right:
          cmd.Positions[m] = new MotionSystemPosition() { Channel = (byte)ServoNumber.RLNG, Value = ev.Positions[n].Lng, StepSize = ev.Positions[n].StepSize };
          cmd.Positions[m + 1] = new MotionSystemPosition() { Channel = (byte)ServoNumber.RRTN, Value = ev.Positions[n].Rtn, StepSize = ev.Positions[n].StepSize };
          break;
        case Cylinder.Platform:
          cmd.Positions[m] = new MotionSystemPosition() { Channel = (byte)ServoNumber.GLNG, Value = ev.Positions[n].Lng, StepSize = ev.Positions[n].StepSize };
          cmd.Positions[m + 1] = new MotionSystemPosition() { Channel = (byte)ServoNumber.GRTN, Value = ev.Positions[n].Rtn, StepSize = ev.Positions[n].StepSize };
          break;
        }
      }
      return cmd;
    }

    private void OnEvCylinderMotionUI(Ev.UI.EvCylinderMotion ev) {
      if (_isMotionSystemConnected) {
        EvMotionSystemPositions cmd = CylinderToNo2MotionSystem(ev);
        _motionSystem.MotionSystemPositions(cmd);
      }
    }

    private void OnEvCylinderPositionsPreSet(Ev.PreSet.EvCylinderPositions ev) {
      // not over the event queue (yet)
      if (_isMotionSystemConnected) {
        EvMotionSystemPositions cmd = CylinderToNo2MotionSystem(ev);
        _motionSystem.MotionSystemPositions(cmd);
      }
      var mqmsg = WireMessage.Serialize(ev);
      using (var frame = new ZFrame(mqmsg)) { _mqNotifyViewModel.Send(frame); }
    }

    private void OnEvLogMessage(EvLogMessage ev) {
      string mqmsg = WireMessage.Serialize(ev);
      using (var frame = new ZFrame(mqmsg)) { _mqNotifyViewModel.Send(frame); }
    }

    private void OnEvManualMotionClick(EvManualMotionClick ev) {
      _preSetMotionGenerator.ManualMotionClick(ev);
    }

    private void OnEvPresetMotionClick(EvPresetMotionClick ev) {
      _preSetMotionGenerator.PresetMotionClick(ev);
    }

    private void OnEvShutdown(EvShutdown ev) {
      var mqmsg = WireMessage.Serialize(ev);
      using (var frame = new ZFrame(mqmsg)) { _mqNotifyViewModel.Send(frame); }

      EvDisconnect evMotionSystemDisconnect = new EvDisconnect();
      _motionSystem.Disconnect(evMotionSystemDisconnect);
      _preSetMotionGenerator.Shutdown(ev);

      throw new OperationCanceledException();
    }

    private void OnEvConnect(EvConnect ev) {
      _isMotionSystemConnected = true;
      _motionSystem.Connect(ev);
    }

    private void OnEvDisconnect(EvDisconnect ev) {
      _isMotionSystemConnected = false;
      _motionSystem.Disconnect(ev);
    }

    private void OnMotionSystemTextMessage(EvSerialOutText logMsg) {
      // not over the event queue (yet)
      EvLogMessage msg = new EvLogMessage();
      msg.Source = "MotionSystem";
      msg.Message = logMsg.Text;
      string mqmsg = WireMessage.Serialize(msg);
      using (var frame = new ZFrame(mqmsg)) { _mqNotifyViewModel.Send(frame); }
    }

    private void RegisterCommands() {
      _eventHandlers[typeof(Ev.UI.EvCylinderMotion)] = ev => OnEvCylinderMotionUI((Ev.UI.EvCylinderMotion)ev);
      _eventHandlers[typeof(Ev.PreSet.EvCylinderPositions)] = ev => OnEvCylinderPositionsPreSet((Ev.PreSet.EvCylinderPositions)ev);
      _eventHandlers[typeof(EvLogMessage)] = ev => OnEvLogMessage((EvLogMessage)ev);
      _eventHandlers[typeof(EvManualMotionClick)] = ev => OnEvManualMotionClick((EvManualMotionClick)ev);
      _eventHandlers[typeof(EvPresetMotionClick)] = ev => OnEvPresetMotionClick((EvPresetMotionClick)ev);
      _eventHandlers[typeof(EvConnect)] = ev => OnEvConnect((EvConnect)ev);
      _eventHandlers[typeof(EvDisconnect)] = ev => OnEvDisconnect((EvDisconnect)ev);
      _eventHandlers[typeof(EvShutdown)] = ev => OnEvShutdown((EvShutdown)ev);
    }
 
  }

}
