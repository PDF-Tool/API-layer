import { type PageFormat } from '@/lib/pageFormat'
import { type FileSizeFormat } from '@/lib/utils'

export interface FdfFields {
  pages: number
  format?: PageFormat
  size: number
  byteUnit?: FileSizeFormat
  square?: number
  width?: number
  height?: number
  metricsUnit?: string
  User?: string
}
