using Paradise.Core.Models;
using Paradise.Core.Serialization;
using Paradise.DataCenter.Common.Entities;
using Paradise.Realtime.Server;
using Paradise.WebServices;
using Paradise.WebServices.Contracts;
using System;
using System.Collections.Generic;
using System.IO;

namespace Paradise.Realtime {
	public class ModerationWebServiceClient : BaseWebServiceClient<IModerationWebServiceContract> {
		public static readonly ModerationWebServiceClient Instance = new ModerationWebServiceClient(
			endpointUrl: BaseRealtimeApplication.Instance.Configuration.WebServiceBaseUrl,
			webServicePrefix: BaseRealtimeApplication.Instance.Configuration.WebServicePrefix,
			webServiceSuffix: BaseRealtimeApplication.Instance.Configuration.WebServiceSuffix
		);

		public ModerationWebServiceClient(string endpointUrl, string webServicePrefix, string webServiceSuffix) : base(endpointUrl, $"{webServicePrefix}ModerationWebService{webServiceSuffix}") { }

		public MemberOperationResult OpPlayer(string authToken, int targetCmid, MemberAccessLevel accessLevel) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, targetCmid);
				EnumProxy<MemberAccessLevel>.Serialize(bytes, accessLevel);

				var result = Service.OpPlayer(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return EnumProxy<MemberOperationResult>.Deserialize(inputStream);
				}
			}
		}

		public MemberOperationResult DeopPlayer(string authToken, int targetCmid) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, targetCmid);

				var result = Service.DeopPlayer(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return EnumProxy<MemberOperationResult>.Deserialize(inputStream);
				}
			}
		}

		//public MemberOperationResult BanPermanently(string authToken, int targetCmid, string reason) {
		//	using (var bytes = new MemoryStream()) {
		//		StringProxy.Serialize(bytes, authToken);
		//		Int32Proxy.Serialize(bytes, targetCmid);
		//		StringProxy.Serialize(bytes, reason);

		//		var result = Service.BanPermanently(bytes.ToArray());

		//		using (var inputStream = new MemoryStream(result)) {
		//			return EnumProxy<MemberOperationResult>.Deserialize(inputStream);
		//		}
		//	}
		//}

		//public MemberOperationResult UnbanPlayer(string authToken, int targetCmid) {
		//	using (var bytes = new MemoryStream()) {
		//		StringProxy.Serialize(bytes, authToken);
		//		Int32Proxy.Serialize(bytes, targetCmid);

		//		var result = Service.UnbanPlayer(bytes.ToArray());

		//		using (var inputStream = new MemoryStream(result)) {
		//			return EnumProxy<MemberOperationResult>.Deserialize(inputStream);
		//		}
		//	}
		//}

		//public MemberOperationResult MutePlayer(string authToken, int durationInMinutes, int mutedCmid) {
		//	using (var bytes = new MemoryStream()) {
		//		StringProxy.Serialize(bytes, authToken);
		//		Int32Proxy.Serialize(bytes, durationInMinutes);
		//		Int32Proxy.Serialize(bytes, mutedCmid);

		//		var result = Service.MutePlayer(bytes.ToArray());

		//		using (var inputStream = new MemoryStream(result)) {
		//			return EnumProxy<MemberOperationResult>.Deserialize(inputStream);
		//		}
		//	}
		//}

		//public MemberOperationResult UnmutePlayer(string authToken, int mutedCmid) {
		//	using (var bytes = new MemoryStream()) {
		//		StringProxy.Serialize(bytes, authToken);
		//		Int32Proxy.Serialize(bytes, mutedCmid);

		//		var result = Service.UnmutePlayer(bytes.ToArray());

		//		using (var inputStream = new MemoryStream(result)) {
		//			return EnumProxy<MemberOperationResult>.Deserialize(inputStream);
		//		}
		//	}
		//}

		//public List<CommActorInfo> GetNaughtyList(string authToken) {
		//	using (var bytes = new MemoryStream()) {
		//		StringProxy.Serialize(bytes, authToken);

		//		var result = Service.GetNaughtyList(bytes.ToArray());

		//		using (var inputStream = new MemoryStream(result)) {
		//			return ListProxy<CommActorInfo>.Deserialize(inputStream, CommActorInfoProxy.Deserialize);
		//		}
		//	}
		//}

		public MemberOperationResult SetModerationFlag(string authToken, int targetCmid, ModerationFlag moderationFlag, DateTime expireTime, string reason) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, targetCmid);
				EnumProxy<ModerationFlag>.Serialize(bytes, moderationFlag);
				DateTimeProxy.Serialize(bytes, expireTime);
				StringProxy.Serialize(bytes, reason);

				var result = Service.SetModerationFlag(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return EnumProxy<MemberOperationResult>.Deserialize(inputStream);
				}
			}
		}

		public MemberOperationResult UnsetModerationFlag(string authToken, int targetCmid, ModerationFlag moderationFlag) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, targetCmid);
				EnumProxy<ModerationFlag>.Serialize(bytes, moderationFlag);

				var result = Service.UnsetModerationFlag(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return EnumProxy<MemberOperationResult>.Deserialize(inputStream);
				}
			}
		}

		public MemberOperationResult ClearModerationFlags(string authToken, int targetCmid) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, targetCmid);

				var result = Service.ClearModerationFlags(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return EnumProxy<MemberOperationResult>.Deserialize(inputStream);
				}
			}
		}

		public List<CommActorInfo> GetNaughtyList(string authToken) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);

				var result = Service.GetNaughtyList(bytes.ToArray());

				using (var inputStream = new MemoryStream(result)) {
					return ListProxy<CommActorInfo>.Deserialize(inputStream, CommActorInfoProxy.Deserialize);
				}
			}
		}
	}
}
