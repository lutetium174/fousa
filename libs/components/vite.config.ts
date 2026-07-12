import { defineConfig } from "vite";
import dts from "vite-plugin-dts";
import solid from "vite-plugin-solid";
import path from "path";
import cssInjectedByJsPlugin from "vite-plugin-css-injected-by-js";

export default defineConfig({
  root: process.cwd(),
  plugins: [
    solid(),
    cssInjectedByJsPlugin(),
    dts({
      insertTypesEntry: true,
      include: ["src"],
    }),
  ],
  build: {
    target: "esnext",
    sourcemap: true,
    cssCodeSplit: false,
    lib: {
      entry: path.resolve(__dirname, "src/index.ts"),
      name: "Components",
      formats: ["es", "cjs"],
      fileName: (format) => `components.${format}.js`,
    },
    rollupOptions: {
      external: ["solid-js", "solid-js/web"],
      output: {
        globals: {
          "solid-js": "Solid",
          "solid-js/web": "SolidWeb",
          assetFileNames: "assets/[name].[ext]",
        },
      },
    },
  },
  css: {
    modules: {
      localsConvention: "dashes",
    },
  },
  resolve: {
    alias: {
      components: path.resolve(__dirname, "src"),
    },
  },
  optimizeDeps: {
    exclude: ["solid-js"],
  },
});
