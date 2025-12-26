import axiosClient from './axiousClient'
import type { ServiceResponse } from './types'

// Types
export interface CategoryDto {
  id: string
  name: string
  slug: string
  description: string | null
  parentCategoryId: string | null
}

export interface TagDto {
  id: string
  name: string
  slug: string
  description: string | null
}

export interface CreateCategoryRequest {
  name: string
  description?: string | null
  parentCategoryId?: string | null
}

export interface UpdateCategoryRequest {
  name: string
  description?: string | null
  parentCategoryId?: string | null
}

export interface CreateTagRequest {
  name: string
  description?: string | null
}

export interface UpdateTagRequest {
  name: string
  description?: string | null
}

// Categories API
export const categoriesApi = {
  getAll: async (parentCategoryId?: string, topLevelOnly?: boolean): Promise<ServiceResponse<CategoryDto[]>> => {
    const params = new URLSearchParams()
    if (parentCategoryId) params.append('parentCategoryId', parentCategoryId)
    if (topLevelOnly) params.append('topLevelOnly', 'true')
    const query = params.toString()
    const response = await axiosClient.get<ServiceResponse<CategoryDto[]>>(`/categories${query ? `?${query}` : ''}`)
    return response.data
  },

  getById: async (id: string): Promise<ServiceResponse<CategoryDto>> => {
    const response = await axiosClient.get<ServiceResponse<CategoryDto>>(`/categories/${id}`)
    return response.data
  },

  getBySlug: async (slug: string): Promise<ServiceResponse<CategoryDto>> => {
    const response = await axiosClient.get<ServiceResponse<CategoryDto>>(`/categories/slug/${slug}`)
    return response.data
  },

  create: async (data: CreateCategoryRequest): Promise<ServiceResponse<string>> => {
    const response = await axiosClient.post<ServiceResponse<string>>('/categories', data)
    return response.data
  },

  update: async (id: string, data: UpdateCategoryRequest): Promise<ServiceResponse<void>> => {
    const response = await axiosClient.put<ServiceResponse<void>>(`/categories/${id}`, data)
    return response.data
  },

  delete: async (id: string): Promise<ServiceResponse<void>> => {
    const response = await axiosClient.delete<ServiceResponse<void>>(`/categories/${id}`)
    return response.data
  }
}

// Tags API
export const tagsApi = {
  getAll: async (): Promise<ServiceResponse<TagDto[]>> => {
    const response = await axiosClient.get<ServiceResponse<TagDto[]>>('/tags')
    return response.data
  },

  getById: async (id: string): Promise<ServiceResponse<TagDto>> => {
    const response = await axiosClient.get<ServiceResponse<TagDto>>(`/tags/${id}`)
    return response.data
  },

  getBySlug: async (slug: string): Promise<ServiceResponse<TagDto>> => {
    const response = await axiosClient.get<ServiceResponse<TagDto>>(`/tags/slug/${slug}`)
    return response.data
  },

  create: async (data: CreateTagRequest): Promise<ServiceResponse<string>> => {
    const response = await axiosClient.post<ServiceResponse<string>>('/tags', data)
    return response.data
  },

  update: async (id: string, data: UpdateTagRequest): Promise<ServiceResponse<void>> => {
    const response = await axiosClient.put<ServiceResponse<void>>(`/tags/${id}`, data)
    return response.data
  },

  delete: async (id: string): Promise<ServiceResponse<void>> => {
    const response = await axiosClient.delete<ServiceResponse<void>>(`/tags/${id}`)
    return response.data
  }
}
