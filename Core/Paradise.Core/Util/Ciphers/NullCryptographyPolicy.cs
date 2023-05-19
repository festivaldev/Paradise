namespace Paradise.Util.Ciphers {
	public class NullCryptographyPolicy : ICryptographyPolicy {
		public string SHA256Encrypt(string inputString) {
			return inputString;
		}

		public byte[] RijndaelEncrypt(byte[] inputClearText, string passPhrase, string initVector) {
			return inputClearText;
		}

		public byte[] RijndaelDecrypt(byte[] inputCipherText, string passPhrase, string initVector) {
			return inputCipherText;
		}
	}
}
