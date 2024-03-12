using Cmune.DataCenter.Common.Entities;
using Paradise.Realtime.Server;
using Paradise.WebServices;
using Paradise.WebServices.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using UberStrike.Core.Models;
using UberStrike.Core.Serialization;

namespace Paradise.Realtime {
	public class ModerationWebServiceClient : BaseWebServiceClient<IModerationWebServiceContract> {
		public static readonly ModerationWebServiceClient Instance;

		static ModerationWebServiceClient() {
			Instance = new ModerationWebServiceClient(
				masterHostname: BaseRealtimeApplication.Instance.Configuration.MasterHostname,
				port: BaseRealtimeApplication.Instance.Configuration.WebServicePort,
				serviceEndpoint: BaseRealtimeApplication.Instance.Configuration.WebServiceEndpoint,
				webServicePrefix: BaseRealtimeApplication.Instance.Configuration.WebServicePrefix,
				webServiceSuffix: BaseRealtimeApplication.Instance.Configuration.WebServiceSuffix
			);
		}

		public ModerationWebServiceClient(string masterHostname, int port, string serviceEndpoint, string webServicePrefix, string webServiceSuffix) : base(masterHostname, port, serviceEndpoint, $"{webServicePrefix}ModerationWebService{webServiceSuffix}") { }

		public bool BanPermanently(string authToken, int targetCmid, string reason) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, targetCmid);
				StringProxy.Serialize(bytes, reason);

				var result = Service.BanPermanently(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return BooleanProxy.Deserialize(inputStream);
				}
			}
		}

		public MemberOperationResult SetModerationFlag(string authToken, int targetCmid, ModerationFlag moderationFlag, DateTime expireTime, string reason) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, targetCmid);
				EnumProxy<ModerationFlag>.Serialize(bytes, moderationFlag);
				DateTimeProxy.Serialize(bytes, expireTime);
				StringProxy.Serialize(bytes, reason);

				var result = Service.SetModerationFlag(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return EnumProxy<MemberOperationResult>.Deserialize(inputStream);
				}
			}
		}

		public MemberOperationResult UnsetModerationFlag(string authToken, int targetCmid, ModerationFlag moderationFlag) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, targetCmid);
				EnumProxy<ModerationFlag>.Serialize(bytes, moderationFlag);

				var result = Service.UnsetModerationFlag(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return EnumProxy<MemberOperationResult>.Deserialize(inputStream);
				}
			}
		}

		public MemberOperationResult ClearModerationFlags(string authToken, int targetCmid) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);
				Int32Proxy.Serialize(bytes, targetCmid);

				var result = Service.ClearModerationFlags(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return EnumProxy<MemberOperationResult>.Deserialize(inputStream);
				}
			}
		}

		public List<CommActorInfo> GetNaughtyList(string authToken) {
			using (var bytes = new MemoryStream()) {
				StringProxy.Serialize(bytes, authToken);

				var result = Service.GetNaughtyList(Encrypt(bytes.ToArray()));

				using (var inputStream = new MemoryStream(Decrypt(result))) {
					return ListProxy<CommActorInfo>.Deserialize(inputStream, CommActorInfoProxy.Deserialize);
				}
			}
		}
	}
}
