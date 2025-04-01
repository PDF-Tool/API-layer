import { type ClassValue, clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export const fileSizeFormats = ['MB', 'GB', 'TB'] as const

export type FileSizeFormat = (typeof fileSizeFormats)[number]
