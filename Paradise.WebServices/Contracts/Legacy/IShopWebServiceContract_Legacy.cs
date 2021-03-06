using System.ServiceModel;

namespace Paradise.WebServices.Contracts {
	[ServiceContract(Name = "IShopWebServiceContract")]
	public interface IShopWebServiceContract_Legacy {
		[OperationContract]
		byte[] BuyiPadBundle(byte[] data);

		[OperationContract]
		byte[] BuyiPhoneBundle(byte[] data);

		[OperationContract]
		byte[] BuyItem(byte[] data);

		[OperationContract]
		byte[] BuyMasBundle(byte[] data);

		[OperationContract]
		byte[] BuyPack(byte[] data);

		[OperationContract]
		byte[] GetAllLuckyDraws_1(byte[] data);

		[OperationContract]
		byte[] GetAllLuckyDraws_2(byte[] data);

		[OperationContract]
		byte[] GetAllMysteryBoxs_1(byte[] data);

		[OperationContract]
		byte[] GetAllMysteryBoxs_2(byte[] data);

		[OperationContract]
		byte[] GetBundles(byte[] data);

		[OperationContract]
		byte[] GetLuckyDraw(byte[] data);

		[OperationContract]
		byte[] GetMysteryBox(byte[] data);

		[OperationContract]
		byte[] GetShop(byte[] data);

		[OperationContract]
		byte[] RollLuckyDraw(byte[] data);

		[OperationContract]
		byte[] RollMysteryBox(byte[] data);

		[OperationContract]
		byte[] UseConsumableItem(byte[] data);
	}
}
