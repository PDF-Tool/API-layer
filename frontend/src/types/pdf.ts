import { type PageFormat } from '@/lib/pageFormat'
import { type FileSizeFormat } from '@/lib/utils'

export interface PdfFields {
  Pages?: number
  SizePerPage: number
  ByteUnit?: FileSizeFormat
  Square?: number
  Width?: number
  Height?: number
  User?: string
  Duration?: number
  Host: string
}

export interface BatchPdfFields extends PdfFields {
  NumberOfFiles: number
  PagesPerFile: number
}
export interface PerformancePdfFields extends PdfFields {
  Duration: number
  PagesPerFile: number
}

export interface RandomPdfFields extends PdfFields {
  SizeMin: number
  SizeMax: number
  PageMin: number
  PageMax: number
  Mode: string
  NumberOfFiles: number
  ByteUnit: FileSizeFormat
  User?: string
}
