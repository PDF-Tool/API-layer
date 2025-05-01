import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import type {
  ChatMessage,
  ConnectionStatus,
  UserListMessage,
  UserConnectedMessage,
  PingMessage,
  HistoryMessage,
  UserDisconnectedMessage,
  ErrorMessage,
  PongMessage,
  ProcessStartedMessage,
  ProcessProgressMessage,
  ProcessCompletedMessage,
  ProcessFailedMessage,
  Message,
} from '@/types/websockets'
import { MessageType, SocketStatus } from '@/enums/websockets'

export const useWebSocketStore = defineStore('websocket', () => {
  const socket = ref<WebSocket | null>(null)
  const status = ref<ConnectionStatus>(SocketStatus.Disconnected)
  const messages = ref<ChatMessage[]>([])
  const users = ref<string[]>([])
  const username = ref('')

  const processes = ref<Record<string, {
    started?: ProcessStartedMessage,
    progress?: ProcessProgressMessage,
    completed?: ProcessCompletedMessage,
    failed?: ProcessFailedMessage,
  }>>({})

  const isConnected = computed(() => status.value === SocketStatus.Connected)

  function connect(name: string) {
    if (socket.value?.readyState === WebSocket.OPEN) {
      return
    }

    status.value = 'connecting'
    username.value = name

    const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:'
    const host = import.meta.env.VITE_API_URL || `${protocol}//${window.location.hostname}:5091`

    socket.value = new WebSocket(`${host}/ws?name=${encodeURIComponent(name)}`)

    socket.value.onopen = () => (status.value = SocketStatus.Connected)

    socket.value.onclose = () => (status.value = SocketStatus.Disconnected)

    socket.value.onmessage = (event) => {
      const data: Message = JSON.parse(event.data)

      switch (data.Type) {
        case MessageType.ChatMessage:
          messages.value.push(data as ChatMessage)
          break

        case MessageType.UserList:
          users.value = (data as UserListMessage).Users
          break

        case MessageType.UserConnected:
          const connectedUser = (data as UserConnectedMessage).Name
          if (!users.value.includes(connectedUser)) {
            users.value = [...users.value, connectedUser]
          }
          break

        case MessageType.UserDisconnected:
          const disconnectedUser = (data as UserDisconnectedMessage).Name
          users.value = users.value.filter((user) => user !== disconnectedUser)
          break

        case MessageType.History:
          const historyData = data as HistoryMessage
          if (historyData.Messages) {
            const chatMessages = historyData.Messages.filter(
              (m) => m.Type === MessageType.ChatMessage,
            ).map((m) => m as ChatMessage)
            messages.value = chatMessages
          }
          break

        case MessageType.PingMessage:
          sendPong((data as PingMessage).Timestamp)
          break

        case MessageType.Error:
          console.error(`Server error: ${(data as ErrorMessage).ErrorMessage}`)
          break

        case MessageType.ProcessStarted: {
          const msg = data as ProcessStartedMessage
          processes.value[msg.ProcessId] = { started: msg }
          break
        }
        case MessageType.ProcessProgress: {
          const msg = data as ProcessProgressMessage
          if (!processes.value[msg.ProcessId]) processes.value[msg.ProcessId] = {}
          processes.value[msg.ProcessId].progress = msg
          break
        }
        case MessageType.ProcessCompleted: {
          const msg = data as ProcessCompletedMessage
          if (!processes.value[msg.ProcessId]) processes.value[msg.ProcessId] = {}
          processes.value[msg.ProcessId].completed = msg
          break
        }
        case MessageType.ProcessFailed: {
          const msg = data as ProcessFailedMessage
          if (!processes.value[msg.ProcessId]) processes.value[msg.ProcessId] = {}
          processes.value[msg.ProcessId].failed = msg
          break
        }

        default:
          console.warn('Unknown message type:', data)
      }
    }

    socket.value.onerror = (error) => {
      console.error('WebSocket error:', error)
      status.value = 'disconnected'
    }
  }

  function disconnect() {
    socket.value?.close()
    socket.value = null
    status.value = 'disconnected'
  }

  function sendMessage(content: string) {
    if (socket.value?.readyState === WebSocket.OPEN) {
      const message = {
        Name: username.value,
        Content: content,
        Type: 'ChatMessage',
      }
      socket.value.send(JSON.stringify(message))
    }
  }

  function sendPong(originalTimestamp: number) {
    if (socket.value?.readyState === WebSocket.OPEN) {
      const pong: PongMessage = {
        Type: MessageType.PongMessage,
        Timestamp: Date.now(),
        OriginalTimestamp: originalTimestamp,
      }
      socket.value.send(JSON.stringify(pong))
    }
  }

  return {
    connect,
    disconnect,
    sendMessage,
    status,
    isConnected,
    messages,
    users,
    username,
    processes,
  }
})
