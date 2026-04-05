/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        brand: {
          DEFAULT: '#E85D04',
          light: '#FF8C42',
          dark: '#C44D03',
        },
        dark: {
          DEFAULT: '#1A1A2E',
          mid: '#16213E',
        },
      },
    },
  },
  plugins: [],
}
