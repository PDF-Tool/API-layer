
export const pageFormats = ['A4', 'A3', 'A2', 'A1', 'A0'] as const

export type PageFormat = (typeof pageFormats)[number]

