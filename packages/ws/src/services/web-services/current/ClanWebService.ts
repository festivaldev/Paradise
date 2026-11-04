/*
 * Copyright (C) 2017, 2022-2026 Team FESTIVAL
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

import ParadiseService from '@/ParadiseService';
import { ProfanityFilter } from '@/ProfanityFilter';
import { XpPointsUtil } from '@/utils';
import { ApiVersion, UberstrikeInventoryItem } from '@/utils/enums';
import {
  Clan,
  ClanMember,
  ContactRequest,
  GroupInvitation,
  PlayerInventoryItem,
  PlayerStatistics,
  PublicProfile
} from '@festivaldev/paradise-models';
import {
  ClanCreationReturnView,
  ClanMemberView,
  ClanRequestAcceptView,
  ClanRequestDeclineView,
  ClanView,
  ContactRequestStatus,
  GroupInvitationView,
  GroupPosition,
  GroupType,
  MemberAccessLevel
} from '@festivaldev/uberstrike-js/Cmune/DataCenter/Common/Entities';
import {
  BooleanProxy,
  ClanCreationReturnViewProxy,
  ClanRequestAcceptViewProxy,
  ClanRequestDeclineViewProxy,
  ClanViewProxy,
  GroupCreationViewProxy,
  GroupInvitationViewProxy,
  Int32Proxy,
  ListProxy,
  MemberPositionUpdateViewProxy,
  StringProxy
} from '@festivaldev/uberstrike-js/UberStrike/Core/Serialization';
import { In } from 'typeorm';
import BaseWebService from '../BaseWebService';

enum ClanCreationResultCode {
  Success,
  InvalidClanName,
  ClanCollision,
  ClanNameTaken,
  InvalidClanTag,
  InvalidClanMotto = 8,
  ClanTagTaken = 10,
  RequirementPlayerLevel = 100,
  RequirementPlayerFriends,
  RequirementClanLicense
}

enum ClanActionResultCode {
  Success,
  Error
}

export default class ClanWebService extends BaseWebService {
  static override get ServiceName(): string {
    return 'ClanWebService';
  }

  static override get ServiceVersion(): string {
    return ApiVersion.Current;
  }

  private static readonly ProfanityFilter: ProfanityFilter = new ProfanityFilter();

  static async AcceptClanInvitation(data: number[], outputStream: number[]): Promise<number[] | null> {
    const isEncrypted = this.isEncrypted(data);
    const bytes = isEncrypted
      ? this.CryptoPolicy.RijndaelDecrypt(data, this.EncryptionPassPhrase, this.EncryptionInitVector)
      : data;

    try {
      const clanInvitationId = Int32Proxy.Deserialize(bytes);
      const authToken = StringProxy.Deserialize(bytes);

      this.debugEndpointData({ clanInvitationId, authToken });

      const session = await ParadiseService.Instance.SessionManager.findSessionForSteamUser(authToken);
      if (session) {
        const steamMember = await session.SteamMember;

        if (steamMember) {
          const publicProfile = await PublicProfile.findOne({ where: { Cmid: steamMember.Cmid } });

          if (publicProfile) {
            const groupInvitation = await GroupInvitation.findOne({ where: { GroupInvitationId: clanInvitationId } });

            if (
              groupInvitation &&
              groupInvitation.InviteeCmid === publicProfile.Cmid &&
              !(await ClanMember.findOne({ where: { Cmid: groupInvitation.InviteeCmid } }))
            ) {
              const clan = await Clan.findOne({ where: { GroupId: groupInvitation.GroupId } });

              if (clan) {
                const memberCount = await ClanMember.count({ where: { GroupId: clan.GroupId } });

                if (clan.MembersLimit && memberCount >= clan.MembersLimit) {
                  ClanRequestAcceptViewProxy.Serialize(
                    outputStream,
                    new ClanRequestAcceptView({
                      ActionResult: ClanActionResultCode.Error,
                      ClanRequestId: clanInvitationId
                    })
                  );

                  return isEncrypted
                    ? this.CryptoPolicy.RijndaelEncrypt(
                        outputStream,
                        this.EncryptionPassPhrase,
                        this.EncryptionInitVector
                      )
                    : outputStream;
                }

                await ClanMember.insert({
                  GroupId: clan.GroupId,
                  Cmid: publicProfile.Cmid,
                  Name: publicProfile.Name,
                  Position: GroupPosition.Member,
                  JoiningDate: new Date(),
                  Lastlogin: publicProfile.LastLoginDate
                });

                await GroupInvitation.delete(groupInvitation.GroupInvitationId);

                ClanRequestAcceptViewProxy.Serialize(
                  outputStream,
                  new ClanRequestAcceptView({
                    ActionResult: ClanActionResultCode.Success,
                    ClanRequestId: clanInvitationId,
                    ClanView: clan as ClanView
                  })
                );

                return isEncrypted
                  ? this.CryptoPolicy.RijndaelEncrypt(
                      outputStream,
                      this.EncryptionPassPhrase,
                      this.EncryptionInitVector
                    )
                  : outputStream;
              }
            }
          }
        }

        ClanRequestAcceptViewProxy.Serialize(
          outputStream,
          new ClanRequestAcceptView({
            ActionResult: ClanActionResultCode.Error,
            ClanRequestId: clanInvitationId
          })
        );
      }

      return isEncrypted
        ? this.CryptoPolicy.RijndaelEncrypt(outputStream, this.EncryptionPassPhrase, this.EncryptionInitVector)
        : outputStream;
    } catch (error) {
      this.handleEndpointError('AcceptClanInvitation', error);
    }

    return null;
  }

  static async CancelInvitation(data: number[], outputStream: number[]): Promise<number[] | null> {
    const isEncrypted = this.isEncrypted(data);
    const bytes = isEncrypted
      ? this.CryptoPolicy.RijndaelDecrypt(data, this.EncryptionPassPhrase, this.EncryptionInitVector)
      : data;

    try {
      const groupInvitationId = Int32Proxy.Deserialize(bytes);
      const authToken = StringProxy.Deserialize(bytes);

      this.debugEndpointData({ groupInvitationId, authToken });

      const session = await ParadiseService.Instance.SessionManager.findSessionForSteamUser(authToken);
      if (session) {
        const steamMember = await session.SteamMember;

        if (steamMember) {
          const groupInvitation = await GroupInvitation.findOne({ where: { GroupInvitationId: groupInvitationId } });

          if (groupInvitation && groupInvitation.InviterCmid === steamMember.Cmid) {
            await GroupInvitation.delete(groupInvitation.GroupInvitationId);

            Int32Proxy.Serialize(outputStream, ClanActionResultCode.Success);
          } else {
            Int32Proxy.Serialize(outputStream, ClanActionResultCode.Error);
          }
        }
      }

      return isEncrypted
        ? this.CryptoPolicy.RijndaelEncrypt(outputStream, this.EncryptionPassPhrase, this.EncryptionInitVector)
        : outputStream;
    } catch (error) {
      this.handleEndpointError('CancelInvitation', error);
    }

    return null;
  }

  static async CanOwnAClan(data: number[], outputStream: number[]): Promise<number[] | null> {
    const isEncrypted = this.isEncrypted(data);
    const bytes = isEncrypted
      ? this.CryptoPolicy.RijndaelDecrypt(data, this.EncryptionPassPhrase, this.EncryptionInitVector)
      : data;

    try {
      const authToken = StringProxy.Deserialize(bytes);

      this.debugEndpointData({ authToken });

      let canOwnClan = false;

      const session = await ParadiseService.Instance.SessionManager.findSessionForSteamUser(authToken);
      if (session) {
        const steamMember = await session.SteamMember;

        if (steamMember) {
          const publicProfile = await PublicProfile.findOne({ where: { Cmid: steamMember.Cmid } });

          if (publicProfile) {
            if (!(await ClanMember.findOne({ where: { Cmid: steamMember.Cmid } }))) {
              if (publicProfile.AccessLevel === MemberAccessLevel.Admin) {
                canOwnClan = true;
              } else {
                const [friends, statistics, license] = await Promise.all([
                  ContactRequest.find({
                    where: [
                      { InitiatorCmid: publicProfile.Cmid, Status: ContactRequestStatus.Accepted },
                      { ReceiverCmid: publicProfile.Cmid, Status: ContactRequestStatus.Accepted }
                    ]
                  }),
                  PlayerStatistics.findOne({ where: { Cmid: publicProfile.Cmid } }),
                  PlayerInventoryItem.findOne({
                    where: { Cmid: publicProfile.Cmid, ItemId: UberstrikeInventoryItem.ClanLicense }
                  })
                ]);

                canOwnClan =
                  friends.length > 0 && !!statistics && XpPointsUtil.GetLevelForXp(statistics.Xp) >= 4 && !!license;
              }
            }
          }
        }
      }

      BooleanProxy.Serialize(outputStream, canOwnClan);
      return isEncrypted
        ? this.CryptoPolicy.RijndaelEncrypt(outputStream, this.EncryptionPassPhrase, this.EncryptionInitVector)
        : outputStream;
    } catch (error) {
      this.handleEndpointError('CanOwnAClan', error);
    }

    return null;
  }

  static async CreateClan(data: number[], outputStream: number[]): Promise<number[] | null> {
    const isEncrypted = this.isEncrypted(data);
    const bytes = isEncrypted
      ? this.CryptoPolicy.RijndaelDecrypt(data, this.EncryptionPassPhrase, this.EncryptionInitVector)
      : data;

    try {
      const createClanData = GroupCreationViewProxy.Deserialize(bytes);

      this.debugEndpointData({ createClanData });

      const session = await ParadiseService.Instance.SessionManager.findSessionForSteamUser(createClanData.AuthToken);
      if (session) {
        const steamMember = await session.SteamMember;

        if (steamMember) {
          const publicProfile = await PublicProfile.findOne({ where: { Cmid: steamMember.Cmid } });

          if (publicProfile) {
            if (await ClanMember.findOne({ where: { Cmid: steamMember.Cmid } })) {
              // "Clan Collision", "You are already member of another clan, please leave first before creating your own."

              ClanCreationReturnViewProxy.Serialize(
                outputStream,
                new ClanCreationReturnView({
                  ResultCode: ClanCreationResultCode.ClanCollision
                })
              );

              return isEncrypted
                ? this.CryptoPolicy.RijndaelEncrypt(outputStream, this.EncryptionPassPhrase, this.EncryptionInitVector)
                : outputStream;
            }

            const friendsList = await ContactRequest.find({
              where: [
                { InitiatorCmid: publicProfile.Cmid, Status: ContactRequestStatus.Accepted },
                { ReceiverCmid: publicProfile.Cmid, Status: ContactRequestStatus.Accepted }
              ]
            });
            const playerStatistics = await PlayerStatistics.findOne({ where: { Cmid: steamMember.Cmid } });
            const hasClanLicense =
              (await PlayerInventoryItem.findOne({
                where: { Cmid: steamMember.Cmid, ItemId: UberstrikeInventoryItem.ClanLicense }
              })) != null;

            if (
              createClanData.Name.length < 3 ||
              !createClanData.Name.match(/^[a-zA-Z0-9_]+$/) ||
              this.ProfanityFilter.DetectAllProfanities(createClanData.Name).length > 0
            ) {
              // "Invalid Clan Name", "The name '" + name + "' is not valid, please modify it."

              ClanCreationReturnViewProxy.Serialize(
                outputStream,
                new ClanCreationReturnView({
                  ResultCode: ClanCreationResultCode.InvalidClanName
                })
              );
            } else if (await Clan.findOne({ where: { Name: createClanData.Name } })) {
              // "Clan Name", "The name '" + name + "' is already taken, try another one."

              ClanCreationReturnViewProxy.Serialize(
                outputStream,
                new ClanCreationReturnView({
                  ResultCode: ClanCreationResultCode.ClanNameTaken
                })
              );
            } else if (this.ProfanityFilter.DetectAllProfanities(createClanData.Tag).length > 0) {
              // "Invalid Clan Tag", "The tag '" + tag + "' is not valid, please modify it."

              ClanCreationReturnViewProxy.Serialize(
                outputStream,
                new ClanCreationReturnView({
                  ResultCode: ClanCreationResultCode.InvalidClanTag
                })
              );
            } else if (this.ProfanityFilter.DetectAllProfanities(createClanData.Motto).length > 0) {
              // "Invalid Clan Motto", "The motto '" + motto + "' is not valid, please modify it."

              ClanCreationReturnViewProxy.Serialize(
                outputStream,
                new ClanCreationReturnView({
                  ResultCode: ClanCreationResultCode.InvalidClanMotto
                })
              );
            } else if ((await Clan.findOne({ where: { Tag: createClanData.Tag } })) != null) {
              // "Clan Tag", "The tag '" + tag + "' is already taken, try another one."

              ClanCreationReturnViewProxy.Serialize(
                outputStream,
                new ClanCreationReturnView({
                  ResultCode: ClanCreationResultCode.ClanTagTaken
                })
              );
            } else if (
              (!playerStatistics || XpPointsUtil.GetLevelForXp(playerStatistics.Xp) < 4) &&
              publicProfile.AccessLevel !== MemberAccessLevel.Admin
            ) {
              // "Sorry", "You don't fulfill the minimal requirements to create your own clan."

              ClanCreationReturnViewProxy.Serialize(
                outputStream,
                new ClanCreationReturnView({
                  ResultCode: ClanCreationResultCode.RequirementPlayerLevel
                })
              );
            } else if (!friendsList.length && publicProfile.AccessLevel !== MemberAccessLevel.Admin) {
              ClanCreationReturnViewProxy.Serialize(
                outputStream,
                new ClanCreationReturnView({
                  ResultCode: ClanCreationResultCode.RequirementPlayerFriends
                })
              );
            } else if (!hasClanLicense && publicProfile.AccessLevel !== MemberAccessLevel.Admin) {
              ClanCreationReturnViewProxy.Serialize(
                outputStream,
                new ClanCreationReturnView({
                  ResultCode: ClanCreationResultCode.RequirementClanLicense
                })
              );
            } else {
              const clan = await Clan.save(
                Clan.create({
                  GroupId: Math.randomInt(),
                  Name: createClanData.Name,
                  Motto: createClanData.Motto,
                  FoundingDate: new Date(),
                  Type: GroupType.Clan,
                  LastUpdated: new Date(),
                  Tag: createClanData.Tag,
                  MembersLimit: 12,
                  ApplicationId: createClanData.ApplicationId,
                  OwnerCmid: publicProfile.Cmid,
                  Owner: publicProfile
                })
              );

              const clanMember = await ClanMember.save(
                ClanMember.create({
                  GroupId: clan.GroupId,
                  Cmid: publicProfile.Cmid,
                  Name: publicProfile.Name,
                  Position: GroupPosition.Leader,
                  JoiningDate: new Date(),
                  Lastlogin: publicProfile.LastLoginDate,
                  PublicProfile: publicProfile
                })
              );

              await PlayerInventoryItem.delete({
                ItemId: UberstrikeInventoryItem.ClanLicense,
                Cmid: steamMember.Cmid
              });

              ClanCreationReturnViewProxy.Serialize(
                outputStream,
                new ClanCreationReturnView({
                  ResultCode: ClanCreationResultCode.Success,
                  ClanView: {
                    ...(clan as ClanView),
                    OwnerName: clan.OwnerName,
                    Members: [clanMember as ClanMemberView]
                  }
                })
              );
            }
          }
        }
      }

      return isEncrypted
        ? this.CryptoPolicy.RijndaelEncrypt(outputStream, this.EncryptionPassPhrase, this.EncryptionInitVector)
        : outputStream;
    } catch (error) {
      this.handleEndpointError('CreateClan', error);
    }

    return null;
  }

  static async DeclineClanInvitation(data: number[], outputStream: number[]): Promise<number[] | null> {
    const isEncrypted = this.isEncrypted(data);
    const bytes = isEncrypted
      ? this.CryptoPolicy.RijndaelDecrypt(data, this.EncryptionPassPhrase, this.EncryptionInitVector)
      : data;

    try {
      const clanInvitationId = Int32Proxy.Deserialize(bytes);
      const authToken = StringProxy.Deserialize(bytes);

      this.debugEndpointData({ clanInvitationId, authToken });

      const session = await ParadiseService.Instance.SessionManager.findSessionForSteamUser(authToken);
      if (session) {
        const steamMember = await session.SteamMember;

        if (steamMember) {
          const groupInvitation = await GroupInvitation.findOne({ where: { GroupInvitationId: clanInvitationId } });

          if (groupInvitation && groupInvitation.InviteeCmid === steamMember.Cmid) {
            await GroupInvitation.delete(groupInvitation.GroupInvitationId);

            ClanRequestDeclineViewProxy.Serialize(
              outputStream,
              new ClanRequestDeclineView({
                ActionResult: ClanActionResultCode.Success,
                ClanRequestId: clanInvitationId
              })
            );
          } else {
            ClanRequestDeclineViewProxy.Serialize(
              outputStream,
              new ClanRequestDeclineView({
                ActionResult: ClanActionResultCode.Error,
                ClanRequestId: clanInvitationId
              })
            );
          }
        }
      }

      return isEncrypted
        ? this.CryptoPolicy.RijndaelEncrypt(outputStream, this.EncryptionPassPhrase, this.EncryptionInitVector)
        : outputStream;
    } catch (error) {
      this.handleEndpointError('DeclineClanInvitation', error);
    }

    return null;
  }

  static async DisbandGroup(data: number[], outputStream: number[]): Promise<number[] | null> {
    const isEncrypted = this.isEncrypted(data);
    const bytes = isEncrypted
      ? this.CryptoPolicy.RijndaelDecrypt(data, this.EncryptionPassPhrase, this.EncryptionInitVector)
      : data;

    try {
      const groupId = Int32Proxy.Deserialize(bytes);
      const authToken = StringProxy.Deserialize(bytes);

      this.debugEndpointData({ groupId, authToken });

      const session = await ParadiseService.Instance.SessionManager.findSessionForSteamUser(authToken);
      if (session) {
        const steamMember = await session.SteamMember;

        if (steamMember) {
          const clan = await Clan.findOne({
            where: {
              GroupId: groupId
            },
            relations: {
              Members: {
                PublicProfile: true
              }
            }
          });

          if (clan && clan.Members.find((_) => _.Cmid === steamMember.Cmid && _.Position === GroupPosition.Leader)) {
            const memberCmids = clan.Members.map((_) => _.Cmid);

            if (memberCmids.length) {
              await PublicProfile.update(
                {
                  Cmid: In(memberCmids)
                },
                {
                  GroupTag: ''
                }
              );
            }

            await Clan.delete(groupId);
            Int32Proxy.Serialize(outputStream, 0);
          }
        }
      }

      return isEncrypted
        ? this.CryptoPolicy.RijndaelEncrypt(outputStream, this.EncryptionPassPhrase, this.EncryptionInitVector)
        : outputStream;
    } catch (error) {
      this.handleEndpointError('DisbandGroup', error);
    }

    return null;
  }

  static async GetAllGroupInvitations(data: number[], outputStream: number[]): Promise<number[] | null> {
    const isEncrypted = this.isEncrypted(data);
    const bytes = isEncrypted
      ? this.CryptoPolicy.RijndaelDecrypt(data, this.EncryptionPassPhrase, this.EncryptionInitVector)
      : data;

    try {
      const authToken = StringProxy.Deserialize(bytes);

      this.debugEndpointData({ authToken });

      const session = await ParadiseService.Instance.SessionManager.findSessionForSteamUser(authToken);
      if (session) {
        const steamMember = await session.SteamMember;

        if (steamMember) {
          const groupInvitations = await GroupInvitation.find({ where: { InviteeCmid: steamMember.Cmid } });

          ListProxy.Serialize<GroupInvitationView>(
            outputStream,
            groupInvitations as GroupInvitationView[],
            GroupInvitationViewProxy.Serialize
          );
        }
      }

      return isEncrypted
        ? this.CryptoPolicy.RijndaelEncrypt(outputStream, this.EncryptionPassPhrase, this.EncryptionInitVector)
        : outputStream;
    } catch (error) {
      this.handleEndpointError('GetAllGroupInvitations', error);
    }

    return null;
  }

  static async GetMyClanId(data: number[], outputStream: number[]): Promise<number[] | null> {
    const isEncrypted = this.isEncrypted(data);
    const bytes = isEncrypted
      ? this.CryptoPolicy.RijndaelDecrypt(data, this.EncryptionPassPhrase, this.EncryptionInitVector)
      : data;

    try {
      const authToken = StringProxy.Deserialize(bytes);

      this.debugEndpointData({ authToken });

      const session = await ParadiseService.Instance.SessionManager.findSessionForSteamUser(authToken);
      if (session) {
        const steamMember = await session.SteamMember;

        if (steamMember) {
          // const clans = await Clan.find({
          //   relations: {
          //     Members: true
          //   }
          // });

          // for (const clan of clans) {
          //   if (await clan.Members.find((_) => _.Cmid === steamMember.Cmid)) {
          //     Int32Proxy.Serialize(outputStream, clan.GroupId);

          //     break;
          //   }
          // }

          const clanMember = await ClanMember.findOne({ where: { Cmid: steamMember.Cmid } });

          Int32Proxy.Serialize(outputStream, clanMember?.GroupId ?? 0);
        }
      }

      return isEncrypted
        ? this.CryptoPolicy.RijndaelEncrypt(outputStream, this.EncryptionPassPhrase, this.EncryptionInitVector)
        : outputStream;
    } catch (error) {
      this.handleEndpointError('GetMyClanId', error);
    }

    return null;
  }

  static async GetOwnClan(data: number[], outputStream: number[]): Promise<number[] | null> {
    const isEncrypted = this.isEncrypted(data);
    const bytes = isEncrypted
      ? this.CryptoPolicy.RijndaelDecrypt(data, this.EncryptionPassPhrase, this.EncryptionInitVector)
      : data;

    try {
      const authToken = StringProxy.Deserialize(bytes);

      this.debugEndpointData({ authToken });

      const session = await ParadiseService.Instance.SessionManager.findSessionForSteamUser(authToken);
      if (session) {
        const steamMember = await session.SteamMember;

        if (steamMember) {
          const clans = await Clan.find({
            relations: {
              Owner: true,
              Members: {
                PublicProfile: true
              }
            }
          });

          for (const clan of clans) {
            if (clan.Members.find((_) => _.Cmid === steamMember.Cmid)) {
              ClanViewProxy.Serialize(outputStream, new ClanView({ ...(clan as ClanView), OwnerName: clan.OwnerName }));

              break;
            }
          }
        }
      }

      return isEncrypted
        ? this.CryptoPolicy.RijndaelEncrypt(outputStream, this.EncryptionPassPhrase, this.EncryptionInitVector)
        : outputStream;
    } catch (error) {
      this.handleEndpointError('GetOwnClan', error);
    }

    return null;
  }

  static async GetPendingGroupInvitations(data: number[], outputStream: number[]): Promise<number[] | null> {
    const isEncrypted = this.isEncrypted(data);
    const bytes = isEncrypted
      ? this.CryptoPolicy.RijndaelDecrypt(data, this.EncryptionPassPhrase, this.EncryptionInitVector)
      : data;

    try {
      const groupId = Int32Proxy.Deserialize(bytes);
      const authToken = StringProxy.Deserialize(bytes);

      this.debugEndpointData({ authToken });

      const session = await ParadiseService.Instance.SessionManager.findSessionForSteamUser(authToken);
      if (session) {
        const steamMember = await session.SteamMember;

        if (steamMember) {
          const groupInvitations = await GroupInvitation.find({
            where: { GroupId: groupId, InviterCmid: steamMember.Cmid }
          });

          ListProxy.Serialize<GroupInvitationView>(
            outputStream,
            groupInvitations as GroupInvitationView[],
            GroupInvitationViewProxy.Serialize
          );
        }
      }

      return isEncrypted
        ? this.CryptoPolicy.RijndaelEncrypt(outputStream, this.EncryptionPassPhrase, this.EncryptionInitVector)
        : outputStream;
    } catch (error) {
      this.handleEndpointError('GetPendingGroupInvitations', error);
    }

    return null;
  }

  static async InviteMemberToJoinAGroup(data: number[], outputStream: number[]): Promise<number[] | null> {
    const isEncrypted = this.isEncrypted(data);
    const bytes = isEncrypted
      ? this.CryptoPolicy.RijndaelDecrypt(data, this.EncryptionPassPhrase, this.EncryptionInitVector)
      : data;

    try {
      const clanId = Int32Proxy.Deserialize(bytes);
      const authToken = StringProxy.Deserialize(bytes);
      const inviteeCmid = Int32Proxy.Deserialize(bytes);
      const message = StringProxy.Deserialize(bytes);

      this.debugEndpointData({ authToken });

      const session = await ParadiseService.Instance.SessionManager.findSessionForSteamUser(authToken);
      if (session) {
        const steamMember = await session.SteamMember;

        if (steamMember) {
          const publicProfile = await PublicProfile.findOne({ where: { Cmid: steamMember.Cmid } });
          const inviteeProfile = await PublicProfile.findOne({ where: { Cmid: inviteeCmid } });

          if (publicProfile && inviteeProfile) {
            const clan = await Clan.findOne({ where: { GroupId: clanId } });
            const inviterMember = await ClanMember.findOne({ where: { GroupId: clanId, Cmid: publicProfile.Cmid } });
            const inviteeMember = await ClanMember.findOne({ where: { GroupId: clanId, Cmid: inviteeProfile.Cmid } });
            const existingInvite = await GroupInvitation.findOne({
              where: { GroupId: clanId, InviteeCmid: inviteeProfile.Cmid }
            });
            const memberCount = clan ? await ClanMember.count({ where: { GroupId: clan.GroupId } }) : 0;

            if (
              clan != null &&
              // (await ClanMember.findOne({ where: { GroupId: clanId, Cmid: inviteeCmid } })) == null &&
              // (await GroupInvitation.findOne({ where: { GroupId: clanId, InviteeCmid: inviteeCmid } })) == null
              inviterMember &&
              inviterMember.Position !== GroupPosition.Member &&
              !inviteeMember &&
              !existingInvite &&
              (!clan.MembersLimit || memberCount <= clan.MembersLimit)
            ) {
              await GroupInvitation.insert({
                InviterCmid: publicProfile.Cmid,
                InviterName: publicProfile.Name,
                GroupName: clan.Name,
                GroupTag: clan.Tag,
                GroupId: clan.GroupId,
                GroupInvitationId: Math.randomInt(),
                InviteeCmid: inviteeCmid,
                InviteeName: inviteeProfile.Name,
                Message: message
              });

              Int32Proxy.Serialize(outputStream, ClanActionResultCode.Success);
            } else {
              Int32Proxy.Serialize(outputStream, ClanActionResultCode.Error);
            }
          } else {
            Int32Proxy.Serialize(outputStream, ClanActionResultCode.Error);
          }
        } else {
          Int32Proxy.Serialize(outputStream, ClanActionResultCode.Error);
        }
      } else {
        Int32Proxy.Serialize(outputStream, ClanActionResultCode.Error);
      }

      return isEncrypted
        ? this.CryptoPolicy.RijndaelEncrypt(outputStream, this.EncryptionPassPhrase, this.EncryptionInitVector)
        : outputStream;
    } catch (error) {
      this.handleEndpointError('InviteMemberToJoinAGroup', error);
    }

    return null;
  }

  static async KickMemberFromClan(data: number[], outputStream: number[]): Promise<number[] | null> {
    const isEncrypted = this.isEncrypted(data);
    const bytes = isEncrypted
      ? this.CryptoPolicy.RijndaelDecrypt(data, this.EncryptionPassPhrase, this.EncryptionInitVector)
      : data;

    try {
      const groupId = Int32Proxy.Deserialize(bytes);
      const authToken = StringProxy.Deserialize(bytes);
      const cmidToKick = Int32Proxy.Deserialize(bytes);

      this.debugEndpointData({ groupId, authToken, cmidToKick });

      const session = await ParadiseService.Instance.SessionManager.findSessionForSteamUser(authToken);
      if (session) {
        const steamMember = await session.SteamMember;

        if (!steamMember) {
          Int32Proxy.Serialize(outputStream, ClanActionResultCode.Error);
        } else {
          const publicProfile = await PublicProfile.findOne({ where: { Cmid: steamMember.Cmid } });
          const toKickProfile = await PublicProfile.findOne({ where: { Cmid: cmidToKick } });

          if (!publicProfile || !toKickProfile) {
            Int32Proxy.Serialize(outputStream, ClanActionResultCode.Error);
          } else {
            const clan = await Clan.findOne({
              where: {
                GroupId: groupId
              },
              relations: {
                Members: true
              }
            });

            if (!clan) {
              Int32Proxy.Serialize(outputStream, ClanActionResultCode.Error);
            } else {
              const clanMember = clan.Members.find((_) => _.Cmid === publicProfile.Cmid);
              const memberToKick = clan.Members.find((_) => _.Cmid === cmidToKick);

              if (
                !clanMember ||
                !memberToKick ||
                clanMember.Cmid === memberToKick.Cmid ||
                memberToKick.Position === GroupPosition.Leader ||
                clanMember.Position === GroupPosition.Member ||
                (memberToKick.Position === GroupPosition.Officer && clanMember.Position !== GroupPosition.Leader) ||
                (memberToKick.Position === GroupPosition.Member &&
                  !(clanMember.Position === GroupPosition.Officer || clanMember.Position === GroupPosition.Leader))
              ) {
                Int32Proxy.Serialize(outputStream, ClanActionResultCode.Error);
              } else {
                await ClanMember.delete({
                  GroupId: memberToKick.GroupId,
                  Cmid: memberToKick.Cmid
                });

                await PublicProfile.update(toKickProfile.Cmid, { GroupTag: '' });

                Int32Proxy.Serialize(outputStream, ClanActionResultCode.Success);
              }
            }
          }
        }
      }

      return isEncrypted
        ? this.CryptoPolicy.RijndaelEncrypt(outputStream, this.EncryptionPassPhrase, this.EncryptionInitVector)
        : outputStream;
    } catch (error) {
      this.handleEndpointError('KickMemberFromClan', error);
    }

    return null;
  }

  static async LeaveAClan(data: number[], outputStream: number[]): Promise<number[] | null> {
    const isEncrypted = this.isEncrypted(data);
    const bytes = isEncrypted
      ? this.CryptoPolicy.RijndaelDecrypt(data, this.EncryptionPassPhrase, this.EncryptionInitVector)
      : data;

    try {
      const groupId = Int32Proxy.Deserialize(bytes);
      const authToken = StringProxy.Deserialize(bytes);

      this.debugEndpointData({ groupId, authToken });

      const session = await ParadiseService.Instance.SessionManager.findSessionForSteamUser(authToken);
      if (session) {
        const steamMember = await session.SteamMember;

        if (!steamMember) {
          Int32Proxy.Serialize(outputStream, ClanActionResultCode.Error);
        } else {
          const publicProfile = await PublicProfile.findOne({ where: { Cmid: steamMember.Cmid } });

          if (!publicProfile) {
            Int32Proxy.Serialize(outputStream, ClanActionResultCode.Error);
          } else {
            const clan = await Clan.findOne({
              where: {
                GroupId: groupId
              },
              relations: {
                Members: true
              }
            });

            if (!clan) {
              Int32Proxy.Serialize(outputStream, ClanActionResultCode.Error);
            } else {
              const clanMember = clan.Members.find((_) => _.Cmid === publicProfile.Cmid);

              if (!clanMember) {
                Int32Proxy.Serialize(outputStream, ClanActionResultCode.Error);
              } else {
                const others = clan.Members.filter((_) => _.Cmid !== publicProfile.Cmid);

                if (clanMember.Position === GroupPosition.Leader && others.length > 0) {
                  const successor = others.find((_) => _.Position === GroupPosition.Officer) ?? others[0];

                  if (successor) {
                    await ClanMember.update(
                      { GroupId: successor.GroupId, Cmid: successor.Cmid },
                      { Position: GroupPosition.Leader }
                    );

                    await Clan.update(clan.GroupId, {
                      OwnerCmid: successor.Cmid,
                      OwnerName: successor.Name
                    });
                  }
                }

                await ClanMember.delete({
                  GroupId: clanMember.GroupId,
                  Cmid: clanMember.Cmid
                });

                await PublicProfile.update(publicProfile.Cmid, { GroupTag: '' });

                if (others.length === 0) {
                  await Clan.delete(groupId);
                }

                Int32Proxy.Serialize(outputStream, ClanActionResultCode.Success);
              }
            }
          }
        }
      }

      return isEncrypted
        ? this.CryptoPolicy.RijndaelEncrypt(outputStream, this.EncryptionPassPhrase, this.EncryptionInitVector)
        : outputStream;
    } catch (error) {
      this.handleEndpointError('LeaveAClan', error);
    }

    return null;
  }

  static async TransferOwnership(data: number[], outputStream: number[]): Promise<number[] | null> {
    const isEncrypted = this.isEncrypted(data);
    const bytes = isEncrypted
      ? this.CryptoPolicy.RijndaelDecrypt(data, this.EncryptionPassPhrase, this.EncryptionInitVector)
      : data;

    try {
      const groupId = Int32Proxy.Deserialize(bytes);
      const authToken = StringProxy.Deserialize(bytes);
      const newLeaderCmid = Int32Proxy.Deserialize(bytes);

      this.debugEndpointData({ groupId, authToken, newLeaderCmid });

      const session = await ParadiseService.Instance.SessionManager.findSessionForSteamUser(authToken);
      if (session) {
        const steamMember = await session.SteamMember;

        if (!steamMember) {
          Int32Proxy.Serialize(outputStream, ClanActionResultCode.Error);
        } else {
          const publicProfile = await PublicProfile.findOne({ where: { Cmid: steamMember.Cmid } });
          const newLeaderProfile = await PublicProfile.findOne({ where: { Cmid: newLeaderCmid } });

          if (!publicProfile || !newLeaderProfile) {
            Int32Proxy.Serialize(outputStream, ClanActionResultCode.Error);
          } else {
            const clan = await Clan.findOne({
              where: {
                GroupId: groupId
              },
              relations: {
                Members: true
              }
            });

            if (!clan) {
              Int32Proxy.Serialize(outputStream, ClanActionResultCode.Error);
            } else {
              const clanMember = clan!.Members.find((_) => _.Cmid === publicProfile.Cmid);
              const newLeader = clan!.Members.find((_) => _.Cmid === newLeaderCmid);

              if (!clanMember || !newLeader || clanMember.Position !== GroupPosition.Leader) {
                Int32Proxy.Serialize(outputStream, ClanActionResultCode.Error);
              } else {
                const friendsList = await ContactRequest.find({
                  where: [
                    { InitiatorCmid: newLeaderProfile.Cmid, Status: ContactRequestStatus.Accepted },
                    { ReceiverCmid: newLeaderProfile.Cmid, Status: ContactRequestStatus.Accepted }
                  ]
                });
                const playerStatistics = await PlayerStatistics.findOne({ where: { Cmid: newLeaderProfile.Cmid } });
                const hasClanLicense =
                  (await PlayerInventoryItem.findOne({
                    where: { Cmid: newLeaderProfile.Cmid, ItemId: UberstrikeInventoryItem.ClanLicense }
                  })) != null;

                if (!playerStatistics || XpPointsUtil.GetLevelForXp(playerStatistics.Xp) < 4) {
                  Int32Proxy.Serialize(outputStream, ClanCreationResultCode.RequirementPlayerLevel);
                } else if (friendsList.length < 1) {
                  Int32Proxy.Serialize(outputStream, ClanCreationResultCode.RequirementPlayerFriends);
                } else if (!hasClanLicense) {
                  Int32Proxy.Serialize(outputStream, ClanCreationResultCode.RequirementClanLicense);
                } else {
                  const prevPosition = newLeader.Position;

                  await Clan.update(clan.GroupId, {
                    OwnerCmid: newLeaderProfile.Cmid
                  });

                  await ClanMember.update(
                    {
                      GroupId: clanMember.GroupId,
                      Cmid: clanMember.Cmid
                    },
                    { Position: prevPosition }
                  );

                  await ClanMember.update(
                    {
                      GroupId: newLeader.GroupId,
                      Cmid: newLeader.Cmid
                    },
                    { Position: GroupPosition.Leader }
                  );

                  Int32Proxy.Serialize(outputStream, ClanActionResultCode.Success);
                }
              }
            }
          }
        }
      }

      return isEncrypted
        ? this.CryptoPolicy.RijndaelEncrypt(outputStream, this.EncryptionPassPhrase, this.EncryptionInitVector)
        : outputStream;
    } catch (error) {
      this.handleEndpointError('TransferOwnership', error);
    }

    return null;
  }

  static async UpdateMemberPosition(data: number[], outputStream: number[]): Promise<number[] | null> {
    const isEncrypted = this.isEncrypted(data);
    const bytes = isEncrypted
      ? this.CryptoPolicy.RijndaelDecrypt(data, this.EncryptionPassPhrase, this.EncryptionInitVector)
      : data;

    try {
      const updateMemberPositionData = MemberPositionUpdateViewProxy.Deserialize(bytes);

      this.debugEndpointData({ updateMemberPositionData });

      const session = await ParadiseService.Instance.SessionManager.findSessionForSteamUser(
        updateMemberPositionData.AuthToken
      );
      if (session) {
        const steamMember = await session.SteamMember;

        if (!steamMember) {
          Int32Proxy.Serialize(outputStream, ClanActionResultCode.Error);
        } else {
          const publicProfile = await PublicProfile.findOne({ where: { Cmid: steamMember.Cmid } });
          const targetProfile = await PublicProfile.findOne({ where: { Cmid: updateMemberPositionData.MemberCmid } });

          if (!publicProfile || !targetProfile) {
            Int32Proxy.Serialize(outputStream, ClanActionResultCode.Error);
          } else {
            const clan = await Clan.findOne({
              where: {
                GroupId: updateMemberPositionData.GroupId
              },
              relations: {
                Members: true
              }
            });

            if (!clan) {
              Int32Proxy.Serialize(outputStream, ClanActionResultCode.Error);
            } else {
              const clanMember = clan.Members.find((_) => _.Cmid === publicProfile.Cmid);
              const targetClanMember = clan.Members.find((_) => _.Cmid === targetProfile.Cmid);

              if (
                !clanMember ||
                !targetClanMember ||
                clanMember.Cmid === targetClanMember.Cmid ||
                targetClanMember.Position === GroupPosition.Leader ||
                clanMember.Position !== GroupPosition.Leader ||
                updateMemberPositionData.Position === GroupPosition.Leader
              ) {
                Int32Proxy.Serialize(outputStream, ClanActionResultCode.Error);
              } else {
                await ClanMember.update(
                  {
                    GroupId: targetClanMember.GroupId,
                    Cmid: targetClanMember.Cmid
                  },
                  {
                    Position: updateMemberPositionData.Position
                  }
                );

                Int32Proxy.Serialize(outputStream, ClanActionResultCode.Success);
              }
            }
          }
        }
      }

      return isEncrypted
        ? this.CryptoPolicy.RijndaelEncrypt(outputStream, this.EncryptionPassPhrase, this.EncryptionInitVector)
        : outputStream;
    } catch (error) {
      this.handleEndpointError('UpdateMemberPosition', error);
    }

    return null;
  }
}
