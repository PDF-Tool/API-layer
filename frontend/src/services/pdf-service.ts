import Api from '@/plugins/axios'
import type { BatchPdfFields, PdfFields, RandomPdfFields } from '@/types/pdf' // Assuming RandomPdfFields type exists

const api = new Api()

// Rename functions to match API endpoints
export async function generateAndPrintPdf(body: PdfFields) {
  try {
    // Call the new endpoint
    const response = await api.post('/PDFGenerator/GenerateAndPrint', body)
    return response // Response contains ProcessId and initial status
  } catch (error) {
    console.error('Error submitting print job:', error)
    throw error // Re-throw to be caught in the store
  }
}

export async function generateAndPrintBatchPdf(body: BatchPdfFields) {
  try {
    // Call the new endpoint
    const response = await api.post('/PDFGenerator/GenerateBatchAndPrint', body)
    return response
  } catch (error) {
    console.error('Error submitting batch print job:', error)
    throw error
  }
}

export async function generateAndPrintRandomPdf(body: RandomPdfFields) {
  // Use specific type if available
  try {
    // Call the new endpoint
    const response = await api.post('/PDFGenerator/GenerateRandomAndPrint', body)
    return response
  } catch (error) {
    console.error('Error submitting random print job:', error)
    throw error
  }
}
