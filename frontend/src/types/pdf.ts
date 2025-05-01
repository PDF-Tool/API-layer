import { type PageFormat } from '@/lib/pageFormat'
import { type FileSizeFormat } from '@/lib/utils'

export interface PdfFields {
  pages: number
  format?: PageFormat
  sizePerPage: number
  byteUnit?: FileSizeFormat
  square?: number
  width?: number
  height?: number
  metricsUnit?: string
  User?: string
}

export interface BatchPdfFields{
  numberOfFiles: number
  pagesPerFile: number
  sizePerPage: number
  byteUnit: FileSizeFormat
  metricUnit: string
  User?: string
}