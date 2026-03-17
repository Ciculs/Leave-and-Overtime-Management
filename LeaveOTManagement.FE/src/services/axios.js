import axios from "axios"
import router from "../router"

const api = axios.create({
  baseURL: "https://localhost:7121/api"
})

// Gửi token vào mọi request
api.interceptors.request.use((config) => {

  const token = localStorage.getItem("token")

  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  return config
})


// Bắt lỗi response
api.interceptors.response.use(

  response => response,

  error => {

    if (error.response) {

      // Token hết hạn
      if (error.response.status === 401) {

        alert("Phiên đăng nhập đã hết hạn")

        localStorage.removeItem("token")

        router.push("/login")
      }

      // Không đủ quyền
      if (error.response.status === 403) {

        alert("Bạn không có quyền truy cập")
      }
    }

    return Promise.reject(error)
  }
)

export default api