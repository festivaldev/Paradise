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
import { ApiVersion } from '@/utils/enums';
import { PrivateMessage, PublicProfile } from '@festivaldev/paradise-models';
import { MessageThreadView, PrivateMessageView } from '@festivaldev/uberstrike-js/Cmune/DataCenter/Common/Entities';
import {
  BooleanProxy,
  Int32Proxy,
  ListProxy,
  MessageThreadViewProxy,
  PrivateMessageViewProxy,
  StringProxy
} from '@festivaldev/uberstrike-js/UberStrike/Core/Serialization';
import BaseWebService from '../BaseWebService';

export default class PrivateMessageWebService extends BaseWebService {
  static override get ServiceName(): string {
    return 'PrivateMessageWebService';
  }

  static override get ServiceVersion(): string {
    return ApiVersion.Current;
  }

  static async DeleteThread(data: number[], outputStream: number[]): Promise<number[] | null> {
    const isEncrypted = this.isEncrypted(data);
    const bytes = isEncrypted
      ? this.CryptoPolicy.RijndaelDecrypt(data, this.EncryptionPassPhrase, this.EncryptionInitVector)
      : data;

    try {
      const authToken = StringProxy.Deserialize(bytes);
      const otherCmid = Int32Proxy.Deserialize(bytes);

      this.debugEndpointData({ authToken, otherCmid });

      const session = await ParadiseService.Instance.SessionManager.findSessionForSteamUser(authToken);
      if (session) {
        const steamMember = await session.SteamMember;

        if (steamMember) {
          const messages = await PrivateMessage.find({
            where: [
              { FromCmid: steamMember.Cmid, ToCmid: otherCmid },
              { FromCmid: otherCmid, ToCmid: steamMember.Cmid }
            ]
          });

          for (const message of messages) {
            if (message.FromCmid === steamMember.Cmid) {
              await PrivateMessage.update(message.PrivateMessageId, { IsDeletedBySender: true });
            } else {
              await PrivateMessage.update(message.PrivateMessageId, { IsDeletedByReceiver: true });
            }
          }

          BooleanProxy.Serialize(outputStream, true);
        }
      }

      return isEncrypted
        ? this.CryptoPolicy.RijndaelEncrypt(outputStream, this.EncryptionPassPhrase, this.EncryptionInitVector)
        : outputStream;
    } catch (error) {
      this.handleEndpointError('DeleteThread', error);
    }

    return null;
  }

  static async GetAllMessageThreadsForUser(data: number[], outputStream: number[]): Promise<number[] | null> {
    const isEncrypted = this.isEncrypted(data);
    const bytes = isEncrypted
      ? this.CryptoPolicy.RijndaelDecrypt(data, this.EncryptionPassPhrase, this.EncryptionInitVector)
      : data;

    try {
      const authToken = StringProxy.Deserialize(bytes);
      const pageNumber = Int32Proxy.Deserialize(bytes);

      this.debugEndpointData({ authToken, pageNumber });

      const session = await ParadiseService.Instance.SessionManager.findSessionForSteamUser(authToken);
      if (session) {
        const steamMember = await session.SteamMember;

        if (steamMember) {
          const messages = (
            await PrivateMessage.find({
              where: [{ FromCmid: steamMember.Cmid }, { ToCmid: steamMember.Cmid }],
              order: { DateSent: 'ASC' }
            })
          ).reduce((acc: any, curr: PrivateMessage) => {
            const threadId = [curr.FromCmid, curr.ToCmid].sort().join(',');

            if (!acc[threadId]) {
              acc[threadId] = [];
            }

            acc[threadId].push(curr);
            return acc;
          }, {});

          const threads: MessageThreadView[] = [];

          for (const messageGroup of Object.values(messages)) {
            const filteredMessages = (messageGroup as any).filter(
              (_: PrivateMessage) =>
                (_.FromCmid === steamMember.Cmid && !_.IsDeletedBySender) ||
                (_.ToCmid === steamMember.Cmid && !_.IsDeletedByReceiver)
            );

            if (filteredMessages.length) {
              const message = filteredMessages[filteredMessages.length - 1];

              const otherCmid = message.FromCmid !== steamMember.Cmid ? message.FromCmid : message.ToCmid;
              const otherProfile = await PublicProfile.findOne({ where: { Cmid: otherCmid } });

              threads.push(
                new MessageThreadView({
                  ThreadId: otherCmid,
                  ThreadName: otherProfile?.Name?.trim() || (otherCmid === 0 ? 'System' : 'Unknown player'),
                  MessageCount: filteredMessages.length,
                  LastMessagePreview: message.ContentText,
                  LastUpdate: message.DateSent,
                  HasNewMessages: filteredMessages.some(
                    (_: PrivateMessage) => _.ToCmid === steamMember.Cmid && !_.IsRead
                  )
                })
              );
            }
          }

          ListProxy.Serialize<MessageThreadView>(outputStream, threads, MessageThreadViewProxy.Serialize);
        }
      }

      return isEncrypted
        ? this.CryptoPolicy.RijndaelEncrypt(outputStream, this.EncryptionPassPhrase, this.EncryptionInitVector)
        : outputStream;
    } catch (error) {
      this.handleEndpointError('GetAllMessageThreadsForUser', error);
    }

    return null;
  }

  static async GetMessageWithIdForCmid(data: number[], outputStream: number[]): Promise<number[] | null> {
    const isEncrypted = this.isEncrypted(data);
    const bytes = isEncrypted
      ? this.CryptoPolicy.RijndaelDecrypt(data, this.EncryptionPassPhrase, this.EncryptionInitVector)
      : data;

    try {
      const authToken = StringProxy.Deserialize(bytes);
      const messageId = Int32Proxy.Deserialize(bytes);

      this.debugEndpointData({ authToken, messageId });

      const session = await ParadiseService.Instance.SessionManager.findSessionForSteamUser(authToken);
      if (session) {
        const steamMember = await session.SteamMember;

        if (steamMember) {
          const message = await PrivateMessage.findOne({
            where: [
              {
                PrivateMessageId: messageId,
                FromCmid: steamMember.Cmid,
                IsDeletedBySender: false
              },
              {
                PrivateMessageId: messageId,
                ToCmid: steamMember.Cmid,
                IsDeletedByReceiver: false
              }
            ],
            order: { DateSent: 'ASC' }
          });

          if (message) {
            PrivateMessageViewProxy.Serialize(outputStream, new PrivateMessageView({ ...message }));
          }
        }
      }

      return isEncrypted
        ? this.CryptoPolicy.RijndaelEncrypt(outputStream, this.EncryptionPassPhrase, this.EncryptionInitVector)
        : outputStream;
    } catch (error) {
      this.handleEndpointError('GetMessageWithIdForCmid', error);
    }

    return null;
  }

  static async GetThreadMessages(data: number[], outputStream: number[]): Promise<number[] | null> {
    const isEncrypted = this.isEncrypted(data);
    const bytes = isEncrypted
      ? this.CryptoPolicy.RijndaelDecrypt(data, this.EncryptionPassPhrase, this.EncryptionInitVector)
      : data;

    try {
      const authToken = StringProxy.Deserialize(bytes);
      const otherCmid = Int32Proxy.Deserialize(bytes);
      const pageNumber = Int32Proxy.Deserialize(bytes);

      this.debugEndpointData({ authToken, otherCmid, pageNumber });

      const session = await ParadiseService.Instance.SessionManager.findSessionForSteamUser(authToken);
      if (session) {
        const steamMember = await session.SteamMember;

        if (steamMember) {
          const messages = await PrivateMessage.find({
            where: [
              {
                FromCmid: steamMember.Cmid,
                ToCmid: otherCmid,
                IsDeletedBySender: false
              },
              {
                FromCmid: otherCmid,
                ToCmid: steamMember.Cmid,
                IsDeletedByReceiver: false
              }
            ],
            order: { DateSent: 'ASC' }
          });

          ListProxy.Serialize<PrivateMessageView>(
            outputStream,
            messages as PrivateMessageView[],
            PrivateMessageViewProxy.Serialize
          );
        }
      }

      return isEncrypted
        ? this.CryptoPolicy.RijndaelEncrypt(outputStream, this.EncryptionPassPhrase, this.EncryptionInitVector)
        : outputStream;
    } catch (error) {
      this.handleEndpointError('GetThreadMessages', error);
    }

    return null;
  }

  static async MarkThreadAsRead(data: number[], outputStream: number[]): Promise<number[] | null> {
    const isEncrypted = this.isEncrypted(data);
    const bytes = isEncrypted
      ? this.CryptoPolicy.RijndaelDecrypt(data, this.EncryptionPassPhrase, this.EncryptionInitVector)
      : data;

    try {
      const authToken = StringProxy.Deserialize(bytes);
      const otherCmid = Int32Proxy.Deserialize(bytes);

      this.debugEndpointData({ authToken, otherCmid });

      const session = await ParadiseService.Instance.SessionManager.findSessionForSteamUser(authToken);
      if (session) {
        const steamMember = await session.SteamMember;

        if (steamMember) {
          const messages = await PrivateMessage.find({
            where: {
              ToCmid: steamMember.Cmid,
              FromCmid: otherCmid,
              IsRead: false,
              IsDeletedByReceiver: false
            }
          });

          await Promise.all(
            messages.map((message) => PrivateMessage.update(message.PrivateMessageId, { IsRead: true }))
          );

          BooleanProxy.Serialize(outputStream, true);
        }
      }

      return isEncrypted
        ? this.CryptoPolicy.RijndaelEncrypt(outputStream, this.EncryptionPassPhrase, this.EncryptionInitVector)
        : outputStream;
    } catch (error) {
      this.handleEndpointError('MarkThreadAsRead', error);
    }

    return null;
  }

  static async SendMessage(data: number[], outputStream: number[]): Promise<number[] | null> {
    const isEncrypted = this.isEncrypted(data);
    const bytes = isEncrypted
      ? this.CryptoPolicy.RijndaelDecrypt(data, this.EncryptionPassPhrase, this.EncryptionInitVector)
      : data;

    try {
      const authToken = StringProxy.Deserialize(bytes);
      const receiverCmid = Int32Proxy.Deserialize(bytes);
      const content = StringProxy.Deserialize(bytes);

      this.debugEndpointData({ authToken, receiverCmid, content });

      const session = await ParadiseService.Instance.SessionManager.findSessionForSteamUser(authToken);
      if (session) {
        const steamMember = await session.SteamMember;

        if (steamMember) {
          const sender = await PublicProfile.findOne({ where: { Cmid: steamMember.Cmid } });
          const receiver = await PublicProfile.findOne({ where: { Cmid: receiverCmid } });

          if (sender && receiver && content.trim().length) {
            const privateMessage = await PrivateMessage.save(
              PrivateMessage.create({
                PrivateMessageId: Math.randomInt(),
                FromCmid: sender.Cmid,
                FromName: sender.Name,
                ToCmid: receiver.Cmid,
                DateSent: new Date(),
                ContentText: content,
                IsRead: false
              })
            );

            PrivateMessageViewProxy.Serialize(outputStream, new PrivateMessageView({ ...privateMessage }));
          }
        }
      }

      return isEncrypted
        ? this.CryptoPolicy.RijndaelEncrypt(outputStream, this.EncryptionPassPhrase, this.EncryptionInitVector)
        : outputStream;
    } catch (error) {
      this.handleEndpointError('SendMessage', error);
    }

    return null;
  }
}
