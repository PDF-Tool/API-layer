import Api from '@/plugins/axios'
import type { FdfFields } from '@/types/pdf'

const api = new Api()

export async function makePdf(body: FdfFields) {
  try {
    const response = await api.post('/response', body)
    return response
  } catch (error) {
    console.error(error)
  }
}
