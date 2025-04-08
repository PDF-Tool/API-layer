import { MessageType } from '@/enums/websockets'

export type ConnectionStatus = 'disconnected' | 'connecting' | 'connected'

export interface BaseMessage {
  Type: MessageType
}

// Updated chat message interface
export interface ChatMessage extends BaseMessage {
  Type: MessageType.ChatMessage
  Name: string
  Content: string
}

export interface UserListMessage extends BaseMessage {
  Type: MessageType.UserList
  Users: string[]
}

export interface UserConnectedMessage extends BaseMessage {
  Type: MessageType.UserConnected
  Name: string
  Transport: string
}

export interface UserDisconnectedMessage extends BaseMessage {
  Type: MessageType.UserDisconnected
  Name: string
}

export interface HistoryMessage extends BaseMessage {
  Type: MessageType.History
  Messages: BaseMessage[]
}

export interface PingMessage extends BaseMessage {
  Type: MessageType.PingMessage
  Timestamp: number
}

export interface PongMessage extends BaseMessage {
  Type: MessageType.PongMessage
  Timestamp: number
  OriginalTimestamp: number
}

export interface ErrorMessage extends BaseMessage {
  Type: MessageType.Error
  ErrorMessage: string
}

// Union type for all possible message types
export type Message =
  | ChatMessage
  | UserListMessage
  | UserConnectedMessage
  | UserDisconnectedMessage
  | HistoryMessage
  | PingMessage
  | PongMessage
  | ErrorMessage
