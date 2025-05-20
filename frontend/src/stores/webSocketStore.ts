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
import { toast } from 'vue-sonner'

export const useWebSocketStore = defineStore('websocket', () => {
  const socket = ref<WebSocket | null>(null)
  const status = ref<ConnectionStatus>(SocketStatus.Disconnected)
  const messages = ref<ChatMessage[]>([])
  const users = ref<string[]>([])
  const username = ref('')

  const processes = ref<
    Record<
      string,
      {
        started?: ProcessStartedMessage
        progress?: ProcessProgressMessage
        completed?: ProcessCompletedMessage
        failed?: ProcessFailedMessage
      }
    >
  >({})

  const isConnected = computed(() => status.value === SocketStatus.Connected)

  function connect(name: string) {
    if (socket.value?.readyState === WebSocket.OPEN) {
      console.log('WebSocket already connected.')
      return
    }
    if (status.value === SocketStatus.Connecting) {
      console.log('WebSocket connection already in progress.')
      return
    }

    status.value = SocketStatus.Connecting // Use enum value
    username.value = name

    // Use the VITE_BACKEND_SERVER_URL directly as the base host
    const host = import.meta.env.VITE_BACKEND_SERVER_URL

    if (!host) {
      console.error('VITE_BACKEND_SERVER_URL is not defined in environment variables.')
      status.value = SocketStatus.Disconnected // Use enum value
      return
    }

    const wsProtocol = host.startsWith('https://') ? 'wss:' : 'ws:'
    const wsHost = host.replace(/^https?:/, wsProtocol)
    const wsUrl = `${wsHost}/ws?name=${encodeURIComponent(name)}`

    console.log(`Attempting WebSocket connection to: ${wsUrl}`)

    try {
      socket.value = new WebSocket(wsUrl)
    } catch (e) {
      console.error('Failed to create WebSocket:', e)
      status.value = SocketStatus.Disconnected
      return
    }

    socket.value.onopen = () => {
      console.log('WebSocket connection opened.')
      status.value = SocketStatus.Connected // Use enum value
    }

    socket.value.onclose = (event) => {
      console.log('WebSocket connection closed:', event.code, event.reason)
      status.value = SocketStatus.Disconnected // Use enum value
      socket.value = null // Clear the socket ref
    }

    socket.value.onmessage = (event) => {
      try {
        const data: Message = JSON.parse(event.data)
        // console.log('WebSocket message received:', data); // Optional: log received messages

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
              users.value = [...users.value, connectedUser].sort() // Keep sorted
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
            toast.error('Error', {
              description: (data as ErrorMessage).ErrorMessage,
            })
            console.error(`Server error message: ${(data as ErrorMessage).ErrorMessage}`)
            break

          case MessageType.ProcessStarted: {
            const msg = data as ProcessStartedMessage
            toast.info(`Process started by: ${data.Initiator}`, {
              description: `Process: ${msg.ProcessName}`,
            })
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
            toast.error('Process failed', {
              description: data.ErrorMessage,
            })
            if (!processes.value[msg.ProcessId]) processes.value[msg.ProcessId] = {}
            processes.value[msg.ProcessId].failed = msg
            break
          }

          default:
            console.warn('Unknown WebSocket message type received:', data.Type, data)
        }
      } catch (e) {
        console.error('Failed to parse WebSocket message:', e, 'Raw data:', event.data)
      }
    }

    socket.value.onerror = (event) => {
      // The 'error' event is usually followed by 'close'.
      // Log the error, state will be updated in onclose.
      console.error('WebSocket error event:', event)
      // Setting status here might be redundant if onclose handles it,
      // but can be useful if onclose doesn't fire for some errors.
      // status.value = SocketStatus.Disconnected;
    }
  }

  function disconnect() {
    if (socket.value) {
      console.log('Disconnecting WebSocket...')
      socket.value.close() // This will trigger the onclose handler
    }
    // Clear local state immediately
    status.value = SocketStatus.Disconnected
    username.value = ''
    users.value = []
    messages.value = []
    processes.value = {}
    socket.value = null
  }

  function sendMessage(content: string) {
    if (socket.value?.readyState === WebSocket.OPEN) {
      const message: ChatMessage = {
        Type: MessageType.ChatMessage, // Use enum value
        Name: username.value,
        Content: content,
      }
      try {
        socket.value.send(JSON.stringify(message))
      } catch (e) {
        console.error('Failed to send WebSocket message:', e)
      }
    } else {
      console.warn(
        'Cannot send message, WebSocket is not open. State:',
        status.value,
        socket.value?.readyState,
      )
    }
  }

  function sendPong(originalTimestamp: number) {
    if (socket.value?.readyState === WebSocket.OPEN) {
      const pong: PongMessage = {
        Type: MessageType.PongMessage, // Use enum value
        Timestamp: Date.now(),
        OriginalTimestamp: originalTimestamp,
      }
      try {
        socket.value.send(JSON.stringify(pong))
      } catch (e) {
        console.error('Failed to send Pong message:', e)
      }
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
