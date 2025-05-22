import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
// Rename imported functions
import {
  generateAndPrintPdf,
  generateAndPrintBatchPdf,
  generateAndPrintRandomPdf,
} from '@/services/pdf-service'
import type { PdfFields, BatchPdfFields, RandomPdfFields } from '@/types/pdf'
import { useWebSocketStore } from '@/stores/webSocketStore'

// Rename store for clarity (optional)
export const usePdfStore = defineStore('pdf', () => {
  async function createAndPrintPdf(data: PdfFields) {
    const ws = useWebSocketStore()
    try {
      const result = await generateAndPrintPdf({ ...data, User: ws.username })
      console.log('Print job submitted:', result?.data)
      return result
    } catch (error) {
      console.error('Failed to submit PDF for printing:', error)
      return null
    }
  }

  async function createAndPrintBatchPdf(data: BatchPdfFields) {
    const ws = useWebSocketStore()
    try {
      // Call the renamed service function
      const result = await generateAndPrintBatchPdf({ ...data, User: ws.username })
      console.log('Batch print job submitted:', result?.data)
      return result
    } catch (error) {
      console.error('Failed to submit batch PDF for printing:', error)
      return null
    }
  }

  async function createAndPrintRandomPdf(randomFields: RandomPdfFields) {
    const ws = useWebSocketStore()
    try {
      const result = await generateAndPrintRandomPdf(randomFields)
      console.log('Random print job submitted:', result?.data)
      return result
    } catch (error) {
      console.error('Failed to submit random PDF for printing:', error)
      return null
    }
  }

  return {
    createAndPrintPdf, // Renamed
    createAndPrintBatchPdf, // Renamed
    createAndPrintRandomPdf, // Renamed
  }
})
