export interface ServiceResponse<T = unknown> {
  isSuccess: boolean
  message: string
  payload?: T | null
}

export function unwrapServiceResponse<T>(res: ServiceResponse<T>): T {
  if (!res || !res.isSuccess) {
    throw new Error(res?.message || 'Service error')
  }
  return (res.payload as T)!
}
