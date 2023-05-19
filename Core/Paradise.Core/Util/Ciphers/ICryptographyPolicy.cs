namespace Paradise.Util.Ciphers {
	public interface ICryptographyPolicy {
		string SHA256Encrypt(string inputString);

		byte[] RijndaelEncrypt(byte[] inputClearText, string passPhrase, string initVector);

		byte[] RijndaelDecrypt(byte[] inputCipherText, string passPhrase, string initVector);
	}
}
