import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import { makePdf, makeBatchPdf, makeRandomPdf } from '@/services/pdf-service'
import type { PdfFields, BatchPdfFields } from '@/types/pdf'
import { useWebSocketStore } from '@/stores/webSocketStore'

export const usePdfStore = defineStore('counter', () => {
  const formData = ref<PdfFields>({
    pages: 1,
    sizePerPage: 0,
    byteUnit: 'MB',
    width: undefined,
    height: undefined,
    format: 'A4',
    metricsUnit: 'mm',
  })

  const batchFormData = ref<BatchPdfFields>({
    numberOfFiles: 1,
    pagesPerFile: 1,
    sizePerPage: 0,
    byteUnit: 'MB',
    metricUnit: 'mm',
  })


  async function createPdf() {
    const ws = useWebSocketStore()
    const result = await makePdf({ ...formData.value, User: ws.username })
    return result
  }

  async function createBatchPdf() {
    const ws = useWebSocketStore()
    const result = await makeBatchPdf({ ...batchFormData.value, User: ws.username })
    return result
  }

  async function createRandomPdf(randomFields: any) {
    const ws = useWebSocketStore()
    const result = await makeRandomPdf({ ...randomFields, User: ws.username })
    return result
  }

  return { createPdf, formData, createBatchPdf, batchFormData, createRandomPdf }
})
