import { type PageFormat } from '@/lib/pageFormat'
import { type FileSizeFormat } from '@/lib/utils'

export interface PdfFields {
  Pages: number
  Format?: PageFormat
  SizePerPage: number
  ByteUnit?: FileSizeFormat
  Square?: number
  Width?: number
  Height?: number
  MetricUnit?: string
  User?: string
}

export interface BatchPdfFields{
  NumberOfFiles: number
  PagesPerFile: number
  SizePerPage: number
  ByteUnit: FileSizeFormat
  MetricUnit: string
  User?: string
}

export interface RandomPdfFields{
  SizeMin: number
  SizeMax: number
  PageMin: number
  PageMax: number
  Mode: string
  NumberOfFiles: number
  ByteUnit: FileSizeFormat
  MetricUnit: string  
  User?: string
}