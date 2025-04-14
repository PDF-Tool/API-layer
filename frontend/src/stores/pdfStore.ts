import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import { makePdf } from '@/services/pdf-service'
import type { FdfFields } from '@/types/pdf'

export const usePdfStore = defineStore('counter', () => {
  const formData = ref<FdfFields>({
    pages: 1,
    size: 0,
    byteUnit: 'MB',
    width: undefined,
    height: undefined,
    format: 'A4',
    metricUnit: 'mm',
  })

  async function createPdf() {
    const result = await makePdf(formData.value)
    return result
  }

  return { createPdf, formData }
})
