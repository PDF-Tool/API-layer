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
} from '@/types/websockets'
import { MessageType } from '@/enums/websockets'

export const useWebSocketStore = defineStore('websocket', () => {
  const socket = ref<WebSocket | null>(null)
  const status = ref<ConnectionStatus>('disconnected')
  const messages = ref<ChatMessage[]>([])
  const users = ref<string[]>([])
  const username = ref('')

  const isConnected = computed(() => status.value === 'connected')

  function connect(name: string) {
    if (socket.value?.readyState === WebSocket.OPEN) {
      return // Already connected
    }

    status.value = 'connecting'
    username.value = name

    const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:'
    const host = import.meta.env.VITE_API_URL || `${protocol}//${window.location.hostname}:5091`

    socket.value = new WebSocket(`${host}/ws?name=${encodeURIComponent(name)}`)

    socket.value.onopen = () => {
      status.value = 'connected'
    }

    socket.value.onclose = () => {
      status.value = 'disconnected'
    }

    socket.value.onmessage = (event) => {
      const data = JSON.parse(event.data)

      switch (data.Type) {
        case MessageType.ChatMessage:
          messages.value.push(data as ChatMessage)
          break

        case MessageType.UserList:
          users.value = (data as UserListMessage).Users
          break

        case MessageType.UserConnected:
          // A user connected
          console.log(`User connected: ${(data as UserConnectedMessage).Name}`)
          break

        case MessageType.UserDisconnected:
          // A user disconnected
          console.log(`User disconnected: ${(data as UserDisconnectedMessage).Name}`)
          break

        case MessageType.History:
          // Process history messages
          const historyData = data as HistoryMessage
          if (historyData.Messages) {
            const chatMessages = historyData.Messages.filter(
              (m) => m.Type === MessageType.ChatMessage,
            ).map((m) => m as ChatMessage)
            messages.value = chatMessages
          }
          break

        case MessageType.PingMessage:
          // Respond to server ping with pong
          sendPong((data as PingMessage).Timestamp)
          break

        case MessageType.Error:
          console.error(`Server error: ${(data as ErrorMessage).ErrorMessage}`)
          break

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
  }
})
