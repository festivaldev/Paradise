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
		public static readonly UserWebServiceClient Instance = new UserWebServiceClient(
			endpointUrl: BaseRealtimeApplication.Instance.Configuration.WebServiceBaseUrl,
			webServicePrefix: BaseRealtimeApplication.Instance.Configuration.WebServicePrefix,
			webServiceSuffix: BaseRealtimeApplication.Instance.Configuration.WebServiceSuffix
		);

		public UserWebServiceClient(string endpointUrl, string webServicePrefix, string webServiceSuffix) : base(endpointUrl, $"{webServicePrefix}UserWebService{webServiceSuffix}") { }

		public bool AddItemTransaction(ItemTransactionView itemTransaction, string authToken) {
			using (var bytes = new MemoryStream()) {
				ItemTransactionViewProxy.Serialize(bytes, itemTransaction);
				StringProxy.Serialize(bytes, authToken);

				var result = Service.AddItemTransaction(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
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

				var result = Service.ChangeMemberName(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return EnumProxy<MemberOperationResult>.Deserialize(inputStream);
				}
			}
		}

		public bool DepositCredits(CurrencyDepositView depositTransaction, string authToken) {
			using (var bytes = new MemoryStream()) {
				CurrencyDepositViewProxy.Serialize(bytes, depositTransaction);
				StringProxy.Serialize(bytes, authToken);

				var result = Service.DepositCredits(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return BooleanProxy.Deserialize(inputStream);
				}
			}
		}

		public bool DepositPoints(PointDepositView depositTransaction, string authToken) {
			using (var bytes = new MemoryStream()) {
				PointDepositViewProxy.Serialize(bytes, depositTransaction);
				StringProxy.Serialize(bytes, authToken);

				var result = Service.DepositPoints(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return BooleanProxy.Deserialize(inputStream);
				}
			}
		}

		public List<string> GenerateNonDuplicatedMemberNames(string username) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, username);

				var result = Service.GenerateNonDuplicatedMemberNames(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return ListProxy<string>.Deserialize(inputStream, StringProxy.Deserialize);
				}
			}
		}

		public MemberWalletView GetCurrentDeposits(string authToken, int pageIndex, int elementPerPage) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, pageIndex);
				Int32Proxy.Serialize(bytes, elementPerPage);

				var result = Service.GetCurrencyDeposits(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return MemberWalletViewProxy.Deserialize(inputStream);
				}
			}
		}

		public List<ItemInventoryView> GetInventory(string authToken) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);

				var result = Service.GetInventory(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return ListProxy<ItemInventoryView>.Deserialize(inputStream, ItemInventoryViewProxy.Deserialize);
				}
			}
		}

		public ItemTransactionsViewModel GetItemTransactions(string authToken, int pageIndex, int elementPerPage) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, pageIndex);
				Int32Proxy.Serialize(bytes, elementPerPage);

				var result = Service.GetItemTransactions(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return ItemTransactionsViewModelProxy.Deserialize(inputStream);
				}
			}
		}

		public LoadoutView GetLoadout(string authToken) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);

				var result = Service.GetLoadout(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return LoadoutViewProxy.Deserialize(inputStream);
				}
			}
		}

		public UberstrikeUserViewModel GetMember(string authToken) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);

				var result = Service.GetMember(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return UberstrikeUserViewModelProxy.Deserialize(inputStream);
				}
			}
		}

		public List<MemberSessionDataView> GetMemberListSessionData(List<string> authTokens) {
			using (var bytes = new MemoryStream()) {
				ListProxy<string>.Serialize(bytes, authTokens, StringProxy.Serialize);

				var result = Service.GetMemberListSessionData(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return ListProxy<MemberSessionDataView>.Deserialize(inputStream, MemberSessionDataViewProxy.Deserialize);
				}
			}
		}

		public MemberSessionDataView GetMemberSessionData(string authToken) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);

				var result = Service.GetMemberSessionData(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return MemberSessionDataViewProxy.Deserialize(inputStream);
				}
			}
		}

		public MemberWalletView GetMemberWallet(string authToken) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);

				var result = Service.GetMemberWallet(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return MemberWalletViewProxy.Deserialize(inputStream);
				}
			}
		}

		public PointDepositsViewModel GetPointsDeposits(string authToken, int pageIndex, int elementPerPage) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, pageIndex);
				Int32Proxy.Serialize(bytes, elementPerPage);

				var result = Service.GetPointsDeposits(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return PointDepositsViewModelProxy.Deserialize(inputStream);
				}
			}
		}

		public bool IsDuplicateMemberName(string username) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, username);

				var result = Service.IsDuplicateMemberName(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return BooleanProxy.Deserialize(inputStream);
				}
			}
		}

		public MemberOperationResult UpdatePlayerStatistics(string authToken, PlayerStatisticsView playerStatistics) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				PlayerStatisticsViewProxy.Serialize(bytes, playerStatistics);

				var result = Service.UpdatePlayerStatistics(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return EnumProxy<MemberOperationResult>.Deserialize(inputStream);
				}
			}
		}
	}
}
