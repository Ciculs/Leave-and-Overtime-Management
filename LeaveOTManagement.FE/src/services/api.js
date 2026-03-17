import axios from "axios"
import router from "@/router"

const api = axios.create({
  baseURL: "https://localhost:7121/api"
})

/* =========================
   Request Interceptor
   Tự động gửi JWT token
========================= */

api.interceptors.request.use((config) => {

  const token = localStorage.getItem("token")

  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  return config

})


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

      // Token hết hạn / chưa login
      if (status === 401) {

        alert("Session expired. Please login again.")

        localStorage.removeItem("token")
        localStorage.removeItem("role")

        router.push("/login")
      }

      // Không có quyền
      if (status === 403) {

        alert("You do not have permission to access this page.")
      }

    }

    return Promise.reject(error)
  }

)

export default api