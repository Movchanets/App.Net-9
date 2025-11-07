import type { Config } from 'tailwindcss'

export default {
  content: [
    './index.html',
    './src/**/*.{ts,tsx}',
  ],
  theme: {
    extend: {
      colors: {
        brand: {
          DEFAULT: '#0ea5e9',
          dark: '#0284c7',
          light: '#38bdf8',
        },
        surface: {
          DEFAULT: '#0f172a',
          card: '#111827',
        },
        text: {
          DEFAULT: '#e2e8f0',
          muted: '#94a3b8',
        },
      },
      fontFamily: {
        brand: ['Inter', 'system-ui', 'Avenir', 'Helvetica', 'Arial', 'sans-serif'],
      },
      boxShadow: {
        card: '0 10px 25px -5px rgba(0,0,0,0.35), 0 8px 10px -6px rgba(0,0,0,0.25)',
      },
      container: {
        center: true,
        padding: {
          DEFAULT: '1rem',
          lg: '2rem',
        },
      },
    },
  },
  plugins: [],
} satisfies Config


