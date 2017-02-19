using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimalEvolution
{
    public static class SerializationHelper
    {
        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        public static void Write(this BinaryWriter writer, Vector2 data)
        {
            writer.Write(data.X);
            writer.Write(data.Y);
        }


        public static Color ReadColor(this BinaryReader reader)
        {
            return new Color(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        }

        public static void Write(this BinaryWriter writer, Color data)
        {
            writer.Write(data.R);
            writer.Write(data.G);
            writer.Write(data.B);
            writer.Write(data.A);
        }
        
        

        
        public static void WriteArray<T>(this BinaryWriter writer, T[] data, Action<T> writeFuntion)
        {
            writer.Write(data.Length);
            for(int i =0; i< data.Length; i++)
            {
                writeFuntion(data[i]);
            }
        }

        public static void WriteEnumerable<T>(this BinaryWriter writer, IEnumerable<T> data, Action<T> writeFuntion)
        {
            writer.Write(data.Count());
            foreach(T t in data)
            {
                writeFuntion(t);
            }
        }

        public static void WriteArray2<T>(this BinaryWriter writer, T[,] data, Action<T> writeFuntion)
        {
            writer.Write(data.GetLength(0));
            writer.Write(data.GetLength(1));
            for (int i = 0; i < data.GetLength(0); i++)
            {
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    writeFuntion(data[i, j]);
                }
            }
        }

        public static T[] ReadArray<T>(this BinaryReader reader, Func<T> readFunction)
        {
            int length = reader.ReadInt32();
            T[] data = new T[length];
            for(int i = 0; i< length; i++)
            {
                data[i] = readFunction();
            }
            return data;
        }

        public static T[,] ReadArray2<T>(this BinaryReader reader, Func<T> readFunction)
        {
            int length0 = reader.ReadInt32();
            int length1 = reader.ReadInt32();
            T[,] data = new T[length0,length1];
            for (int i = 0; i < length0; i++)
            {
                for(int j = 0; j< length1; j++)
                {
                    data[i,j] = readFunction();
                }
            }
            return data;
        }

        public static LinkedList<T> ReadLinkedList<T>(this BinaryReader reader, Func<T> readFunction)
        {
            LinkedList<T> data = new LinkedList<T>();
            int length = reader.ReadInt32();
            for(int i = 0; i< length; i++)
            {
                data.AddLast(readFunction());
            }
            return data;
        }

        public static bool Expect(this BinaryReader reader, String str)
        {
            return str == reader.ReadString();
        }
    }

    
}
