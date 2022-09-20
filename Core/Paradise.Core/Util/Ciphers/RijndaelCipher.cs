using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Paradise.Util.Ciphers {
	internal class RijndaelCipher {
		private static string DEFAULT_HASH_ALGORITHM = "SHA1";
		private static int DEFAULT_KEY_SIZE = 256;
		private static int MAX_ALLOWED_SALT_LEN = 255;
		private static int MIN_ALLOWED_SALT_LEN = 4;
		private static int DEFAULT_MIN_SALT_LEN = RijndaelCipher.MIN_ALLOWED_SALT_LEN;
		private static int DEFAULT_MAX_SALT_LEN = 8;

		private int minSaltLen = -1;
		private int maxSaltLen = -1;

		private ICryptoTransform encryptor;
		private ICryptoTransform decryptor;

		public RijndaelCipher(string passPhrase) : this(passPhrase, null) { }
		public RijndaelCipher(string passPhrase, string initVector) : this(passPhrase, initVector, -1) { }
		public RijndaelCipher(string passPhrase, string initVector, int minSaltLen) : this(passPhrase, initVector, minSaltLen, -1) { }
		public RijndaelCipher(string passPhrase, string initVector, int minSaltLen, int maxSaltLen) : this(passPhrase, initVector, minSaltLen, maxSaltLen, -1) { }
		public RijndaelCipher(string passPhrase, string initVector, int minSaltLen, int maxSaltLen, int keySize) : this(passPhrase, initVector, minSaltLen, maxSaltLen, keySize, null) { }
		public RijndaelCipher(string passPhrase, string initVector, int minSaltLen, int maxSaltLen, int keySize, string hashAlgorithm) : this(passPhrase, initVector, minSaltLen, maxSaltLen, keySize, hashAlgorithm, null) { }
		public RijndaelCipher(string passPhrase, string initVector, int minSaltLen, int maxSaltLen, int keySize, string hashAlgorithm, string saltValue) : this(passPhrase, initVector, minSaltLen, maxSaltLen, keySize, hashAlgorithm, saltValue, 1) { }

		public RijndaelCipher(string passPhrase, string initVector, int minSaltLen, int maxSaltLen, int keySize, string hashAlgorithm, string saltValue, int passwordIterations) {
			if (minSaltLen < RijndaelCipher.MIN_ALLOWED_SALT_LEN) {
				this.minSaltLen = RijndaelCipher.DEFAULT_MIN_SALT_LEN;
			} else {
				this.minSaltLen = minSaltLen;
			}
			if (maxSaltLen < 0 || maxSaltLen > RijndaelCipher.MAX_ALLOWED_SALT_LEN) {
				this.maxSaltLen = RijndaelCipher.DEFAULT_MAX_SALT_LEN;
			} else {
				this.maxSaltLen = maxSaltLen;
			}
			if (keySize <= 0) {
				keySize = RijndaelCipher.DEFAULT_KEY_SIZE;
			}
			if (hashAlgorithm == null) {
				hashAlgorithm = RijndaelCipher.DEFAULT_HASH_ALGORITHM;
			} else {
				hashAlgorithm = hashAlgorithm.ToUpper().Replace("-", string.Empty);
			}
			byte[] array;
			if (initVector == null) {
				array = new byte[0];
			} else {
				array = Encoding.ASCII.GetBytes(initVector);
			}
			byte[] rgbSalt;
			if (saltValue == null) {
				rgbSalt = new byte[0];
			} else {
				rgbSalt = Encoding.ASCII.GetBytes(saltValue);
			}
			PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(passPhrase, rgbSalt, hashAlgorithm, passwordIterations);
			byte[] bytes = passwordDeriveBytes.GetBytes(keySize / 8);
			RijndaelManaged rijndaelManaged = new RijndaelManaged();
			if (array.Length == 0) {
				rijndaelManaged.Mode = CipherMode.ECB;
			} else {
				rijndaelManaged.Mode = CipherMode.CBC;
			}
			this.encryptor = rijndaelManaged.CreateEncryptor(bytes, array);
			this.decryptor = rijndaelManaged.CreateDecryptor(bytes, array);
		}

		public string Encrypt(string plainText) {
			return this.Encrypt(Encoding.UTF8.GetBytes(plainText));
		}

		public string Encrypt(byte[] plainTextBytes) {
			return Convert.ToBase64String(this.EncryptToBytes(plainTextBytes));
		}

		public byte[] EncryptToBytes(string plainText) {
			return this.EncryptToBytes(Encoding.UTF8.GetBytes(plainText));
		}

		public byte[] EncryptToBytes(byte[] plainTextBytes) {
			byte[] array = this.AddSalt(plainTextBytes);
			MemoryStream memoryStream = new MemoryStream();
			byte[] result;
			lock (this) {
				CryptoStream cryptoStream = new CryptoStream(memoryStream, this.encryptor, CryptoStreamMode.Write);
				cryptoStream.Write(array, 0, array.Length);
				cryptoStream.FlushFinalBlock();
				byte[] array2 = memoryStream.ToArray();
				memoryStream.Close();
				cryptoStream.Close();
				result = array2;
			}
			return result;
		}

		public string Decrypt(string cipherText) {
			return this.Decrypt(Convert.FromBase64String(cipherText));
		}

		public string Decrypt(byte[] cipherTextBytes) {
			return Encoding.UTF8.GetString(this.DecryptToBytes(cipherTextBytes));
		}

		public byte[] DecryptToBytes(string cipherText) {
			return this.DecryptToBytes(Convert.FromBase64String(cipherText));
		}

		public byte[] DecryptToBytes(byte[] cipherTextBytes) {
			byte[] array = null;
			int num = 0;
			int num2 = 0;
			MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
			array = new byte[cipherTextBytes.Length];
			lock (this) {
				CryptoStream cryptoStream = new CryptoStream(memoryStream, this.decryptor, CryptoStreamMode.Read);
				num = cryptoStream.Read(array, 0, array.Length);
				memoryStream.Close();
				cryptoStream.Close();
			}
			if (this.maxSaltLen > 0 && this.maxSaltLen >= this.minSaltLen) {
				num2 = (int)((array[0] & 3) | (array[1] & 12) | (array[2] & 48) | (array[3] & 192));
			}
			byte[] array2 = new byte[num - num2];
			Array.Copy(array, num2, array2, 0, num - num2);
			return array2;
		}

		private byte[] AddSalt(byte[] plainTextBytes) {
			if (this.maxSaltLen == 0 || this.maxSaltLen < this.minSaltLen) {
				return plainTextBytes;
			}
			byte[] array = this.GenerateSalt();
			byte[] array2 = new byte[plainTextBytes.Length + array.Length];
			Array.Copy(array, array2, array.Length);
			Array.Copy(plainTextBytes, 0, array2, array.Length, plainTextBytes.Length);
			return array2;
		}

		private byte[] GenerateSalt() {
			int num;
			if (this.minSaltLen == this.maxSaltLen) {
				num = this.minSaltLen;
			} else {
				num = this.GenerateRandomNumber(this.minSaltLen, this.maxSaltLen);
			}
			byte[] array = new byte[num];
			RNGCryptoServiceProvider rngcryptoServiceProvider = new RNGCryptoServiceProvider();
			rngcryptoServiceProvider.GetNonZeroBytes(array);
			array[0] = (byte)((int)(array[0] & 252) | (num & 3));
			array[1] = (byte)((int)(array[1] & 243) | (num & 12));
			array[2] = (byte)((int)(array[2] & 207) | (num & 48));
			array[3] = (byte)((int)(array[3] & 63) | (num & 192));
			return array;
		}

		private int GenerateRandomNumber(int minValue, int maxValue) {
			byte[] array = new byte[4];
			RNGCryptoServiceProvider rngcryptoServiceProvider = new RNGCryptoServiceProvider();
			rngcryptoServiceProvider.GetBytes(array);
			int seed = (int)(array[0] & 127) << 24 | (int)array[1] << 16 | (int)array[2] << 8 | (int)array[3];
			Random random = new Random(seed);
			return random.Next(minValue, maxValue + 1);
		}
	}
}
