/* MotionSustem.cs - Virtual No2 (C) motion phantom application.
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
using System.IO.Ports;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using VirtualNo2.MessagingUtil;

namespace VirtualNo2.Model {

  #region Events
  public delegate void MotionSustemLog(EvSerialOutText ev);
  public delegate void MotionSustemPosition(EvSerialOutPosition ev);
  public delegate void MotionSustemFreeMemory(EvSerialOutFreeMemory ev);

  [DataContract]
  public class EvConnect : IMessage {
    [DataMember] public string ComPort;

    private static readonly ushort MSGID = WireMessage.CreateMsgID();
    static EvConnect() {
      MessageRegistry.Instance.Register(MSGID, typeof(EvConnect));
    }
    public ushort MsgId => MSGID;
  }

  [DataContract]
  public class EvDisconnect : IMessage {
    private static readonly ushort MSGID = WireMessage.CreateMsgID();
    static EvDisconnect() {
      MessageRegistry.Instance.Register(MSGID, typeof(EvDisconnect));
    }
    public ushort MsgId => MSGID;
  }

  [DataContract]
  public class MotionSystemPosition {
    [DataMember] public byte Channel;
    [DataMember] public ushort Value;
    [DataMember] public ushort StepSize;
  }

  [DataContract]
  public class EvMotionSystemPositions : IMessage {
    [DataMember] public MotionSystemPosition[] Positions;

    private static ushort MSGID = WireMessage.CreateMsgID();
    static EvMotionSystemPositions() {
      MessageRegistry.Instance.Register(MSGID, typeof(EvMotionSystemPositions));
    }
    public ushort MsgId => MSGID;
  }

  [DataContract]
  public class EvSerialOutText : IMessage {
    [DataMember] public string Text;

    private static readonly ushort MSGID = WireMessage.CreateMsgID();
    static EvSerialOutText() {
      MessageRegistry.Instance.Register(MSGID, typeof(EvSerialOutText));
    }
    public ushort MsgId => MSGID;
  }

  [DataContract]
  public class EvSerialOutPosition : IMessage {
    [DataMember] public byte Channel;
    [DataMember] public ushort Value;

    private static readonly ushort MSGID = WireMessage.CreateMsgID();
    static EvSerialOutPosition() {
      MessageRegistry.Instance.Register(MSGID, typeof(EvSerialOutPosition));
    }
    public ushort MsgId => MSGID;
  }

  [DataContract]
  public class EvSerialOutFreeMemory : IMessage {
    [DataMember] public string Text;

    private static readonly ushort MSGID = WireMessage.CreateMsgID();
    static EvSerialOutFreeMemory() {
      MessageRegistry.Instance.Register(MSGID, typeof(EvSerialOutFreeMemory));
    }
    public ushort MsgId => MSGID;
  }

  #endregion Events

  internal class SerialOutMessage {
    private const byte CMD = 2;
    private Dictionary<byte, byte[]> _servoData = new Dictionary<byte, byte[]>();

    public void Add(byte servo, UInt16 pos, UInt16 step) {
      var motorData = new byte[2];
      motorData[0] = (byte)((step << 4) | servo);
      motorData[1] = (byte)(pos);
      lock (_servoData) {
        if (_servoData.ContainsKey(servo)) {
          _servoData[servo] = motorData;
        }
        else {
          _servoData.Add(servo, motorData);
        }
      }
      //_servoData.Add(m, (byte)((step << 4) | (byte)m));
      //_servoData.Add((byte)(pos));
    }

    public void Add(SerialOutMessage moreData) {
      lock (_servoData) {
        moreData._servoData.Keys.ToList().ForEach(k => {
          if (_servoData.ContainsKey(k)) {
            _servoData[k] = moreData._servoData[k];
          }
          else {
            _servoData.Add(k, moreData._servoData[k]);
          }
        });
      }
    }

    public void Clear() {
      _servoData.Clear();
    }

    public byte[] Data {
      get {
        lock (_servoData) {
          int numBytes = _servoData.Values.Sum(x => x.Length);
          List<byte> serout = new List<byte>(1 + numBytes);
          serout.Add((byte)(CMD | (numBytes << 3)));
          _servoData.Values.ToList().ForEach(x => serout.AddRange(x));
          return serout.ToArray<byte>();
        }
        //byte[] serout = new byte[1 + _servoData.Count];
        //serout[0] = (byte)(CMD | (_servoData.Count << 3));
        //Array.Copy(_servoData.ToArray(), 0, serout, 1, _servoData.Count);
        //return serout;
      }
    }

  }
  
  public class MotionSystem : IDisposable {

    public enum SyncState {  Desynced, Synced }

    private const int _portBaudRate = 9600; // 9600, 38400, 115200;
    private SerialOutMessage _sendBuffer = new SerialOutMessage();
    public SerialPort _serialPort;
    private Timer _timer;
    private int _lastSentHashCode;
    private MotionSustemLog _logHandler;
    private SyncState _state = SyncState.Desynced;
    private string _receivedText = string.Empty;
    private bool _isReceivingText;

    public MotionSystem(MotionSustemLog handler) {
      _logHandler = handler;

      TimerCallback timerDelegate =
      new TimerCallback(delegate (object state) {
       lock(_sendBuffer) {
          var data = _sendBuffer.Data;
          int hashCode = data.GetHashCode();
          if (/*hashCode != _lastSentHashCode &&*/ data.Length > 1 && _serialPort != null && _serialPort.IsOpen) {
            _serialPort.Write(data, 0, data.Length);
            _sendBuffer.Clear();
            _lastSentHashCode = hashCode;
          }
        }
      });
      _timer = new Timer(timerDelegate, null, 50, 50);
    }

    public void Dispose() {
      if (_timer != null) {
        _timer.Dispose();
        _timer = null;
      }
      if (_serialPort != null) {
        _serialPort.Dispose();
        _serialPort = null;
      }
    }

    public void Connect(EvConnect ev) {
      SerialConnect(ev.ComPort);

      SendSync();
    }

    public void Disconnect(EvDisconnect ev) {
      SerialDisconnect();
    }
    
    public void MotionSystemPositions(EvMotionSystemPositions ev) {
      SerialOutMessage cmd = new SerialOutMessage();
      foreach (var pos in ev.Positions) {
        cmd.Add(pos.Channel, pos.Value, pos.StepSize);
      }
      Send(cmd);
    }

    private void Send(SerialOutMessage data) {
      if (_serialPort != null && _serialPort.IsOpen) {
        lock (_sendBuffer) {
          //var seroutbytes = data.Data;
          //_serialPort.Write(seroutbytes, 0, seroutbytes.Length);

          //_timer.Change(5000, 5000);
          _sendBuffer.Add(data);
        }
      }
      else {
        EvSerialOutText cmd = new EvSerialOutText() { Text = "Send failed since Serial Port is not open." };
        _logHandler(cmd);
      }
    }

    private void SendSync() {
      if (_serialPort != null && _serialPort.IsOpen) {
        lock (_sendBuffer) {
          byte[] syncMsg = new byte[1];
          syncMsg[0] = 4;
          _serialPort.Write(syncMsg, 0, 1);
        }
      }
      else {
        EvSerialOutText cmd = new EvSerialOutText() { Text = "Send 'Sync' failed since Serial Port is not open." };
        _logHandler(cmd);
      }
    }

    private void SerialConnect(string comPort) {
      SerialDisconnect();

      _serialPort = new SerialPort(comPort, _portBaudRate);
      _serialPort.DataReceived += OnSerialPortDataReceived;
      try {
        _serialPort.Open();
        _state = SyncState.Desynced;
      }
      catch (Exception e) {
        if (e.InnerException != null) {
          EvSerialOutText cmd = new EvSerialOutText() { Text = e.InnerException.Message };
          _logHandler(cmd);
        }
        else {
          EvSerialOutText cmd = new EvSerialOutText() { Text = e.Message };
          _logHandler(cmd);
        }
        SerialDisconnect();
      }
    }

    private void SerialDisconnect() {
      if (_serialPort != null) {
        _serialPort.DataReceived -= OnSerialPortDataReceived;
        _serialPort.Close();
        _serialPort.Dispose();
        _serialPort = null;
      }
      _state = SyncState.Desynced;
    }

    private void OnSerialPortDataReceived(object sender, SerialDataReceivedEventArgs e) {
      char[] serin = new char[_serialPort.BytesToRead];
      _serialPort.Read(serin, 0, serin.Length);

      if (_state == SyncState.Desynced) {
        _receivedText += new string(serin);
        int index = _receivedText.IndexOf("Synced");
        if (index != -1) {
          string syncedData = _receivedText.Remove(0, index + 6);
          _receivedText = string.Empty;

          _isReceivingText = true;
          syncedData.ToList().ForEach(ch => {
            if (_isReceivingText && ch != '|') {
              _receivedText += ch;
            }
            if (ch == '|') {
              _isReceivingText = !_isReceivingText;
            }
          });
          _state = SyncState.Synced;
        }       
        EvSerialOutText cmd = new EvSerialOutText() { Text = _state + _receivedText };
        _logHandler(cmd);
      }
      else if (_state == SyncState.Synced) {

        serin.ToList().ForEach(ch => {
          if (_isReceivingText && ch != '|') {
            _receivedText += ch;
          }
          if (ch == '|') {
            _isReceivingText = !_isReceivingText;
          }
        });

        int index = _receivedText.IndexOf("\r\n");
        if (index != -1) {
          string msg = _receivedText.Substring(0, index);
          EvSerialOutText cmd = new EvSerialOutText() { Text = msg };
          _receivedText = _receivedText.Remove(0, index + 2);
          _logHandler(cmd);
        }
      }
    }

  }
}
