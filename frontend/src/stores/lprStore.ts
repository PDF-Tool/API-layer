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
        const response = await axios.get(`${apiBaseUrl}/Lpr/check`, { // Ensure path is correct
            params: {
                host: lprHost.value,
                port: lprPort.value
            },
            timeout: 5000 // Add timeout
        });

        if (response.data && typeof response.data.connected === 'boolean') {
            isConnected.value = response.data.connected; // Set connection status first

            if (response.data.connected) {
                // Successfully connected
                // Set the detailed message
                connectionStatusMessage.value = `Connected to ${lprHost.value}:${lprPort.value}`;

                // After 2 seconds, revert to "Connected" AND set checking to false
                setTimeout(() => {
                    // Only change message if it hasn't been updated by another check/error since
                    if (connectionStatusMessage.value === `Connected to ${lprHost.value}:${lprPort.value}`) {
                         connectionStatusMessage.value = 'Connected';
                    }
                    // --- Set checking to false HERE, after the delay ---
                    isCheckingConnection.value = false;
                }, 2000);

            } else {
                // Connection failed according to API
                connectionStatusMessage.value = response.data.message || 'Connection Failed';
                // --- Set checking to false immediately on failure ---
                isCheckingConnection.value = false;
            }
        } else {
            // Invalid response structure
            isConnected.value = false;
            connectionStatusMessage.value = 'Invalid response from connection check API.';
             // --- Set checking to false immediately on error ---
            isCheckingConnection.value = false;
        }

    } catch (error: any) {
        console.error('Error checking LPR connection:', error);
        isConnected.value = false; // Assume not connected on error
         if (error.response && error.response.data && error.response.data.message) {
             connectionStatusMessage.value = `Error: ${error.response.data.message}`;
         } else if (axios.isCancel(error) || error.code === 'ECONNABORTED') {
             connectionStatusMessage.value = 'Connection check timed out.';
         } else {
             connectionStatusMessage.value = 'Error checking connection (network or server issue).';
         }
         // --- Set checking to false immediately on error ---
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