import Api from '@/plugins/axios'
import type { BatchPdfFields, PdfFields } from '@/types/pdf'

const api = new Api()

export async function makePdf(body: PdfFields) {
  try {
    const response = await api.post('/GenerateStart', body)
    return response
  } catch (error) {
    console.error(error)
  }
}

export async function makeBatchPdf(body: BatchPdfFields) {
  try {
    const response = await api.post('/GenerateBatchStart', body)
    return response
  } catch (error) {
    console.error(error)
  }
}