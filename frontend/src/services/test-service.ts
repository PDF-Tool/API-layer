import axios from 'axios'

// This file is only for debugging purposes
console.log('Base URL check:')
console.log('Environment variable:', import.meta.env.VITE_BACKEND_SERVER_URL)
console.log('Direct API test to correct endpoint')

// Try a direct axios call instead of using the wrapper
export async function testDirectApiCall() {
  try {
    const formData = new FormData()
    const blob = new Blob(['Test PDF'], { type: 'application/pdf' })
    const file = new File([blob], 'test.pdf', { type: 'application/pdf' })
    formData.append('file', file)
    formData.append('printerName', 'default')
    
    console.log('Making direct API call to: http://localhost:5091/api/Print')
    const response = await axios.post('http://localhost:5091/api/Print', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })
    
    console.log('Direct API call succeeded:', response)
    return response
  } catch (error) {
    console.error('Direct API call error:', error)
    throw error
  }
}

// Execute test right away
testDirectApiCall().catch(err => console.error('Test failed:', err)) 