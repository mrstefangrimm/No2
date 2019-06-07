/* WireMessage.cs - Virtual No2 (C) motion phantom application.
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
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using VirtualNo2.MessagingUtil;

namespace MessagingLib {

  [DataContract]
  public class WireMessage {

    [DataMember] public ushort MsgID;
    [DataMember] public string Data;

    public static ushort MSGID_COUNTER = 1;
    public static ushort CreateMsgID() { return MSGID_COUNTER++; }

    public static IMessage Deserialize(string data) {
      var wm = new WireMessage();
      using (MemoryStream msh = new MemoryStream(Encoding.ASCII.GetBytes(data))) {
        DataContractJsonSerializer serh = new DataContractJsonSerializer(typeof(WireMessage));
        wm = (WireMessage)serh.ReadObject(msh);

        Type type = MessageRegistry.Instance.Get(wm.MsgID);
        using (MemoryStream msd = new MemoryStream(Encoding.ASCII.GetBytes(wm.Data))) {
          DataContractJsonSerializer serd = new DataContractJsonSerializer(type);
          object msg = serd.ReadObject(msd);
          return (IMessage)msg;
        }
      }
    }
    public static string Serialize(IMessage msgObj) {

      WireMessage wm = new WireMessage();
      using (MemoryStream msd = new MemoryStream()) {
        DataContractJsonSerializer ser = new DataContractJsonSerializer(msgObj.GetType());
        ser.WriteObject(msd, msgObj);

        msd.Position = 0;

        using (StreamReader reader = new StreamReader(msd)) {
          wm.Data = reader.ReadToEnd();
        }
      }

      wm.MsgID = msgObj.MsgId;
      using (MemoryStream msh = new MemoryStream()) {
        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(WireMessage));
        ser.WriteObject(msh, wm);

        msh.Position = 0;

        using (StreamReader reader = new StreamReader(msh)) {
          string text = reader.ReadToEnd();
          return text;
        }
      }
    }
  }

}
