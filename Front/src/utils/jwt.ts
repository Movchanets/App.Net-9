// Utilities for parsing JWT payloads and normalizing claim arrays
export function parseJwt(token: string): Record<string, unknown> | null {
  try {
    const parts = token.split('.')
    if (parts.length !== 3) return null
    const payload = parts[1]
    // base64url -> base64
    const base64 = payload.replace(/-/g, '+').replace(/_/g, '/')
    const padLen = (4 - (base64.length % 4)) % 4
    const padded = base64 + '='.repeat(padLen)
    const binary = typeof atob !== 'undefined' ? atob(padded) : ''

    // Convert binary string to Uint8Array then decode as UTF-8
    const bytes = new Uint8Array(binary.length)
    for (let i = 0; i < binary.length; i++) {
      bytes[i] = binary.charCodeAt(i)
    }

    const text = typeof TextDecoder !== 'undefined'
      ? new TextDecoder().decode(bytes)
      : decodeURIComponent(escape(binary))

    return JSON.parse(text)
  } catch {
    return null
  }
}

export function toArray(v: unknown): string[] {
  if (!v) return []
  if (Array.isArray(v)) return (v as unknown[]).map((item) => String(item))
  return [String(v)]
}
