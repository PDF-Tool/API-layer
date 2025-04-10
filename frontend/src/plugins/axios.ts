import axios from 'axios'
import { toast } from 'vue-sonner'

export default class Api {
  api: any

  constructor(withCredentials?: boolean) {
    this.api = axios

    this.api.defaults.baseURL = import.meta.env.VITE_BACKEND_SERVER_URL
    this.api.defaults.headers = {
      'Content-Type': 'application/json',
      Accept: 'application/json',
    }

    if (withCredentials) {
      this.api.defaults.withCredentials = true
    }
  }

  setHeaders(key: string, value: string) {
    this.api.defaults.headers[key] = value
  }

  resetHeaders() {
    this.api.defaults.headers = {
      'Content-Type': 'application/json',
      Accept: 'application/json',
    }
  }

  async get(url: string, data?: any) {
    try {
      const response = await this.api.get(url, data)
      if (response.data.status === 'success') {
        return response
      } else {
        console.group(`GET ${url}`)
        console.error(`GET response error: ${response.data.message}`)
        console.error(response.data)
        console.groupEnd()
        toast.error(response || 'An error occurred during GET request')
        return response
      }
    } catch (error: any) {
      console.error(error)
      toast.error(error || error?.message || 'Network error occurred')
      return error?.response
    }
  }

  async post(url: string, data: any) {
    try {
      const response = await this.api.post(url, data)
      if (response.data.status === 'success') {
        return response
      } else {
        console.group(`POST ${url}`)
        console.error(`POST response error: ${response.data.message}`)
        console.error(response.data)
        console.groupEnd()
        toast.error(response.data || 'An error occurred during POST request')
        return response
      }
    } catch (error: any) {
      console.error(error)
      toast.error(error.response.data || 'Network error occurred')
      return error?.response
    }
  }

  async patch(url: string, data: any) {
    try {
      const response = await this.api.patch(url, data)
      if (response.data.status === 'success') {
        return response
      } else {
        console.group(`PATCH ${url}`)
        console.error(`PATCH response error: ${response.data.message}`)
        console.error(response.data)
        console.groupEnd()
        toast.error(response || 'An error occurred during PATCH request')
        return response
      }
    } catch (error: any) {
      console.error(error)
      toast.error(error || 'Network error occurred')
      return error?.response
    }
  }

  async delete(url: string, data?: any) {
    try {
      const response = await this.api.delete(url, data)
      if (response.data.status === 'success') {
        return response
      } else {
        console.group(`DELETE ${url}`)
        console.error(`DELETE response error: ${response.data.message}`)
        console.error(response.data)
        console.groupEnd()
        toast.error(response.data.message || 'An error occurred during DELETE request')
        return response
      }
    } catch (error: any) {
      console.error(error)
      toast.error(error?.response?.data?.message || error?.message || 'Network error occurred')
      return error?.response
    }
  }
}
