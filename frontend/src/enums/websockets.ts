export enum MessageType {
  ChatMessage = 'ChatMessage',
  UserList = 'UserList',
  UserConnected = 'UserConnected',
  UserDisconnected = 'UserDisconnected',
  History = 'History',
  Error = 'Error',
  PingMessage = 'PingMessage',
  PongMessage = 'PongMessage',
}

export enum SocketStatus {
  Connected = 'connected',
  Disconnected = 'disconnected',
  Connecting = 'connecting',
  Disconnecting = 'disconnecting',
  Error = 'error',
}
