using System;
using System.Collections.Generic;
using System.Text;

namespace NetWolt
{
    public class Command
    {
        private List<object> content;

        public bool empty;
        internal bool complete;
        internal bool started;
        internal int cmdLen;


        public Command()
        {
            content = new List<object>();

            empty = true;

            complete = false;
            started = false;
            cmdLen = 0;
        }

        internal List<byte> sendableFormat()
        {
            List<byte> o = new List<byte>();

            o.Add((byte)dataType.startD);

            for (int i = 0; i < content.Count; i++)
            {
                short r = 0;
                for (int j = i; j < content.Count; j++)
                {
                    if (!content[j].Equals(content[i]))
                    {
                        break;
                    }
                    else
                    {
                        r++;
                    }
                }

                if (r > 1)
                {
                    o.Add((byte)dataType.compD);
                    o.AddRange(shortToByte(r));
                    i += r - 1;
                }

                switch (content[i].GetType().Name)
                {
                    case "Boolean":
                        o.Add((byte)dataType.boolD);
                        o.AddRange(boolToByte((bool)content[i]));
                        break;
                    case "Byte":
                        o.Add((byte)dataType.byteD);
                        o.Add((byte)content[i]);
                        break;
                    case "Int16":
                        o.Add((byte)dataType.shortD);
                        o.AddRange(shortToByte((short)content[i]));
                        break;
                    case "Int32":
                        o.Add((byte)dataType.intD);
                        o.AddRange(intToByte((int)content[i]));
                        break;
                    case "Char":
                        o.Add((byte)dataType.charD);
                        o.AddRange(charToByte((char)content[i]));
                        break;
                    case "Single":
                        o.Add((byte)dataType.floatD);
                        o.AddRange(floatToByte((float)content[i]));
                        break;
                    case "String":
                        o.Add((byte)dataType.stringD);
                        o.AddRange(stringToByte((string)content[i]));
                        break;
                }
            }

            o.Add((byte)dataType.endD);

            cmdLen = o.Count;

            return o;
        }

        internal static Command parseCommand(List<byte> bytes)
        {
            Command cmd = new Command();

            int bytesRead = 0;

            bool error = false;

            short repeat = 0;
            int repeatPoint = 0;

            while (bytesRead < bytes.Count && !error)
            {
                byte type = bytes[bytesRead];
                bytesRead++;

                try
                {
                    switch ((dataType)type)
                    {
                        case dataType.startD:
                            cmd.started = true;
                            break;
                        case dataType.endD:
                            cmd.complete = true;
                            cmd.cmdLen = bytesRead;
                            bytesRead = bytes.Count;
                            break;
                        case dataType.boolD:
                            cmd.content.Add(boolFromByte(bytes.GetRange(bytesRead, 1).ToArray()));
                            bytesRead += 1;
                            break;
                        case dataType.byteD:
                            cmd.content.Add(bytes[bytesRead]);
                            bytesRead += 1;
                            break;
                        case dataType.shortD:
                            cmd.content.Add(shortFromByte(bytes.GetRange(bytesRead, 2).ToArray()));
                            bytesRead += 2;
                            break;
                        case dataType.intD:
                            cmd.content.Add(intFromByte(bytes.GetRange(bytesRead, 4).ToArray()));
                            bytesRead += 4;
                            break;
                        case dataType.charD:
                            cmd.content.Add(charFromByte(bytes.GetRange(bytesRead, 2).ToArray()));
                            bytesRead += 2;
                            break;
                        case dataType.floatD:
                            cmd.content.Add(floatFromByte(bytes.GetRange(bytesRead, 4).ToArray()));
                            bytesRead += 4;
                            break;
                        case dataType.stringD:
                            int strLen = intFromByte(bytes.GetRange(bytesRead, 4).ToArray());
                            bytesRead += 4;
                            cmd.content.Add(stringFromByte(strLen, bytes.GetRange(bytesRead, strLen * 2).ToArray()));
                            bytesRead += strLen * 2;
                            break;
                        case dataType.compD:
                            short count = shortFromByte(bytes.GetRange(bytesRead, 2).ToArray());
                            bytesRead += 2;
                            repeat = count;
                            repeatPoint = bytesRead;
                            break;
                        default:
                            //panik
                            error = true;
                            break;
                    }

                    if (repeat > 0)
                    {
                        repeat--;
                        bytesRead = repeatPoint;
                    }
                }
                catch (Exception)
                {
                    break;
                }

            }

            if (cmd.complete && !cmd.started)
            {
                cmd.cmdLen = bytesRead;
            }

            if (cmd.cmdLen > 0)
            {
                cmd.empty = false;
            }

            return cmd;
        }

        public object getParameter(int pos)
        {
            if (pos > content.Count)
            {
                return (byte)0;
            }
            else
            {
                return content[pos];
            }
        }

        public void addParameter(object parameter)
        {
            content.Add(parameter);
            empty = false;
        }

        public bool setParameter(int pos, object parameter)
        {
            empty = false;
            if (pos < content.Count)
            {
                content[pos] = parameter;
                return true;
            }
            else
            {
                if (pos > content.Count)
                {
                    int add = pos - content.Count;

                    for (int i = 0; i < add; i++)
                    {
                        content.Add((byte)0);
                    }
                }
                content.Add(parameter);
                return false;
            }

        }

        public int getParameterCount()
        {
            return content.Count;
        }

        private static byte[] boolToByte(bool input)
        {
            return BitConverter.GetBytes(input);
        }

        private static byte[] shortToByte(short input)
        {
            return BitConverter.GetBytes(input);
        }

        private static byte[] intToByte(int input)
        {
            byte[] intBytes = BitConverter.GetBytes(input);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);
            return intBytes;
        }

        private static byte[] charToByte(char input)
        {
            return BitConverter.GetBytes(input);
        }

        private static byte[] floatToByte(float input)
        {
            return BitConverter.GetBytes(input);
        }

        private static byte[] stringToByte(string input)
        {
            List<byte> output = new List<byte>();

            output.AddRange(intToByte(input.Length));

            for (int i = 0; i < input.Length; i++)
            {
                output.AddRange(charToByte(input[i]));
            }

            return output.ToArray();
        }



        private static bool boolFromByte(byte[] input)
        {
            return BitConverter.ToBoolean(input, 0);
        }

        private static short shortFromByte(byte[] input)
        {
            return BitConverter.ToInt16(input, 0);
        }

        private static int intFromByte(byte[] input)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(input);

            return BitConverter.ToInt32(input, 0);
        }

        private static char charFromByte(byte[] input)
        {
            return BitConverter.ToChar(input, 0);
        }

        private static float floatFromByte(byte[] input)
        {
            return BitConverter.ToSingle(input, 0);
        }

        private static string stringFromByte(int lenght, byte[] input)
        {
            string output = "";

            for (int i = 0; i < lenght; i++)
            {
                output += charFromByte(new byte[] { input[i * 2], input[(i * 2) + 1] });
            }

            return output;
        }

        public override string ToString()
        {
            string output = "[";

            for (int i = 0; i < content.Count; i++)
            {
                output += content[i].ToString();

                if (i < content.Count - 1)
                {
                    output += "|";
                }
            }

            output += "]";

            return output;
        }

        private enum dataType : byte
        {
            startD,
            endD,
            boolD,
            byteD,
            shortD,
            intD,
            charD,
            floatD,
            stringD,
            compD
        }
    }


}
