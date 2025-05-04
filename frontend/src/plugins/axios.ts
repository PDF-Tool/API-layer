import axios from 'axios';
import type { AxiosInstance, AxiosResponse, AxiosError } from 'axios';

import { toast } from 'vue-sonner'

// standard API response structure
interface ApiResponse<T = any> {
    status: boolean;
    data?: T;
    message?: string;
    ProcessId?: string;
}

export default class Api {
  api: AxiosInstance;

  constructor(withCredentials?: boolean) {
    this.api = axios.create({ 
        baseURL: import.meta.env.VITE_BACKEND_SERVER_URL + '/api',
        headers: {
          'Content-Type': 'application/json',
          Accept: 'application/json',
        }
    });

    if (withCredentials) {
      this.api.defaults.withCredentials = true;
    }
  }


  // --- Helper to extract error message ---
  private getErrorMessage(error: any): string {
      if (axios.isAxiosError(error)) {
          const axiosError = error as AxiosError<ApiResponse>; // Type the error response
          // Prioritize message from API error response data
          if (axiosError.response?.data?.message) {
              return axiosError.response.data.message;
          }
          // Fallback to generic Axios error message
          return axiosError.message;
      }
      // Fallback for non-Axios errors
      return error?.message || 'An unknown error occurred.';
  }

  async get(url: string, config?: any): Promise<AxiosResponse<ApiResponse> | undefined> { // Add return type promise
    try {
      const response: AxiosResponse<ApiResponse> = await this.api.get(url, config);

      // --- CORRECT CHECK: Check for boolean true ---
      if (response.data.status === true) {
        // Success case (optional: maybe show success toast?)
        // toast.success(response.data.message || 'Request successful');
        return response;
      } else {
        // API returned status: false (handled failure)
        const errorMessage = response.data.message || `Request failed: ${url}`;
        console.warn(`API Handled Failure (GET ${url}):`, errorMessage, response.data);
        toast.warning(errorMessage); // Use warning for handled failures
        // Decide whether to return the response or throw/return undefined
        return response; // Or maybe throw new Error(errorMessage);
      }
    } catch (error: any) {
      const errorMessage = this.getErrorMessage(error);
      console.error(`Network/Server Error (GET ${url}):`, errorMessage, error.response?.data || error);
      // --- Pass ONLY the string message to toast.error ---
      toast.error(errorMessage);
      // Decide how to propagate the error
      // Option 1: Return undefined (or null)
      return undefined;
      // Option 2: Rethrow if the caller should handle it
      // throw error;
      // Option 3: Return the error response (less ideal usually)
      // return error?.response;
    }
  }

  async post(url: string, data: any, config?: any): Promise<AxiosResponse<ApiResponse> | undefined> { // Add return type promise
    try {
      const response: AxiosResponse<ApiResponse> = await this.api.post(url, data, config);

      // --- CORRECT CHECK: Check for boolean true ---
      if (response.data.status === true) {
        // Success case (your print job submission)
        // No need to toast success here unless desired
        return response; // Return the successful response object
      } else {
        // API returned status: false (handled failure)
        const errorMessage = response.data.message || `Request failed: ${url}`;
        console.warn(`API Handled Failure (POST ${url}):`, errorMessage, response.data);
        toast.warning(errorMessage); // Use warning for handled failures
        return response; // Or throw?
      }
    } catch (error: any) {
      const errorMessage = this.getErrorMessage(error);
      console.error(`Network/Server Error (POST ${url}):`, errorMessage, error.response?.data || error);
      // --- Pass ONLY the string message to toast.error ---
      toast.error(errorMessage);
      return undefined; // Or throw error;
    }
  }

  async patch(url: string, data: any, config?: any): Promise<AxiosResponse<ApiResponse> | undefined> { // Add return type promise
    try {
       const response: AxiosResponse<ApiResponse> = await this.api.patch(url, data, config);

      // --- CORRECT CHECK: Check for boolean true ---
      if (response.data.status === true) {
         return response;
      } else {
        const errorMessage = response.data.message || `Request failed: ${url}`;
        console.warn(`API Handled Failure (PATCH ${url}):`, errorMessage, response.data);
        toast.warning(errorMessage);
        return response; // Or throw?
      }
    } catch (error: any) {
      const errorMessage = this.getErrorMessage(error);
      console.error(`Network/Server Error (PATCH ${url}):`, errorMessage, error.response?.data || error);
       // --- Pass ONLY the string message to toast.error ---
      toast.error(errorMessage);
      return undefined; // Or throw error;
    }
  }

  async delete(url: string, config?: any): Promise<AxiosResponse<ApiResponse> | undefined> { // Add return type promise
    try {
      const response: AxiosResponse<ApiResponse> = await this.api.delete(url, config);

      // --- CORRECT CHECK: Check for boolean true ---
      if (response.data.status === true) {
         return response;
      } else {
        const errorMessage = response.data.message || `Request failed: ${url}`;
        console.warn(`API Handled Failure (DELETE ${url}):`, errorMessage, response.data);
        toast.warning(errorMessage);
        return response; // Or throw?
      }
    } catch (error: any) {
      const errorMessage = this.getErrorMessage(error);
      console.error(`Network/Server Error (DELETE ${url}):`, errorMessage, error.response?.data || error);
       // --- Pass ONLY the string message to toast.error ---
      toast.error(errorMessage);
      return undefined; // Or throw error;
    }
  }
}