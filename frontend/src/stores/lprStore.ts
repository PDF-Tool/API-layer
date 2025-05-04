import { ref } from 'vue'
import { defineStore } from 'pinia'
import axios from 'axios'

// Configure base URL for your API calls
const apiBaseUrl = import.meta.env.VITE_BACKEND_SERVER_URL + '/api';


export const useLprStore = defineStore('lpr', () => {
  // --- State ---
  const lprHost = ref<string>('localhost')
  const lprQueue = ref<string>('myqueue')
  const lprPort = ref<number>(515)

  const isConnected = ref<boolean | null>(null)
  const connectionStatusMessage = ref<string>('Disconnected') // More accurate initial state
  const isCheckingConnection = ref<boolean>(false)

  // --- Actions ---
  async function checkLprConnection() {
    if (isCheckingConnection.value) return // Prevent simultaneous checks
    if (!lprHost.value) {
        isConnected.value = false;
        connectionStatusMessage.value = 'LPR Host is not set.';
        return;
    }

    isCheckingConnection.value = true
    isConnected.value = null
    connectionStatusMessage.value = 'Connecting' // Static message while checking

    try {
        const response = await axios.get(`${apiBaseUrl}/Lpr/check`, {
            params: {
                host: lprHost.value,
                port: lprPort.value
            },
            timeout: 5000
        });

        if (response.data && typeof response.data.connected === 'boolean') {
            isConnected.value = response.data.connected;

            if (response.data.connected) {
                connectionStatusMessage.value = `Connected to ${lprHost.value}:${lprPort.value}`;
                // Set checking to false immediately on success
                isCheckingConnection.value = false;
                // Optionally, after a short delay, revert to 'Connected'
                setTimeout(() => {
                    if (connectionStatusMessage.value === `Connected to ${lprHost.value}:${lprPort.value}`) {
                        connectionStatusMessage.value = 'Connected';
                    }
                }, 2000);
            } else {
                connectionStatusMessage.value = response.data.message || 'Connection Failed';
                isCheckingConnection.value = false;
            }
        } else {
            isConnected.value = false;
            connectionStatusMessage.value = 'Invalid response from connection check API.';
            isCheckingConnection.value = false;
        }

    } catch (error) {
        const err = error as any;
        console.error('Error checking LPR connection:', err);
        isConnected.value = false;
        if (err.response && err.response.data && err.response.data.message) {
            connectionStatusMessage.value = `Error: ${err.response.data.message}`;
        } else if (axios.isCancel(err) || err.code === 'ECONNABORTED') {
            connectionStatusMessage.value = 'Connection check timed out.';
        } else {
            connectionStatusMessage.value = 'Error checking connection (network or server issue).';
        }
        isCheckingConnection.value = false;
    }
  }

  // --- Load/Save functions remain the same ---
  function loadSettings() { /* ... */ }
  function saveSettings() { /* ... */ }

  return {
    lprHost,
    lprQueue,
    lprPort,
    isConnected,
    connectionStatusMessage,
    isCheckingConnection,
    checkLprConnection,
    saveSettings
  }
})