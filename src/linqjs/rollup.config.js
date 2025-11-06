import { terser } from "rollup-plugin-terser";

export default {
  input: 'index.js',
  output: {
    file: 'dist/bundle.min.js',
    format: 'esm', // veya 'iife' (immediate invocation function expression)
  },
  plugins: [terser()],
};