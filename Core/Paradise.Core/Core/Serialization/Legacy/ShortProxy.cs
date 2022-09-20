using System;
using System.IO;

namespace Paradise.Core.Serialization.Legacy
{
	public static class ShortProxy
	{
		public static void Serialize(Stream bytes, short instance)
		{
			byte[] bytes2 = BitConverter.GetBytes(instance);
			bytes.Write(bytes2, 0, bytes2.Length);
		}

		public static short Deserialize(Stream bytes)
		{
			byte[] array = new byte[2];
			bytes.Read(array, 0, 2);
			return BitConverter.ToInt16(array, 0);
		}
	}
}
