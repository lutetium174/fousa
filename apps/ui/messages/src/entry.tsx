/* @refresh reload */
import { mount } from '.'
import './index.css'

const root = document.getElementById('root')

mount(root!, {
  basePath: "/",
  eventBus: {} as any,
  globalContext: {} as any,
  runtimeVersion: "1.0.0",
  routeParams: undefined
}).then(() => {
  console.log("Test environment loaded successfully")
}).catch((error) => {
  console.error("Failed to load test environment", error)
})