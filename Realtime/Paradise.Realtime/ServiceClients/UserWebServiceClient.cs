using Paradise.Core.Models.Views;
using Paradise.Core.Serialization;
using Paradise.Core.ViewModel;
using Paradise.DataCenter.Common.Entities;
using Paradise.Realtime.Server;
using Paradise.WebServices.Contracts;
using System.Collections.Generic;
using System.IO;

namespace Paradise.Realtime {
	public class UserWebServiceClient : BaseWebServiceClient<IUserWebServiceContract> {
		public static readonly UserWebServiceClient Instance;

		static UserWebServiceClient() {
			Instance = new UserWebServiceClient(
				masterUrl: BaseRealtimeApplication.Instance.Configuration.MasterServerUrl,
				port: BaseRealtimeApplication.Instance.Configuration.WebServicePort,
				serviceEndpoint: BaseRealtimeApplication.Instance.Configuration.WebServiceEndpoint,
				webServicePrefix: BaseRealtimeApplication.Instance.Configuration.WebServicePrefix,
				webServiceSuffix: BaseRealtimeApplication.Instance.Configuration.WebServiceSuffix
			);
		}

		public UserWebServiceClient(string masterUrl, int port, string serviceEndpoint, string webServicePrefix, string webServiceSuffix) : base(masterUrl, port, serviceEndpoint, $"{webServicePrefix}UserWebService{webServiceSuffix}") { }

		public bool AddItemTransaction(ItemTransactionView itemTransaction, string authToken) {
			using (var bytes = new MemoryStream()) {
				ItemTransactionViewProxy.Serialize(bytes, itemTransaction);
				StringProxy.Serialize(bytes, authToken);

				var result = Service.AddItemTransaction(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return BooleanProxy.Deserialize(inputStream);
				}
			}
		}

		public MemberOperationResult ChangeMemberName(string authToken, string name, string locale, string machineId) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				StringProxy.Serialize(bytes, name);
				StringProxy.Serialize(bytes, locale);
				StringProxy.Serialize(bytes, machineId);

				var result = Service.ChangeMemberName(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return EnumProxy<MemberOperationResult>.Deserialize(inputStream);
				}
			}
		}

		public bool DepositCredits(CurrencyDepositView depositTransaction, string authToken) {
			using (var bytes = new MemoryStream()) {
				CurrencyDepositViewProxy.Serialize(bytes, depositTransaction);
				StringProxy.Serialize(bytes, authToken);

				var result = Service.DepositCredits(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return BooleanProxy.Deserialize(inputStream);
				}
			}
		}

		public bool DepositPoints(PointDepositView depositTransaction, string authToken) {
			using (var bytes = new MemoryStream()) {
				PointDepositViewProxy.Serialize(bytes, depositTransaction);
				StringProxy.Serialize(bytes, authToken);

				var result = Service.DepositPoints(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return BooleanProxy.Deserialize(inputStream);
				}
			}
		}

		public List<string> GenerateNonDuplicatedMemberNames(string username) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, username);

				var result = Service.GenerateNonDuplicatedMemberNames(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return ListProxy<string>.Deserialize(inputStream, StringProxy.Deserialize);
				}
			}
		}

		public CurrencyDepositsViewModel GetCurrentDeposits(string authToken, int pageIndex, int elementPerPage) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, pageIndex);
				Int32Proxy.Serialize(bytes, elementPerPage);

				var result = Service.GetCurrencyDeposits(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return CurrencyDepositsViewModelProxy.Deserialize(inputStream);
				}
			}
		}

		public List<ItemInventoryView> GetInventory(string authToken) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);

				var result = Service.GetInventory(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return ListProxy<ItemInventoryView>.Deserialize(inputStream, ItemInventoryViewProxy.Deserialize);
				}
			}
		}

		public ItemTransactionsViewModel GetItemTransactions(string authToken, int pageIndex, int elementPerPage) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, pageIndex);
				Int32Proxy.Serialize(bytes, elementPerPage);

				var result = Service.GetItemTransactions(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return ItemTransactionsViewModelProxy.Deserialize(inputStream);
				}
			}
		}

		public LoadoutView GetLoadout(string authToken) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);

				var result = Service.GetLoadout(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return LoadoutViewProxy.Deserialize(inputStream);
				}
			}
		}

		public UberstrikeUserViewModel GetMember(string authToken) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);

				var result = Service.GetMember(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return UberstrikeUserViewModelProxy.Deserialize(inputStream);
				}
			}
		}

		public List<MemberSessionDataView> GetMemberListSessionData(List<string> authTokens) {
			using (var bytes = new MemoryStream()) {
				ListProxy<string>.Serialize(bytes, authTokens, StringProxy.Serialize);

				var result = Service.GetMemberListSessionData(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return ListProxy<MemberSessionDataView>.Deserialize(inputStream, MemberSessionDataViewProxy.Deserialize);
				}
			}
		}

		public MemberSessionDataView GetMemberSessionData(string authToken) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);

				var result = Service.GetMemberSessionData(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return MemberSessionDataViewProxy.Deserialize(inputStream);
				}
			}
		}

		public MemberWalletView GetMemberWallet(string authToken) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);

				var result = Service.GetMemberWallet(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return MemberWalletViewProxy.Deserialize(inputStream);
				}
			}
		}

		public PointDepositsViewModel GetPointsDeposits(string authToken, int pageIndex, int elementPerPage) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, pageIndex);
				Int32Proxy.Serialize(bytes, elementPerPage);

				var result = Service.GetPointsDeposits(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return PointDepositsViewModelProxy.Deserialize(inputStream);
				}
			}
		}

		public bool IsDuplicateMemberName(string username) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, username);

				var result = Service.IsDuplicateMemberName(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return BooleanProxy.Deserialize(inputStream);
				}
			}
		}

		public MemberOperationResult SetLoadout(string authToken, LoadoutView loadoutView) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				LoadoutViewProxy.Serialize(bytes, loadoutView);

				var result = Service.SetLoadout(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return EnumProxy<MemberOperationResult>.Deserialize(inputStream);
				}
			}
		}

		public MemberOperationResult UpdatePlayerStatistics(string authToken, PlayerStatisticsView playerStatistics) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				PlayerStatisticsViewProxy.Serialize(bytes, playerStatistics);

				var result = Service.UpdatePlayerStatistics(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return EnumProxy<MemberOperationResult>.Deserialize(inputStream);
				}
			}
		}
	}
}
