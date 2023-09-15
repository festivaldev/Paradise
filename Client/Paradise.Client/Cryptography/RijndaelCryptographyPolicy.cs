using System;
using System.Security.Cryptography;
using System.Text;
using UberStrike.WebService.Unity;

namespace Paradise.Client {
	internal class RijndaelCryptographyPolicy : ICryptographyPolicy {
		public string SHA256Encrypt(string inputString) {
			var bytes = Encoding.UTF8.GetBytes(inputString);

			using (var sha256 = SHA256.Create()) {
				var array = sha256.ComputeHash(bytes);

				var text = string.Empty;
				for (int i = 0; i < array.Length; i++) {
					text += Convert.ToString(array[i], 16).PadLeft(2, '0');
				}

				return text.PadLeft(32, '0');
			}
		}

		public byte[] RijndaelEncrypt(byte[] inputClearText, string passPhrase, string initVector) {
			RijndaelCipher rijndaelCipher = new RijndaelCipher(passPhrase, initVector);
			return rijndaelCipher.EncryptToBytes(inputClearText);
		}

		public byte[] RijndaelDecrypt(byte[] inputCipherText, string passPhrase, string initVector) {
			RijndaelCipher rijndaelCipher = new RijndaelCipher(passPhrase, initVector);
			return rijndaelCipher.DecryptToBytes(inputCipherText);
		}
	}
}
