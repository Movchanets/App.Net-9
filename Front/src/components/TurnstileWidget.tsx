/// <reference types="vite/client" />
// Turnstile widget wrapper using `react-turnstile`.
// Uses `import.meta.env.VITE_TURNSTILE_SITEKEY` so Vite replaces the value at build/dev time.

import React, { Suspense } from 'react'

// Lazy-load the react-turnstile component. We provide both `sitekey` and
// `callback`/`onVerify` props to be compatible with different wrapper
// implementations. The `react-turnstile` package typings are declared in
// src/types/react-turnstile.d.ts (minimal `any` shim).

const TurnstileLazy = React.lazy(() => import('react-turnstile'))

export default function TurnstileWidget({
  siteKey,
  onVerify,
}: {
  siteKey?: string
  onVerify?: (token: string) => void
}) {
  // Read the Vite env var directly so the bundler replaces it at build time.
  const key = siteKey || import.meta.env.VITE_TURNSTILE_SITEKEY
  if (!key) {
    // Render a visible diagnostic so developers know why widget is empty.
    // This avoids silent failures when the VITE_TURNSTILE_SITEKEY is not set.
    // The message also appears in console to make debugging easier.
  console.warn('Turnstile site key is not configured. Set VITE_TURNSTILE_SITEKEY in your environment to enable the widget.')
    return (
      <div className="rounded-md border border-yellow-300 bg-yellow-50 px-3 py-2 text-sm text-yellow-800">
        Turnstile not configured — set <code>VITE_TURNSTILE_SITEKEY</code> in your frontend env to enable the widget.
      </div>
    )
  }

  return (
    <Suspense fallback={null}>
      {/* pass both prop names (sitekey + siteKey) and both callbacks to be resilient */}
      {/* @ts-ignore allow unknown props to reach the wrapper */}
      <TurnstileLazy sitekey={key} siteKey={key} callback={onVerify} onVerify={onVerify} />
    </Suspense>
  )
}
