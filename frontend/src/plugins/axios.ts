import axios from 'axios'
import type { AxiosInstance, AxiosResponse, AxiosError } from 'axios'

import { toast } from 'vue-sonner'

// standard API response structure
interface ApiResponse<T = any> {
  status: boolean
  data?: T
  message?: string
  ProcessId?: string
}

export default class Api {
  api: AxiosInstance

  constructor(withCredentials?: boolean) {
    this.api = axios.create({
      baseURL: import.meta.env.VITE_BACKEND_SERVER_URL + '/api',
      headers: {
        'Content-Type': 'application/json',
        Accept: 'application/json',
      },
    })

    if (withCredentials) {
      this.api.defaults.withCredentials = true
    }
  }

  // --- Helper to extract error message ---
  private getErrorMessage(error: any): string {
    if (axios.isAxiosError(error)) {
      const axiosError = error as AxiosError<ApiResponse> // Type the error response
      // Prioritize message from API error response data
      if (axiosError.response?.data?.message) {
        return axiosError.response.data.message
      }
      // Fallback to generic Axios error message
      return axiosError.message
    }
    // Fallback for non-Axios errors
    return error?.message || 'An unknown error occurred.'
  }

  async get(url: string, config?: any): Promise<AxiosResponse<ApiResponse> | undefined> {
    // Add return type promise
    try {
      const response: AxiosResponse<ApiResponse> = await this.api.get(url, config)

      if (response.data.status === true) {
        return response
      } else {
        // API returned status: false (handled failure)
        const errorMessage = response.data.message || `Request failed: ${url}`
        console.warn(`API Handled Failure (GET ${url}):`, errorMessage, response.data)
        toast.warning(errorMessage) // Use warning for handled failures
        return response
      }
    } catch (error: any) {
      const errorMessage = this.getErrorMessage(error)
      console.error(
        `Network/Server Error (GET ${url}):`,
        errorMessage,
        error.response?.data || error,
      )
      toast.error(errorMessage)
      return undefined
    }
  }

  async post(
    url: string,
    data: any,
    config?: any,
  ): Promise<AxiosResponse<ApiResponse> | undefined> {
    try {
      const response: AxiosResponse<ApiResponse> = await this.api.post(url, data, config)

      if (response.data.status === true) {
        return response // Return the successful response object
      } else {
        const errorMessage = response.data.message || `Request failed: ${url}`
        console.warn(`API Handled Failure (POST ${url}):`, errorMessage, response.data)
        toast.warning(errorMessage) // Use warning for handled failures
        return response // Or throw?
      }
    } catch (error: any) {
      const errorMessage = this.getErrorMessage(error)
      console.error(
        `Network/Server Error (POST ${url}):`,
        errorMessage,
        error.response?.data || error,
      )
      toast.error(errorMessage)
      return undefined
    }
  }

  async patch(
    url: string,
    data: any,
    config?: any,
  ): Promise<AxiosResponse<ApiResponse> | undefined> {
    try {
      const response: AxiosResponse<ApiResponse> = await this.api.patch(url, data, config)

      if (response.data.status === true) {
        return response
      } else {
        const errorMessage = response.data.message || `Request failed: ${url}`
        console.warn(`API Handled Failure (PATCH ${url}):`, errorMessage, response.data)
        toast.warning(errorMessage)
        return response // Or throw?
      }
    } catch (error: any) {
      const errorMessage = this.getErrorMessage(error)
      console.error(
        `Network/Server Error (PATCH ${url}):`,
        errorMessage,
        error.response?.data || error,
      )
      // --- Pass ONLY the string message to toast.error ---
      toast.error(errorMessage)
      return undefined // Or throw error;
    }
  }

  async delete(url: string, config?: any): Promise<AxiosResponse<ApiResponse> | undefined> {
    // Add return type promise
    try {
      const response: AxiosResponse<ApiResponse> = await this.api.delete(url, config)

      // --- CORRECT CHECK: Check for boolean true ---
      if (response.data.status === true) {
        return response
      } else {
        const errorMessage = response.data.message || `Request failed: ${url}`
        console.warn(`API Handled Failure (DELETE ${url}):`, errorMessage, response.data)
        toast.warning(errorMessage)
        return response // Or throw?
      }
    } catch (error: any) {
      const errorMessage = this.getErrorMessage(error)
      console.error(
        `Network/Server Error (DELETE ${url}):`,
        errorMessage,
        error.response?.data || error,
      )
      // --- Pass ONLY the string message to toast.error ---
      toast.error(errorMessage)
      return undefined // Or throw error;
    }
  }
}
