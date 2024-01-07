using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Paradise.Client {
	internal class RijndaelCipher {
		private static readonly string DEFAULT_HASH_ALGORITHM = "SHA1";
		private static readonly int DEFAULT_KEY_SIZE = 256;
		private static readonly int MAX_ALLOWED_SALT_LEN = 255;
		private static readonly int MIN_ALLOWED_SALT_LEN = 4;
		private static readonly int DEFAULT_MIN_SALT_LEN = MIN_ALLOWED_SALT_LEN;
		private static readonly int DEFAULT_MAX_SALT_LEN = 8;

		private readonly int minSaltLen = -1;
		private readonly int maxSaltLen = -1;

		private readonly ICryptoTransform encryptor;
		private readonly ICryptoTransform decryptor;

		public RijndaelCipher(string passPhrase) : this(passPhrase, null) { }
		public RijndaelCipher(string passPhrase, string initVector) : this(passPhrase, initVector, -1) { }
		public RijndaelCipher(string passPhrase, string initVector, int minSaltLen) : this(passPhrase, initVector, minSaltLen, -1) { }
		public RijndaelCipher(string passPhrase, string initVector, int minSaltLen, int maxSaltLen) : this(passPhrase, initVector, minSaltLen, maxSaltLen, -1) { }
		public RijndaelCipher(string passPhrase, string initVector, int minSaltLen, int maxSaltLen, int keySize) : this(passPhrase, initVector, minSaltLen, maxSaltLen, keySize, null) { }
		public RijndaelCipher(string passPhrase, string initVector, int minSaltLen, int maxSaltLen, int keySize, string hashAlgorithm) : this(passPhrase, initVector, minSaltLen, maxSaltLen, keySize, hashAlgorithm, null) { }
		public RijndaelCipher(string passPhrase, string initVector, int minSaltLen, int maxSaltLen, int keySize, string hashAlgorithm, string saltValue) : this(passPhrase, initVector, minSaltLen, maxSaltLen, keySize, hashAlgorithm, saltValue, 1) { }

		public RijndaelCipher(string passPhrase, string initVector, int minSaltLen, int maxSaltLen, int keySize, string hashAlgorithm, string saltValue, int passwordIterations) {
			if (minSaltLen < MIN_ALLOWED_SALT_LEN) {
				this.minSaltLen = DEFAULT_MIN_SALT_LEN;
			} else {
				this.minSaltLen = minSaltLen;
			}

			if (maxSaltLen < 0 || maxSaltLen > MAX_ALLOWED_SALT_LEN) {
				this.maxSaltLen = DEFAULT_MAX_SALT_LEN;
			} else {
				this.maxSaltLen = maxSaltLen;
			}

			if (keySize <= 0) {
				keySize = DEFAULT_KEY_SIZE;
			}

			if (hashAlgorithm == null) {
				hashAlgorithm = DEFAULT_HASH_ALGORITHM;
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
			var bytes = passwordDeriveBytes.GetBytes(keySize / 8);
			RijndaelManaged rijndaelManaged = new RijndaelManaged();

			if (array.Length == 0) {
				rijndaelManaged.Mode = CipherMode.ECB;
			} else {
				rijndaelManaged.Mode = CipherMode.CBC;
			}

			encryptor = rijndaelManaged.CreateEncryptor(bytes, array);
			decryptor = rijndaelManaged.CreateDecryptor(bytes, array);
		}

		public string Encrypt(string plainText) {
			return Encrypt(Encoding.UTF8.GetBytes(plainText));
		}

		public string Encrypt(byte[] plainTextBytes) {
			return Convert.ToBase64String(EncryptToBytes(plainTextBytes));
		}

		public byte[] EncryptToBytes(string plainText) {
			return EncryptToBytes(Encoding.UTF8.GetBytes(plainText));
		}

		public byte[] EncryptToBytes(byte[] plainTextBytes) {
			var array = AddSalt(plainTextBytes);
			byte[] result;

			using (var memoryStream = new MemoryStream()) {

				lock (this) {
					using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write)) {
						cryptoStream.Write(array, 0, array.Length);
						cryptoStream.FlushFinalBlock();

						result = memoryStream.ToArray();
					}
				}
			}

			return result;
		}

		public string Decrypt(string cipherText) {
			return Decrypt(Convert.FromBase64String(cipherText));
		}

		public string Decrypt(byte[] cipherTextBytes) {
			return Encoding.UTF8.GetString(this.DecryptToBytes(cipherTextBytes));
		}

		public byte[] DecryptToBytes(string cipherText) {
			return DecryptToBytes(Convert.FromBase64String(cipherText));
		}

		public byte[] DecryptToBytes(byte[] cipherTextBytes) {
			byte[] array;

			int bytesRead = 0, saltLen = 0;

			using (var memoryStream = new MemoryStream(cipherTextBytes)) {
				array = new byte[cipherTextBytes.Length];

				lock (this) {
					using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read)) {
						bytesRead = cryptoStream.Read(array, 0, array.Length);
					}
				}
			}

			if (maxSaltLen > 0 && maxSaltLen >= minSaltLen) {
				saltLen = (array[0] & 3) | (array[1] & 12) | (array[2] & 48) | (array[3] & 192);
			}

			var array2 = new byte[bytesRead - saltLen];
			Array.Copy(array, saltLen, array2, 0, bytesRead - saltLen);

			return array2;
		}

		private byte[] AddSalt(byte[] plainTextBytes) {
			if (maxSaltLen == 0 || maxSaltLen < minSaltLen) {
				return plainTextBytes;
			}

			byte[] array = GenerateSalt();
			byte[] array2 = new byte[plainTextBytes.Length + array.Length];

			Array.Copy(array, array2, array.Length);
			Array.Copy(plainTextBytes, 0, array2, array.Length, plainTextBytes.Length);

			return array2;
		}

		private byte[] GenerateSalt() {
			int saltLen;
			if (minSaltLen == maxSaltLen) {
				saltLen = minSaltLen;
			} else {
				saltLen = GenerateRandomNumber(minSaltLen, maxSaltLen);
			}

			var bytes = new byte[saltLen];

			RNGCryptoServiceProvider rngcryptoServiceProvider = new RNGCryptoServiceProvider();
			rngcryptoServiceProvider.GetNonZeroBytes(bytes);

			bytes[0] = (byte)((bytes[0] & 252) | (saltLen & 3));
			bytes[1] = (byte)((bytes[1] & 243) | (saltLen & 12));
			bytes[2] = (byte)((bytes[2] & 207) | (saltLen & 48));
			bytes[3] = (byte)((bytes[3] & 63) | (saltLen & 192));

			return bytes;
		}

		private int GenerateRandomNumber(int minValue, int maxValue) {
			var bytes = new byte[4];

			RNGCryptoServiceProvider rngcryptoServiceProvider = new RNGCryptoServiceProvider();
			rngcryptoServiceProvider.GetBytes(bytes);

			int seed = (bytes[0] & 127) << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3];

			Random random = new Random(seed);

			return random.Next(minValue, maxValue + 1);
		}
	}
}