/* @refresh reload */
import './index.css'
import {mount} from "./index.tsx";

const root = document.getElementById('root')

mount(root!, {
  basePath: "/",
  eventBus: {},
  globalContext: {},
  runtimeVersion: "1.0.0",
  routeParams: undefined
}).then(() => {
  console.log("Test environment loaded successfully")
}).catch((error) => {
  console.error("Failed to load test environment", error)
})