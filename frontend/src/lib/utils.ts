import { type ClassValue, clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export const fileSizeFormats = ['MB', 'GB'] as const

export const metrics = ['mm', 'cm']

export type FileSizeFormat = (typeof fileSizeFormats)[number]
