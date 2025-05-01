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

// Process-related messages
export interface ProcessStartedMessage extends BaseMessage {
  Type: MessageType.ProcessStarted
  ProcessId: string
  ProcessName: string
  Initiator: string
  StartTime: string
  Status: string
  AdditionalData?: Record<string, any>
}

export interface ProcessProgressMessage extends BaseMessage {
  Type: MessageType.ProcessProgress
  ProcessId: string
  PercentComplete: number
  Timestamp: string
  CurrentStage?: string
  AdditionalData?: Record<string, any>
}

export interface ProcessCompletedMessage extends BaseMessage {
  Type: MessageType.ProcessCompleted
  ProcessId: string
  CompletionTime: string
  Duration: string
  ResultUrl?: string
  AdditionalData?: Record<string, any>
}

export interface ProcessFailedMessage extends BaseMessage {
  Type: MessageType.ProcessFailed
  ProcessId: string
  ErrorMessage: string
  FailureTime: string
  AdditionalData?: Record<string, any>
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
  | ProcessStartedMessage
  | ProcessProgressMessage
  | ProcessCompletedMessage
  | ProcessFailedMessage
