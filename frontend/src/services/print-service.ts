import Api from '@/plugins/axios'

const api = new Api()

/**
 * Tests connection to an LPD printer server
 * @param host The hostname or IP address of the LPD server
 * @param port The port number (default 515)
 * @returns Connection test result
 */
export async function testLpdConnection(host: string, port: number = 515) {
  try {
    const response = await api.api.post('/api/Print/TestConnection', {
      host,
      port
    })
    
    return response.data
  } catch (error: any) {
    console.error('LPD connection test error:', error)
    return {
      success: false,
      message: error.message === 'Network Error' 
        ? 'Cannot connect to the API server. Please make sure the backend is running.' 
        : 'Connection test failed: ' + (error.message || 'Unknown error')
    }
  }
}

/**
 * Sends a PDF file to an LPD printer
 * @param file The PDF file to print
 * @param printerName The name of the printer queue
 * @param serverHost The LPD server hostname or IP
 * @param serverPort The LPD server port
 * @returns Print job response
 */
export async function sendPrintJob(
  file: File, 
  printerName: string = 'default',
  serverHost: string = 'localhost',
  serverPort: number = 515
) {
  try {
    const formData = new FormData()
    formData.append('file', file)
    formData.append('printerName', printerName)
    formData.append('serverHost', serverHost)
    formData.append('serverPort', serverPort.toString())
    
    const response = await api.api.post('/api/Print', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })
    
    return response.data
  } catch (error) {
    console.error('Print job error:', error)
    throw error
  }
} 