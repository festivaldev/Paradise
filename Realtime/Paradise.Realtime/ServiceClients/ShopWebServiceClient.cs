using Paradise.Core.Models.Views;
using Paradise.Core.Serialization;
using Paradise.Core.Types;
using Paradise.DataCenter.Common.Entities;
using Paradise.Realtime.Server;
using Paradise.WebServices.Contracts;
using System.Collections.Generic;
using System.IO;

namespace Paradise.Realtime {
	public class ShopWebServiceClient : BaseWebServiceClient<IShopWebServiceContract> {
		public static readonly ShopWebServiceClient Instance = new ShopWebServiceClient(
			endpointUrl: BaseRealtimeApplication.Instance.Configuration.WebServiceBaseUrl,
			webServicePrefix: BaseRealtimeApplication.Instance.Configuration.WebServicePrefix,
			webServiceSuffix: BaseRealtimeApplication.Instance.Configuration.WebServiceSuffix
		);

		public ShopWebServiceClient(string endpointUrl, string webServicePrefix, string webServiceSuffix) : base(endpointUrl, $"{webServicePrefix}ShopWebService{webServiceSuffix}") { }

		public bool BuyBundle(string authToken, int bundleId, ChannelType channel, string hashedReceipt) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, bundleId);
				EnumProxy<ChannelType>.Serialize(bytes, channel);
				StringProxy.Serialize(bytes, hashedReceipt);

				var result = Service.BuyBundle(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return BooleanProxy.Deserialize(inputStream);
				}
			}
		}

		public bool BuyBundleSteam(int bundleId, string steamId, string authToken) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, bundleId);
				StringProxy.Serialize(bytes, steamId);
				StringProxy.Serialize(bytes, authToken);

				var result = Service.BuyBundleSteam(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return BooleanProxy.Deserialize(inputStream);
				}
			}
		}

		public int BuyItem(int itemId, string authToken, UberStrikeCurrencyType currencyType, BuyingDurationType durationType, UberstrikeItemType itemType, BuyingLocationType marketLocation, BuyingRecommendationType recommendationType) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, itemId);
				StringProxy.Serialize(bytes, authToken);
				EnumProxy<UberStrikeCurrencyType>.Serialize(bytes, currencyType);
				EnumProxy<BuyingDurationType>.Serialize(bytes, durationType);
				EnumProxy<UberstrikeItemType>.Serialize(bytes, itemType);
				EnumProxy<BuyingLocationType>.Serialize(bytes, marketLocation);
				EnumProxy<BuyingRecommendationType>.Serialize(bytes, recommendationType);

				var result = Service.BuyItem(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}

		public int BuyPack(int itemId, string authToken, PackType packType, UberStrikeCurrencyType currencyType, UberstrikeItemType itemType, BuyingLocationType marketLocation, BuyingRecommendationType recommendationType) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, itemId);
				StringProxy.Serialize(bytes, authToken);
				EnumProxy<PackType>.Serialize(bytes, packType);
				EnumProxy<UberStrikeCurrencyType>.Serialize(bytes, currencyType);
				EnumProxy<UberstrikeItemType>.Serialize(bytes, itemType);
				EnumProxy<BuyingLocationType>.Serialize(bytes, marketLocation);
				EnumProxy<BuyingRecommendationType>.Serialize(bytes, recommendationType);

				var result = Service.BuyPack(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}

		public bool FinishBuyBundleSteam(string orderId) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, orderId);

				var result = Service.FinishBuyBundleSteam(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return BooleanProxy.Deserialize(inputStream);
				}
			}
		}

		public List<LuckyDrawUnityView> GetAllLuckyDraws() {
			using (var bytes = new MemoryStream()) {
				var result = Service.GetAllLuckyDraws_1(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return ListProxy<LuckyDrawUnityView>.Deserialize(inputStream, LuckyDrawUnityViewProxy.Deserialize);
				}
			}
		}

		public List<LuckyDrawUnityView> GetAllLuckyDraws(BundleCategoryType bundleCategoryType) {
			using (var bytes = new MemoryStream()) {
				EnumProxy<BundleCategoryType>.Serialize(bytes, bundleCategoryType);

				var result = Service.GetAllLuckyDraws_2(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return ListProxy<LuckyDrawUnityView>.Deserialize(inputStream, LuckyDrawUnityViewProxy.Deserialize);
				}
			}
		}

		public List<MysteryBoxUnityView> GetAllMysteryBoxs() {
			using (var bytes = new MemoryStream()) {
				var result = Service.GetAllMysteryBoxs_1(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return ListProxy<MysteryBoxUnityView>.Deserialize(inputStream, MysteryBoxUnityViewProxy.Deserialize);
				}
			}
		}

		public List<MysteryBoxUnityView> GetAllMysteryBoxs(BundleCategoryType bundleCategoryType) {
			using (var bytes = new MemoryStream()) {
				EnumProxy<BundleCategoryType>.Serialize(bytes, bundleCategoryType);

				var result = Service.GetAllMysteryBoxs_2(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return ListProxy<MysteryBoxUnityView>.Deserialize(inputStream, MysteryBoxUnityViewProxy.Deserialize);
				}
			}
		}

		public List<BundleView> GetBundles(ChannelType channelType) {
			using (var bytes = new MemoryStream()) {
				EnumProxy<ChannelType>.Serialize(bytes, channelType);

				var result = Service.GetBundles(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return ListProxy<BundleView>.Deserialize(inputStream, BundleViewProxy.Deserialize);
				}
			}
		}

		public LuckyDrawUnityView GetLuckyDraw(int luckyDrawId) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, luckyDrawId);

				var result = Service.GetLuckyDraw(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return LuckyDrawUnityViewProxy.Deserialize(inputStream);
				}
			}
		}

		public MysteryBoxUnityView GetMysteryBox(int mysteryBoxId) {
			using (var bytes = new MemoryStream()) {
				Int32Proxy.Serialize(bytes, mysteryBoxId);

				var result = Service.GetMysteryBox(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return MysteryBoxUnityViewProxy.Deserialize(inputStream);
				}
			}
		}

		public UberStrikeItemShopClientView GetShop() {
			using (var bytes = new MemoryStream()) {
				var result = Service.GetShop(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return UberStrikeItemShopClientViewProxy.Deserialize(inputStream);
				}
			}
		}

		public int RollLuckyDraw(string authToken, int luckDrawId, ChannelType channel) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, luckDrawId);
				EnumProxy<ChannelType>.Serialize(bytes, channel);

				var result = Service.RollLuckyDraw(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}

		public int RollMysteryBox(string authToken, int mysteryBoxId, ChannelType channel) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, mysteryBoxId);
				EnumProxy<ChannelType>.Serialize(bytes, channel);

				var result = Service.RollMysteryBox(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return Int32Proxy.Deserialize(inputStream);
				}
			}
		}

		public bool UseComsumableItem(string authToken, int itemId) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, itemId);

				var result = Service.UseConsumableItem(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return BooleanProxy.Deserialize(inputStream);
				}
			}
		}

		public bool VerifyReceipt(string hashedReceipt) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, hashedReceipt);

				var result = Service.VerifyReceipt(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return BooleanProxy.Deserialize(inputStream);
				}
			}
		}
	}
}
