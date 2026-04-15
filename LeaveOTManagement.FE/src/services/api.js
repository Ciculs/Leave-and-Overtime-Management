import axios from "axios"
import router from "@/router"

const api = axios.create({
  baseURL: "https://localhost:7121/api",
  timeout: 10000
})

let isRedirecting = false

/* =========================
   Request Interceptor
   Tự động gửi JWT token
========================= */
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("token")

    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }

    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

/* =========================
   Response Interceptor
   Xử lý 401 / 403
========================= */
api.interceptors.response.use(
  (response) => {
    return response
  },
  (error) => {
    if (error.response) {
      const status = error.response.status
      const url = error.config?.url || ""

      // Bỏ qua 401 của API login
      if (url.toLowerCase().includes("/auth/login")) {
        return Promise.reject(error)
      }

      // Token hết hạn / chưa login
      if (status === 401 && !isRedirecting) {
        isRedirecting = true

        window.$toast?.("Session expired. Please log in again.", "error")

        localStorage.clear()

        setTimeout(() => {
          router.push("/login")
        }, 800)

        return Promise.reject(error)
      }

      // Không có quyền
      if (status === 403) {
        window.$toast?.("You do not have permission to access this page.", "error")
      }
    }

    return Promise.reject(error)
  }
)

export default api