import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
// Rename imported functions
import { generateAndPrintPdf, generateAndPrintBatchPdf, generateAndPrintRandomPdf } from '@/services/pdf-service'
import type { PdfFields, BatchPdfFields, RandomPdfFields } from '@/types/pdf'
import { useWebSocketStore } from '@/stores/webSocketStore'

// Rename store for clarity (optional)
export const usePdfStore = defineStore('pdf', () => {
  const formData = ref<PdfFields>({
    Pages: 1,
    SizePerPage: 1, // Default to 1 instead of 0
    ByteUnit: 'MB',
    Format: 'A4',
    MetricUnit: 'mm',
  })

  const batchFormData = ref<BatchPdfFields>({
    NumberOfFiles: 1,
    PagesPerFile: 1,
    SizePerPage: 1, // Default to 1
    ByteUnit: 'MB',
    MetricUnit: 'mm',
    // Add format, width, height if needed by batch model, currently unused by backend logic
  })

  // --- Rename actions ---
  async function createAndPrintPdf() {
    const ws = useWebSocketStore()
    try {
        // Call the renamed service function
        const result = await generateAndPrintPdf({ ...formData.value, User: ws.username })
        // Optional: Handle initial response (e.g., show ProcessId)
        console.log('Print job submitted:', result?.data);
        return result;
    } catch (error) {
         console.error('Failed to submit PDF for printing:', error);
         // Optional: Show error to user
         return null;
    }
  }

  async function createAndPrintBatchPdf() {
    const ws = useWebSocketStore()
     try {
        // Call the renamed service function
        const result = await generateAndPrintBatchPdf({ ...batchFormData.value, User: ws.username })
        console.log('Batch print job submitted:', result?.data);
        return result;
     } catch (error) {
         console.error('Failed to submit batch PDF for printing:', error);
         return null;
    }
  }

   async function createAndPrintRandomPdf(randomFields: any) {
    const ws = useWebSocketStore()
    try {
      const mappedFields: RandomPdfFields = {
        SizeMin: randomFields.sizeMin,
        SizeMax: randomFields.sizeMax,
        PageMin: randomFields.pagesMin,
        PageMax: randomFields.pagesMax,
        Mode: randomFields.mode,
        NumberOfFiles: randomFields.numberOfFiles,
        ByteUnit: randomFields.byteUnit,
        MetricUnit: randomFields.metricUnit,
        User: ws.username
      }
      const result = await generateAndPrintRandomPdf(mappedFields)
      console.log('Random print job submitted:', result?.data)
      return result
    } catch (error) {
      console.error('Failed to submit random PDF for printing:', error)
      return null
    }
  }

  // --- Update return object ---
  return {
      formData,
      batchFormData,
      createAndPrintPdf, // Renamed
      createAndPrintBatchPdf, // Renamed
      createAndPrintRandomPdf // Renamed
    }
})