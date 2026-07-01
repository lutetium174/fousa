import {defineConfig} from 'vite'
import solid from 'vite-plugin-solid'

export default defineConfig({
    plugins: [solid()], build: {
        lib: {
            entry: 'src/index.tsx',
            formats: ['es'],
            fileName: () => 'index.js'
        },
        rollupOptions: {
            external: ['solid-js'],
            output: {
                globals: {
                    'solid-js': 'Solid'
                }
            }
        }
    }
})
