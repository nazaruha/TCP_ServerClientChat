using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPUserMessage
{
    public enum TypeMessage
    {
        Login, Logout, Message
    }
    public class UserMessage
    {
        public TypeMessage MessageType;
        public string Id { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((int)MessageType);
                    bw.Write(Id);
                    bw.Write(Name);
                    bw.Write(Text);
                }
                return ms.ToArray();
            }
        }

        public static UserMessage Deserialize(byte[] data)
        {
            UserMessage userMsg = new UserMessage();
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    userMsg.MessageType = (TypeMessage)br.ReadInt32();
                    userMsg.Id = br.ReadString();
                    userMsg.Name = br.ReadString();
                    userMsg.Text = br.ReadString();
                }
            }
            return userMsg;
        }
    }
}
