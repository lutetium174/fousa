import { defineConfig } from 'vite';
import solid from 'vite-plugin-solid';
import path from 'path';
export default defineConfig({
    plugins: [solid()],
    server: {
        middlewareMode: false,
        hmr: true,
        port: 5174,
        // Enable CORS for cross-origin loading from the main MFE
        cors: {
            origin: [
                'http://localhost:5173',
                'http://127.0.0.1:5173',
                'http://localhost:5174',
                'http://127.0.0.1:5174',
            ],
            methods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS'],
            allowedHeaders: ['*'],
            credentials: true,
        },
        // Serve the source files for development
        fs: {
            // Allow serving files from outside the root directory
            allow: ['..'],
        },
    },
    resolve: {
        alias: {
            '~': '/src',
            // Ensure we can resolve the components properly
            components: path.resolve(__dirname, '../components/src'),
        },
    },
    build: {
        lib: {
            entry: 'src/viewer.tsx',
            name: 'Messages',
            fileName: () => 'index.js',
            formats: ['es'],
        },
        // Ensure components can be loaded individually
        rollupOptions: {
            input: {
                viewer: 'src/viewer.tsx',
                components: 'src/components/index.ts',
            },
            external: ['solid-js', 'components'],
            output: {
                entryFileNames: '[name].js',
                globals: {
                    'solid-js': 'Solid',
                    'components': 'Components',
                },
            },
        },
    },
});
