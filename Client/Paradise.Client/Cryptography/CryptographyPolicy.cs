using System;
using System.Security.Cryptography;
using System.Text;
using UberStrike.WebService.Unity;

namespace Paradise.Client {
	public class CryptographyPolicy : ICryptographyPolicy {
		public string SHA256Encrypt(string inputString) {
			UTF8Encoding utf8Encoding = new UTF8Encoding();
			byte[] bytes = utf8Encoding.GetBytes(inputString);
			SHA256Managed sha256Managed = new SHA256Managed();
			byte[] array = sha256Managed.ComputeHash(bytes);
			string text = string.Empty;
			for (int i = 0; i < array.Length; i++) {
				text += Convert.ToString(array[i], 16).PadLeft(2, '0');
			}
			return text.PadLeft(32, '0');
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
